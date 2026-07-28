using DroneSwarmPathfinder.Core.Models;
using DroneSwarmPathfinder.Unity.Visuals;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using NumVector3 = System.Numerics.Vector3;
using NumQuaternion = System.Numerics.Quaternion;
using DroneSwarmPathfinder.Unity.EditorTools;

namespace DroneSwarmPathfinder.Unity.Managers
{
    public class DroneManager : MonoBehaviour
    {
        public static DroneManager instance;
        private void Awake() => instance = this;

        [Header("Config")]
        [SerializeField] private GameObject dronePrefab;
        [SerializeField] private Transform dronesHolder;

        private Dictionary<int, DroneView> _activeDrones = new(); // Visualisation drone data
        private Dictionary<int, Drone> _droneModels = new(); // Config drone data

        // Config drone data for config editor
        public IEnumerable<Drone> AllDroneModels => _droneModels.Values;
        //// Gameobject drone data
        //public IEnumerable<DroneView> AllDroneGameObjects => _activeDrones.Values;

        // Unique ID counter
        private int _nextDroneId = 0;

        public void SpawnDrones(IEnumerable<Drone> coreDrones)
        {
            ClearDrones();
            foreach (var coreDrone in coreDrones)
            {
                AddExistingDrone(coreDrone);
                if (coreDrone.ID >= _nextDroneId) _nextDroneId = coreDrone.ID + 1;
            }
        }

        public void ClearDrones()
        {
            foreach (var droneView in _activeDrones.Values)
            {
                Destroy(droneView.gameObject);
            }
            _activeDrones.Clear();
            _droneModels.Clear();
            _nextDroneId = 0;
        }

        /// <summary>
        /// Creates a new drone in the configuration (ex. Add Drone button)
        /// </summary>
        public Drone CreateNewDrone(Vector3 spawnPosition)
        {
            int newId = _nextDroneId++;
            var transformData = new TransformData(
                position: new NumVector3(spawnPosition.x, spawnPosition.y, spawnPosition.z));

            Drone newDrone = new Drone(newId, transformData, groupId: 0);

            AddExistingDrone(newDrone);

            return newDrone;
        }

        /// <summary>
        /// Removes a drone from the configuration and scene
        /// </summary>
        public void RemoveDrone(int id)
        {
            if (_activeDrones.TryGetValue(id, out DroneView view))
            {
                Destroy(view.gameObject);
                _activeDrones.Remove(id);
            }
            _droneModels.Remove(id);
        }

        /// <summary>
        /// Updates a drone's group in config and in the scene
        /// </summary>
        public void UpdateDroneGroup(int id, int newGroup)
        {
            if (_droneModels.TryGetValue(id, out Drone drone))
            {
                drone.GroupId = newGroup;
                _droneModels[id] = drone;

                // Updates color
                if (_activeDrones.TryGetValue(id, out DroneView view))
                {
                    view.Initialize(id, newGroup);
                }
            }
        }

        /// <summary>
        /// Updates a drone's position in config and in the scene
        /// </summary>
        public void UpdateDronePosition(int id, Vector3 newPosition)
        {
            if (_droneModels.TryGetValue(id, out Drone drone))
            {
                drone.Transform = drone.Transform.WithPosition(newPosition.ToNumerics());
                _droneModels[id] = drone;

                // Updates position
                if (_activeDrones.TryGetValue(id, out DroneView view))
                {
                    view.transform.position = newPosition;
                }
            }
        }

        //public record DroneInfo(int ID, int GroupId, Vector3 Position, Quaternion Rotation, Vector3 Size);
        //public DroneInfo GetDroneConfigInfo(int id)
        //{
        //    if (_droneModels.TryGetValue(id, out Drone val))
        //    {
        //        return new(id, val.GroupId, val.Transform.Position.ToUnity(), val.Transform.Rotation.ToUnity(), val.Transform.Size.ToUnity());
        //    }
        //    else { return null; }
        //}

        public Drone GetDroneDataFromID(int id)
        {
            if (_droneModels.TryGetValue(id, out Drone val))
            {
                return val;
            }
            else { return null; }
        }

        /// <summary>
        /// Adds a drone from config to the scene
        /// </summary>
        /// <param name="coreDrone"></param>
        private void AddExistingDrone(Drone coreDrone)
        {
            GameObject droneObj = Instantiate(dronePrefab, dronesHolder);
            //droneObj.layer = ConfigEditorManager.instance.droneLayer;

            droneObj.transform.position = coreDrone.Transform.Position.ToUnity();
            droneObj.transform.rotation = coreDrone.Transform.Rotation.ToUnity();
            droneObj.transform.localScale = coreDrone.Transform.Size.ToUnity();

            DroneView view = droneObj.GetComponent<DroneView>();
            view.Initialize(coreDrone.ID, coreDrone.GroupId);

            _activeDrones.Add(coreDrone.ID, view);
            _droneModels.Add(coreDrone.ID, coreDrone);
        }

        public DroneView GetDroneView(int id)
        {
            return _activeDrones.TryGetValue(id, out var drone) ? drone : null;
        }
    }
}