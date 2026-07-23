using DroneSwampPathfiner.Core.Models;
using DroneSwampPathfiner.Unity.Visuals;
using System.Collections.Generic;
using UnityEngine;

namespace DroneSwampPathfiner.Unity.Managers
{

    public class DroneManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameObject dronePrefab;
        [SerializeField] private Transform dronesHolder;

        private Dictionary<int, DroneView> _activeDrones = new(); // TODO: use for moving drones later

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

        public void ClearDrones()
        {
            foreach (var droneView in _activeDrones.Values)
            {
                Destroy(droneView.gameObject);
            }
            _activeDrones.Clear();
        }
    }
}