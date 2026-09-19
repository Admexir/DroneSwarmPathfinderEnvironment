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
using DroneSwarmPathfinder.Unity.Managers;

namespace DroneSwarmPathfinder.Unity.Testing
{
    /// <summary>
    /// Temporary script for testing the pathfinding API and simulation playback
    /// </summary>
    public class SimulationTester : MonoBehaviour
    {
        public bool USETESTRUNNER;
        private void Start()
        {
            if (!USETESTRUNNER) return;
            Debug.LogWarning("USING TEST RUNNER TO SET UP DEFAULT SCENE!!!");

            // Spawn initial test drones and create groups for them
            DroneManager.instance.CreateNewDroneGroup("Group 1", Color.blue);
            DroneManager.instance.CreateNewDroneGroup("Group 2", Color.red);
            var drones = new List<Drone>
            {
                new Drone(id: 0, new TransformData(new NumVector3(0, 0, 0)), groupName: "Group 1"),
                new Drone(id: 1, new TransformData(new NumVector3(10, 0, 10)), groupName: "Group 2")
            };
            Managers.DroneManager.instance.ClearAndSpawnDrones(drones);
        }
    }
}