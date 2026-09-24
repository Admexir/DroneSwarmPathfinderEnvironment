using DroneSwarmPathfinder.Core.Environment;
using DroneSwarmPathfinder.Core.Models;
using DroneSwarmPathfinder.Core.Serialization;
using DroneSwarmPathfinder.Core.Simulation;
using DroneSwarmPathfinder.Unity.EditorTools;
using DroneSwarmPathfinder.Unity.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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

        public async Task<SimulationResult> RunAlgorithmAsync(IPathfindingAlgorithm algorithm, IProgress<float> uiProgress = null)
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
            //Dictionary<int, Drone> initialDrones;
            DroneConfigJson startConfig;
            EnvironmentConfigJson environment;
            if (!ConfigSelectionManager.instance.UseCurrentSceneForStart)
            {
                startConfig = JSONSerializer.DeserializeFile<DroneConfigJson>(startConfigPath);
                //initialDrones = startConfig?.Drones.ToDictionary(d => d.ID, d => d) ?? new Dictionary<int, Drone>();
                string startJson = System.IO.File.ReadAllText(startConfigPath);
                environment = JSONSerializer.DeserializeFile<EnvironmentConfigJson>(environmentConfigPath);
            }
            else
            {
                //initialDrones = DroneManager.instance.AllDroneItems.ToDictionary(d => d.ID, d => new Drone(d.ID, d.Transform, d.GroupName));
                startConfig = ConfigSelectionManager.CreateDroneConfigJson(DroneManager.instance.AllDroneModelsOnly, DroneManager.instance.AllDroneTargetPositionsOnly, DroneManager.instance._droneGroups);
                environment = new EnvironmentConfigJson(EnvironmentManager.instance.CurrentWorldEnvironment);
            }
            string targetJson = System.IO.File.ReadAllText(targetConfigPath);
            var targetConfig = JSONSerializer.Deserialize<DroneConfigJson>(targetJson);
            //var targetDrones = targetConfig?.Drones.ToDictionary(d => d.ID, d => d) ?? new Dictionary<int, Drone>();
            //var groupTargets = targetConfig?.TargetPositions.GroupBy(d => d.GroupName).ToDictionary(g => g.Key, g => g.ToList()) ?? new Dictionary<string, List<DroneTargetPosition>>();

            // Get environment
            var worldEnv = new WorldEnvironment(environment.SpatialRules, environment.Obstacles);

            //var context = new SimulationContext
            //{
            //    InitialDrones = initialDrones,
            //    DroneSpecificTargets = targetDrones,
            //    GroupTargets = groupTargets,
            //    Environment = worldEnv
            //};
            var context = new SimulationContext
            {
                InitialState = startConfig,
                TargetState = targetConfig,
                Environment = worldEnv
            };

            //Debug.Log($"context: {context.InitialState.Count}, {context.DroneSpecificTargets.Count}, {context.GroupTargets.Count}, {context.TryGetTargetFromGroup("default", out var target)}: {target}");
            //while (context.TryGetTargetFromGroup("default", out var targetPos)) Debug.Log($"target: {targetPos}");

            // Run algorithm async
            _cancellationTokenSource = new CancellationTokenSource();

            var progress = new System.Progress<float>(p =>
            {
                Debug.Log($"Calculating... {p * 100:F0}%");
                uiProgress?.Report(p);
            });

            SimulationResult result = await algorithm.CalculatePathsAsync(context, progress, _cancellationTokenSource.Token);
            if (result.SimulationContext == null) result.SimulationContext = context;

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