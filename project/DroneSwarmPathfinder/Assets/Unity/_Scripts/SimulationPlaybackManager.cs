using DroneSwarmPathfinder.Core.Environment;
using DroneSwarmPathfinder.Core.Models;
using DroneSwarmPathfinder.Core.Serialization;
using DroneSwarmPathfinder.Core.Simulation;
using DroneSwarmPathfinder.Unity.EditorTools;
using DroneSwarmPathfinder.Unity.Visuals;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace DroneSwarmPathfinder.Unity.Managers
{
    [RequireComponent(typeof(PathVisualizer))]
    public class SimulationPlaybackManager : MonoBehaviour
    {
        public static SimulationPlaybackManager instance;
        private PathVisualizer _pathVisualizer;

        private void Awake()
        {
            instance = this;
            _pathVisualizer = GetComponent<PathVisualizer>();
        }

        [Header("Playback config")]
        [Tooltip("Playback FPS")]
        public float playbackSpeed = 2f;
        public float defaultPlaybackSpeed = 2f;

        [Header("State (Read Only)")]
        [SerializeField] private bool _isPlaying = false;
        public bool isPlaying // Automatically calls OnPlaybackStateChanged to update UI
        {
            get => _isPlaying;
            private set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    OnPlaybackStateChanged?.Invoke(_isPlaying);
                }
            }
        }
        public float currentTime = 0f;
        public int maxSteps = 0;

        public event Action<bool> OnPlaybackStateChanged; // used for updating play button state

        private IReadOnlyDictionary<int, DronePath> _currentPaths;
        private SimulationResult _latestResult;
        public SimulationResult LatestResult { get =>  _latestResult; }

        private void Update()
        {
            if (isPlaying && _currentPaths != null)
            {
                currentTime += Time.deltaTime * playbackSpeed;
        
                // End of simulation by reaching step count
                if (currentTime >= maxSteps)
                {
                    currentTime = maxSteps;
                    isPlaying = false;
                    Debug.Log("End of simulation");
                }

                UpdateDronesPositions(currentTime);
            }
        }

        /// <summary>
        /// Ends the simulation and clears all the paths
        /// </summary>
        public void ClearSimulation()
        {
            isPlaying = false;
            _currentPaths = null;
            _pathVisualizer.ClearPaths();
            currentTime = 0f;
            maxSteps = 0;
        }

        /// <summary>
        /// Loads simulation results and prepares its simulation
        /// </summary>
        public void LoadSimulationResult(SimulationResult result)
        {
            LoadConfigForResults(result.SimulationContext.InitialState.Values, result.SimulationContext.Environment);

            _latestResult = result;
            _currentPaths = result.Paths;
            currentTime = 0f;
            maxSteps = 0;
            isPlaying = false;

            // Render the path lines
            _pathVisualizer.DrawPaths(_currentPaths);

            // Find longest path to know step count
            foreach (var path in _currentPaths.Values)
            {
                if (path.Waypoints.Count > 0)
                {
                    int lastStep = path.Waypoints[^1].StepIndex;
                    if (lastStep > maxSteps) maxSteps = lastStep;
                }
            }

            UpdateDronesPositions(0f);
        }
        /// <summary>
        /// Helper method to allow loading the results - first loads all the necessary drones
        /// </summary>
        private void LoadConfigForResults(IEnumerable<IDroneConfigItem> dronesConfig, WorldEnvironment envConfig)
        {
            Debug.Log($"Loading swarm configuration from selected start and environment configurations...");
            // Drone loading
            DroneManager.instance.ClearAndSpawnDrones(dronesConfig);
            ConfigEditorManager.instance.SetDroneSelectionFromUI(new List<int>());

            // Environment loading
            // Spawn the obstacles
            EnvironmentManager.instance.SpawnObstacles(envConfig.Obstacles);

            // Load the spatial rules (Grid)
            if (envConfig.SpatialRules is DiscreteGrid grid)
            {
                EnvironmentManager.instance.ChangeGridSize(grid.CellSize);
            }

            // Clear UI selection state
            ConfigEditorManager.instance.SetObstacleSelectionFromUI(new List<int>());
            Debug.Log("Swarm configuration and environment loaded successfully");
        }
        private void LoadConfigForResults(IEnumerable<IDroneConfigItem> dronesConfig, EnvironmentConfigJson envConfig) => LoadConfigForResults(dronesConfig, new WorldEnvironment(envConfig.SpatialRules, envConfig.Obstacles));

        #region Playback controls (API)

        public void Play() => isPlaying = true;
        public void Pause() => isPlaying = false;

        public void StepForward()
        {
            isPlaying = false; // (pause simulation when stepping)
            currentTime = Mathf.Min(Mathf.Floor(currentTime) + 1f, maxSteps);
            UpdateDronesPositions(currentTime);
        }

        public void StepBackward()
        {
            isPlaying = false;
            currentTime = Mathf.Max(Mathf.Ceil(currentTime) - 1f, 0f);
            UpdateDronesPositions(currentTime);
        }

        public void Restart()
        {
            isPlaying = false;
            currentTime = 0f;
            UpdateDronesPositions(currentTime);
        }

        /// <summary>
        /// Time speed slider (TODO: untested)
        /// </summary>
        public void SetTimeScale(float newSpeedPercentage)
        {
            playbackSpeed = defaultPlaybackSpeed * newSpeedPercentage;
            //currentTime = Mathf.Clamp(time, 0f, maxSteps);
            //UpdateDronesPositions(currentTime);
        }

        #endregion

        #region Position interpolation

        private void UpdateDronesPositions(float time)
        {
            if (_currentPaths == null) return;

            foreach (var kvp in _currentPaths)
            {
                int droneId = kvp.Key;
                DronePath path = kvp.Value;

                DroneView droneView = Managers.DroneManager.instance.GetDroneView(droneId);
                if (droneView == null || path.Waypoints.Count == 0) continue;

                TransformData interpolatedData = GetInterpolatedTransform(path.Waypoints, time);

                droneView.transform.position = interpolatedData.Position.ToUnity(); // conversion extension methods
                droneView.transform.rotation = interpolatedData.Rotation.ToUnity();
            }
        }

        private TransformData GetInterpolatedTransform(List<Waypoint> waypoints, float time)
        {
            // Invalid values check
            if (time <= waypoints[0].StepIndex)
                return new TransformData { Position = waypoints[0].Position, Rotation = waypoints[0].Rotation };
            if (time >= waypoints[^1].StepIndex)
                return new TransformData { Position = waypoints[^1].Position, Rotation = waypoints[^1].Rotation };

            // Find current waypoint
            // TODO: find a better way to do this (constant time steps? ...or at least binsearch)
            Waypoint wpA = waypoints[0];
            Waypoint wpB = waypoints[^1];

            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                if (time >= waypoints[i].StepIndex && time <= waypoints[i + 1].StepIndex)
                {
                    wpA = waypoints[i];
                    wpB = waypoints[i + 1];
                    break;
                }
            }

            float stepDuration = wpB.StepIndex - wpA.StepIndex;
            float fraction = (stepDuration == 0) ? 0 : (time - wpA.StepIndex) / stepDuration;

            return new TransformData
            {
                Position = System.Numerics.Vector3.Lerp(wpA.Position, wpB.Position, fraction),
                Rotation = System.Numerics.Quaternion.Slerp(wpA.Rotation, wpB.Rotation, fraction),
                Size = System.Numerics.Vector3.One
            };
        }

        #endregion
    }
}