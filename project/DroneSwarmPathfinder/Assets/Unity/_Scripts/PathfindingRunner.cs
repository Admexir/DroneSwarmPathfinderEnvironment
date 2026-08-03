using DroneSwarmPathfinder.Core.Environment;
using DroneSwarmPathfinder.Core.Models;
using DroneSwarmPathfinder.Core.Simulation;
using DroneSwarmPathfinder.Unity.Managers;
using DroneSwarmPathfinder.Unity.EditorTools;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using NumVector3 = System.Numerics.Vector3;

namespace DroneSwarmPathfinder.Unity.Simulation
{
    /// <summary>
    /// Class used for gathering scene data, running the pathfinding algorithms (asynchronously) and passing the results to the PlaybackManager
    /// </summary>
    public class PathfindingRunner : MonoBehaviour
    {
        public static PathfindingRunner instance;
        private void Awake() => instance = this;

        private CancellationTokenSource _cancellationTokenSource;

        public async Task RunAlgorithmAsync(IPathfindingAlgorithm algorithm)
        {
            if (algorithm == null) return;

            Debug.Log($"Starting {algorithm.AlgorithmName}...");

            // Get initial state
            var initialDrones = DroneManager.instance.AllDroneModels.ToDictionary(d => d.ID, d => d);

            // TODO: Let user define target config (currently still hardcoded)
            var targetDrones = new Dictionary<int, Drone>();
            if (initialDrones.ContainsKey(0) && initialDrones.ContainsKey(1))
            {
                targetDrones[0] = new Drone(0, new TransformData(new NumVector3(10, 0, 10)), 0);
                targetDrones[1] = new Drone(1, new TransformData(new NumVector3(0, 0, 0)), 1);
            }
            else
            {
                foreach (var kvp in initialDrones)
                {
                    targetDrones[kvp.Key] = kvp.Value;
                }
            }

            // Get environment
            float gridSize = ConfigEditorManager.instance != null ? ConfigEditorManager.instance.gridSize : 1f;
            var grid = new DiscreteGrid(gridSize);

            var obstacles = ObstacleManager.instance != null
                ? ObstacleManager.instance.AllObstacleModels
                : new List<BoxObstacle>();

            var worldEnv = new WorldEnvironment(grid, obstacles);

            var context = new SimulationContext
            {
                InitialState = initialDrones,
                TargetState = targetDrones,
                Environment = worldEnv
            };

            // Run algorithm async
            _cancellationTokenSource = new CancellationTokenSource();

            // TODO: add a progress bar
            var progress = new System.Progress<float>(p => Debug.Log($"Calculating... {p * 100:F0}%"));

            SimulationResult result = await algorithm.CalculatePathsAsync(context, progress, _cancellationTokenSource.Token);

            // Load results
            if (result.IsSuccessful)
            {
                Debug.Log($"Pathfinding successful, computed in {result.ComputationTime.TotalMilliseconds} ms");
                SimulationPlaybackManager.instance.LoadSimulationResult(result);
                SimulationPlaybackManager.instance.Play();
            }
            else
            {
                Debug.LogError($"Pathfinding failed, error: {result.Message}");
            }
        }

        public void CancelCalculation()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                Debug.LogWarning("Pathfinding cancelled by user");
            }
        }
    }
}