using System.Collections.Generic;
using UnityEngine;
using DroneSwampPathfiner.Core.Models;
using DroneSwampPathfiner.Core.Simulation;

// (to prevent Unity.Vector3 collisions)
using NumVector3 = System.Numerics.Vector3;
using NumQuaternion = System.Numerics.Quaternion;

namespace DroneSwampPathfiner.Unity.Testing
{
    /// <summary>
    /// Creates mock drone pathing data for tests before implementing actual pathfinding algorithms and JSON serialization
    /// </summary>
    public class SimulationTester : MonoBehaviour
    {

        private void Start()
        {
            Debug.LogWarning("USING MOCK TEST DATA!!! ----------------");
            // Spawn drones
            var drones = new List<Drone>
            {
                new Drone(id: 0, new TransformData(new NumVector3(0, 0, 0)), groupId: 0),
                new Drone(id: 1, new TransformData(new NumVector3(10, 0, 10)), groupId: 1)
            };
            Managers.DroneManager.instance.SpawnDrones(drones);

            // Create mock paths
            var paths = new Dictionary<int, DronePath>
            {
                {
                    0, new DronePath
                    {
                        DroneId = 0,
                        Waypoints = new List<Waypoint>
                        {
                            new Waypoint(0, new NumVector3(0, 0, 0), NumQuaternion.Identity),
                            new Waypoint(2, new NumVector3(0, 5, 0), NumQuaternion.Identity),   // go up
                            new Waypoint(5, new NumVector3(10, 5, 10), NumQuaternion.Identity)  // switch places with drone 1
                        }
                    }
                },
                {
                    1, new DronePath
                    {
                        DroneId = 1,
                        Waypoints = new List<Waypoint>
                        {
                            new Waypoint(0, new NumVector3(10, 0, 10), NumQuaternion.Identity),
                            new Waypoint(3, new NumVector3(10, 5, 10), NumQuaternion.Identity), // go up (slower)
                            new Waypoint(5, new NumVector3(0, 5, 0), NumQuaternion.Identity)    // switch places with drone 0
                        }
                    }
                }
            };

            // Generate mock results
            var dummyResult = new SimulationResult
            {
                IsSuccessful = true,
                Paths = paths
            };

            // Load the simulation
            Managers.SimulationPlaybackManager.instance.LoadSimulationResult(dummyResult);

            Debug.Log("Mock test data loaded, use UI to control the simulation or use space to play and side arrow keys to step");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (Managers.SimulationPlaybackManager.instance.isPlaying) Managers.SimulationPlaybackManager.instance.Pause();
                else Managers.SimulationPlaybackManager.instance.Play();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Managers.SimulationPlaybackManager.instance.StepForward();
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Managers.SimulationPlaybackManager.instance.StepBackward();
            }
        }
    }
}