using DroneSwampPathfiner.Core.Models;
using DroneSwampPathfiner.Unity.Visuals;
using System.Collections.Generic;
using UnityEngine;

namespace DroneSwampPathfiner.Unity.Managers
{

    public class DroneManager : MonoBehaviour
    {
        public static DroneManager instance; private void Awake() => instance = this;
        [Header("Config")]
        [SerializeField] private GameObject dronePrefab;
        [SerializeField] private Transform dronesHolder;

        private Dictionary<int, DroneView> _activeDrones = new(); // TODO: use for moving drones later
        public IEnumerable<DroneView> ActiveDrones => _activeDrones.Values;

        /// <summary>
        /// Visualizes a collection of drones and destroys old drones
        /// </summary>
        public void SpawnDrones(IEnumerable<Drone> coreDrones)
        {
            ClearDrones();

            foreach (var coreDrone in coreDrones)
            {
                GameObject droneObj = Instantiate(dronePrefab, dronesHolder);

                droneObj.transform.position = coreDrone.Transform.Position.ToUnity();
                droneObj.transform.rotation = coreDrone.Transform.Rotation.ToUnity();
                droneObj.transform.localScale = coreDrone.Transform.Size.ToUnity();

                DroneView view = droneObj.GetComponent<DroneView>();
                view.Initialize(coreDrone.ID, coreDrone.GroupId);

                _activeDrones.Add(coreDrone.ID, view);
            }
        }

        /// <summary>
        /// Deletes all active drones in the scene
        /// </summary>
        public void ClearDrones()
        {
            foreach (var droneView in _activeDrones.Values)
            {
                Destroy(droneView.gameObject);
            }
            _activeDrones.Clear();
        }
        
        /// <summary>
        /// Fetches an active drone by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DroneView GetDrone(int id)
        {
            return _activeDrones.TryGetValue(id, out var drone) ? drone : null;
        }
    }
}