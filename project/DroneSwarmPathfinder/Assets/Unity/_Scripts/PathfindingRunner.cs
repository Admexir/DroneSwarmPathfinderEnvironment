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
using DroneSwarmPathfinder.Core.Serialization;

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

        public async Task<SimulationResult> RunAlgorithmAsync(IPathfindingAlgorithm algorithm)
        {
            if (algorithm == null) return new SimulationResult { IsSuccessful = false, Message = "Missing algorithm" };

            Debug.Log($"Starting {algorithm.AlgorithmName}...");

            // Load the config files, deserialize and convert to used format (dicts)
            string startConfigPath = ConfigSelectionManager.instance.StartConfigPath;
            string targetConfigPath = ConfigSelectionManager.instance.TargetConfigPath;
            string environmentConfigPath = ConfigSelectionManager.instance.EnvironmentConfigPath;
            if ((string.IsNullOrEmpty(startConfigPath) && !ConfigSelectionManager.instance.UseCurrentSceneForStart) || string.IsNullOrEmpty(targetConfigPath))
            {
                Debug.LogError("Can't run pathfinding: start or target configuration path is empty");
                return new SimulationResult { IsSuccessful = false, Message = "Missing configurations" };
            }

            // Need to differentiate between "use current scene as initial config" and using file initial config
            Dictionary<int, Drone> initialDrones;
            EnvironmentConfigJson environment;
            if (!ConfigSelectionManager.instance.UseCurrentSceneForStart)
            {
                var startConfig = JSONSerializer.DeserializeFile<DroneConfigJson>(startConfigPath);
                initialDrones = startConfig?.Drones.ToDictionary(d => d.ID, d => d) ?? new Dictionary<int, Drone>();
                string startJson = System.IO.File.ReadAllText(startConfigPath);
                environment = JSONSerializer.DeserializeFile<EnvironmentConfigJson>(environmentConfigPath);
            }
            else
            {
                initialDrones = DroneManager.instance.AllDroneModels.ToDictionary(d => d.ID, d => d);
                environment = new EnvironmentConfigJson(EnvironmentManager.instance.CurrentWorldEnvironment);
            }
            string targetJson = System.IO.File.ReadAllText(targetConfigPath);
            var targetConfig = JSONSerializer.Deserialize<DroneConfigJson>(targetJson);
            var targetDrones = targetConfig?.Drones.ToDictionary(d => d.ID, d => d) ?? new Dictionary<int, Drone>();


            // Get environment
            var worldEnv = new WorldEnvironment(environment.SpatialRules, environment.Obstacles);

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
            }
            else
            {
                Debug.LogError($"Pathfinding failed, error: {result.Message}");
            }

            return result;
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