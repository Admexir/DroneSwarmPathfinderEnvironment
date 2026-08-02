using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DroneSwarmPathfinder.Core.Models;
using DroneSwarmPathfinder.Core.Simulation;
using DroneSwarmPathfinder.Core.Environment;
using DroneSwarmPathfinder.Algorithms;

// (to prevent Unity.Vector3 collisions)
using NumVector3 = System.Numerics.Vector3;
using NumQuaternion = System.Numerics.Quaternion;
using DroneSwarmPathfinder.Unity.UI;

namespace DroneSwarmPathfinder.Unity.Testing
{
    /// <summary>
    /// Temporary script for testing the pathfinding API and simulation playback
    /// </summary>
    public class SimulationTester : MonoBehaviour
    {
        private void Start()
        {
            Debug.LogWarning("USING TRIVIAL TEST ALGORITHM, Press 'T' to run the pathfinder");

            // Spawn initial test drones
            var drones = new List<Drone>
            {
                new Drone(id: 0, new TransformData(new NumVector3(0, 0, 0)), groupId: 0),
                new Drone(id: 1, new TransformData(new NumVector3(10, 0, 10)), groupId: 1)
            };
            Managers.DroneManager.instance.SpawnDrones(drones);
        }

        private void Update()
        {
            // Quick keyboard shortcut to trigger the algorithm
            if (Input.GetKeyDown(KeyCode.T))
            {
                RunTestAlgorithm();
            }
        }

        public async void RunTestAlgorithm()
        {
            Debug.Log("Starting Trivial Pathfinder...");

            // Get initial State from the drone manager TODO: make this into a separate function outside of this temp test script
            var initialDrones = Managers.DroneManager.instance.AllDroneModels.ToDictionary(d => d.ID, d => d);

            // Define targets (swap positions)
            var targetDrones = new Dictionary<int, Drone>();
            if (initialDrones.ContainsKey(0) && initialDrones.ContainsKey(1))
            {
                targetDrones[0] = new Drone(0, new TransformData(new NumVector3(10, 0, 10)), 0);
                targetDrones[1] = new Drone(1, new TransformData(new NumVector3(0, 0, 0)), 1);
            }

            // Build the environment
            var grid = new DiscreteGrid(1f);
            var obstacles = Managers.ObstacleManager.instance != null
                ? Managers.ObstacleManager.instance.AllObstacleModels
                : new List<BoxObstacle>();

            var worldEnv = new WorldEnvironment(grid, obstacles);

            // Build the simulation context
            var context = new SimulationContext
            {
                InitialState = initialDrones,
                TargetState = targetDrones,
                Environment = worldEnv
            };

            // Run the algo async
            IPathfindingAlgorithm pathfinder = new TrivialPathfinder();
            SimulationResult result = await pathfinder.CalculatePathsAsync(context);

            // Load results to playback
            if (result.IsSuccessful)
            {
                Debug.Log($"Pathfinding Successful, computed in {result.ComputationTime.TotalMilliseconds} ms");
                Managers.SimulationPlaybackManager.instance.LoadSimulationResult(result);
                Managers.SimulationPlaybackManager.instance.Play();
            }
            else
            {
                Debug.LogError($"Pathfinding Failed: {result.Message}");
            }
        }
    }
}