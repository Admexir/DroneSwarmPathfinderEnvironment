using DroneSwarmPathfinder.Core.Models;
using DroneSwarmPathfinder.Unity.Visuals;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using NumVector3 = System.Numerics.Vector3;
using NumQuaternion = System.Numerics.Quaternion;
using DroneSwarmPathfinder.Unity.EditorTools;
using DroneSwarmPathfinder.Core.Serialization;

namespace DroneSwarmPathfinder.Unity.Managers
{
    public class DroneManager : MonoBehaviour
    {
        public static DroneManager instance;
        private void Awake() => instance = this;
        public const int DRONEPOSITIONINDEXOFFSET = 10000;

        [Header("Config")]
        [SerializeField] private GameObject dronePrefab;
        [SerializeField] private GameObject droneTargetPrefab;
        [SerializeField] private Transform dronesHolder;

        public event System.Action OnDroneRosterChanged; // Callback for UI

        private Dictionary<int, DroneView> _activeDrones = new(); // Visualisation drone data
        private Dictionary<int, Drone> _droneModels = new(); // Config drone data
        private Dictionary<int, DroneTargetPosition> _droneTargets = new(); // Config drone data
        //private Dictionary<int, IDroneConfigItem> _droneAllItems = //It'd be beneficial to have a combined dictionary to reduce copy-paste code, but it'd affect performance (regenerating any time you'd want to access it) or you'd have to set up callbacks for whenever you'd update drone models or targets

        // Config drone data for config editor
        public bool HasNoTargetPositions => _droneTargets.Count == 0;
        public IEnumerable<Drone> AllDroneModelsOnly => _droneModels.Values;
        public IEnumerable<DroneTargetPosition> AllDroneTargetPositionsOnly => _droneTargets.Values;
        public IEnumerable<IDroneConfigItem> AllDroneItems => _droneModels.Values.Cast<IDroneConfigItem>().Concat(_droneTargets.Values.Cast<IDroneConfigItem>());

        public Dictionary<string, Color> _droneGroups = new() { { "default", Color.white } };
        public IReadOnlyCollection<string> DroneGroups => _droneGroups.Keys;
        //// Gameobject drone data
        //public IEnumerable<DroneView> AllDroneGameObjects => _activeDrones.Values;

        // Unique ID counter
        private int _nextDroneId = 0;
        private int _nextDronePositionId = DRONEPOSITIONINDEXOFFSET;

        public void ClearAndSpawnDrones(IEnumerable<IDroneConfigItem> coreDrones)
        {
            ClearDrones();
            foreach (var coreDrone in coreDrones)
            {
                if(coreDrone is Drone) AddExistingDrone((Drone)coreDrone);
                if (coreDrone is DroneTargetPosition) AddExistingDrone((DroneTargetPosition)coreDrone);
                if (coreDrone.ID >= _nextDroneId) _nextDroneId = coreDrone.ID + 1;
            }
            OnDroneRosterChanged?.Invoke();
        }

        public void ClearDrones()
        {
            foreach (var droneView in _activeDrones.Values)
            {
                Destroy(droneView.gameObject);
            }
            _activeDrones.Clear();
            _droneModels.Clear();
            _droneTargets.Clear();
            _nextDroneId = 0;
            _nextDronePositionId = DRONEPOSITIONINDEXOFFSET;
            OnDroneRosterChanged?.Invoke();
        }

        /// <summary>
        /// Creates a new drone in the configuration (ex. Add Drone button)
        /// </summary>
        public Drone CreateNewDrone(Vector3 spawnPosition)
        {
            int newId = _nextDroneId++;
            var transformData = new TransformData(
                position: new NumVector3(spawnPosition.x, spawnPosition.y, spawnPosition.z),
                size: new NumVector3(1f, 0.4f, 1f));

            Drone newDrone = new Drone(newId, transformData);

            AddExistingDrone(newDrone);
            OnDroneRosterChanged?.Invoke();
            return newDrone;
        }

        public DroneTargetPosition CreateNewDronePosition(Vector3 spawnPosition)
        {
            int newId = _nextDronePositionId++;
            var transformData = new TransformData(
                position: new NumVector3(spawnPosition.x, spawnPosition.y, spawnPosition.z));

            DroneTargetPosition newDronePos = new DroneTargetPosition(newId, transformData, "default");

            AddExistingDrone(newDronePos);
            OnDroneRosterChanged?.Invoke();
            return newDronePos;
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
            _droneTargets.Remove(id);
            OnDroneRosterChanged?.Invoke();
        }

        /// <summary>
        /// Updates a drone's group in config and in the scene
        /// </summary>
        public void UpdateDroneGroup(int id, string newGroup)
        {
            if (id >= DRONEPOSITIONINDEXOFFSET)
            {
                if (_droneTargets.TryGetValue(id, out DroneTargetPosition drone))
                {
                    drone.GroupName = newGroup;
                    _droneTargets[id] = drone;

                    // Updates color
                    if (_activeDrones.TryGetValue(id, out DroneView view))
                    {
                        view.Initialize(id, newGroup, true);
                    }
                }
            }
            else
            {
                if (_droneModels.TryGetValue(id, out Drone drone))
                {
                    drone.GroupName = newGroup;
                    _droneModels[id] = drone;

                    // Updates color
                    if (_activeDrones.TryGetValue(id, out DroneView view))
                    {
                        view.Initialize(id, newGroup);
                    }
                }
            }
        }

        public void UpdateDroneName(int id, string newName)
        {
            var drone = GetDroneDataFromID(id);
            if (drone != null) drone.Name = newName;
        }

        public void UpdateDroneDescription(int id, string newDescription)
        {
            var drone = GetDroneDataFromID(id);
            if (drone != null) drone.Description = newDescription;
        }

        public void UpdateDroneColor(int id, System.Drawing.Color newColor)
        {
            var drone = GetDroneDataFromID(id);
            if (drone != null)
            {
                drone.Color = newColor;
                if (_activeDrones.TryGetValue(id, out DroneView view))
                {
                    view.SetCustomColor(newColor.ToUnity(), id >= DRONEPOSITIONINDEXOFFSET);
                }
            }
        }

        public void RemoveDroneGroup(string groupName)
        {
            if (_droneGroups.ContainsKey(groupName))
            {
                foreach(DroneView d in _activeDrones.Values.Where(x => x.DroneGroup == groupName))
                {
                    UpdateDroneGroup(d.ID, "default");
                }
                _droneGroups.Remove(groupName);
            }
        }

        /// <summary>
        /// Updates a drone's position in config and in the scene
        /// </summary>
        public void UpdateDronePosition(int id, Vector3 newPosition)
        {
            if(id >= DRONEPOSITIONINDEXOFFSET)
            {
                // Drone position
                if (_droneTargets.TryGetValue(id, out DroneTargetPosition drone))
                {
                    drone.Transform = drone.Transform.WithPosition(newPosition.ToNumerics());
                    _droneTargets[id] = drone;

                    // Updates position
                    if (_activeDrones.TryGetValue(id, out DroneView view))
                    {
                        view.transform.position = newPosition;
                    }
                }
            }
            else
            {
                // Actual Drone
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
        }

        public IDroneConfigItem GetDroneDataFromID(int id)
        {
            if (id < DRONEPOSITIONINDEXOFFSET &&  _droneModels.TryGetValue(id, out Drone val))
            {
                return val;
            }else if(id >= DRONEPOSITIONINDEXOFFSET && _droneTargets.TryGetValue(id, out DroneTargetPosition val2))
            {
                return val2;
            }
            else { return null; }
        }

        /// <summary>
        /// Adds a drone from config to the scene
        /// </summary>
        private void AddExistingDrone(Drone coreDrone)
        {
            GameObject droneObj = Instantiate(dronePrefab, dronesHolder);
            //droneObj.layer = ConfigEditorManager.instance.droneLayer;

            droneObj.transform.position = coreDrone.Transform.Position.ToUnity();
            droneObj.transform.rotation = coreDrone.Transform.Rotation.ToUnity();
            droneObj.transform.localScale = coreDrone.Transform.Size.ToUnity();

            if (!_droneGroups.ContainsKey(coreDrone.GroupName)) CreateNewDroneGroup(coreDrone.GroupName, new Color(Random.value, Random.value, Random.value)); //TODO: allow user to choose color, then add it to serialization and load it here instead of randomising

            DroneView view = droneObj.GetComponent<DroneView>();
            view.Initialize(coreDrone.ID, coreDrone.GroupName);
            if (coreDrone.Color.A > 0) view.SetCustomColor(coreDrone.Color.ToUnity(), false);

            _activeDrones.Add(coreDrone.ID, view);
            _droneModels.Add(coreDrone.ID, coreDrone);
        }
        /// <summary>
        /// Adds a drone target position from config to the scene
        /// </summary>
        private void AddExistingDrone(DroneTargetPosition coreDrone)
        {
            GameObject droneObj = Instantiate(droneTargetPrefab, dronesHolder);
            droneObj.name = $"Drone Pos {coreDrone.ID} (group: {coreDrone.GroupName})";
            //droneObj.layer = ConfigEditorManager.instance.droneLayer;

            droneObj.transform.position = coreDrone.Transform.Position.ToUnity();
            droneObj.transform.rotation = coreDrone.Transform.Rotation.ToUnity();
            droneObj.transform.localScale = coreDrone.Transform.Size.ToUnity();

            DroneView view = droneObj.GetComponent<DroneView>();
            view.Initialize(coreDrone.ID, coreDrone.GroupName, true);
            if (coreDrone.Color.A > 0) view.SetCustomColor(coreDrone.Color.ToUnity(), true);

            _activeDrones.Add(coreDrone.ID, view);
            _droneTargets.Add(coreDrone.ID, coreDrone);
        }

        public DroneView GetDroneView(int id)
        {
            return _activeDrones.TryGetValue(id, out var drone) ? drone : null;
        }

        /// <summary>
        /// Returns whether all drones are in a valid position of the world
        /// </summary>
        /// <param name="invalidDrones">List of drones in invalid places</param>
        /// <returns>True if all drones are in valid places</returns>
        public bool CheckDronePositionValidity(out List<IDroneConfigItem> invalidDrones)
        {
            invalidDrones = new List<IDroneConfigItem>();
            foreach(var drone in AllDroneItems)
            {
                if (!EnvironmentManager.instance.CurrentWorldEnvironment.IsEmpty(drone.Transform.Position)) { invalidDrones.Add(drone); }
            }
            return invalidDrones.Count == 0;
        }

        /// <summary>
        /// Creates a new drone group with the given name and color
        /// </summary>
        public void CreateNewDroneGroup(string groupName, Color droneColor)
        {
            int index = 0;
            while (!_droneGroups.TryAdd($"{groupName}{(index == 0 ? "" : "_" + index)}", droneColor)) { index++; } // To prevent key collisions
        }

        /// <summary>
        /// Returns whether this group exists and gives the color in the out parameter
        /// </summary>
        /// <param name="groupName">Name of the group</param>
        /// <param name="color">The color of that group</param>
        /// <returns>Whether this group exists</returns>
        public bool GetColorByGroup(string groupName, out Color color)
        {
            return _droneGroups.TryGetValue(groupName, out color);
        }
    }
}