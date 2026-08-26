using DroneSwarmPathfinder.Core.Environment;
using DroneSwarmPathfinder.Core.Models;
using DroneSwarmPathfinder.Core.Serialization;
using DroneSwarmPathfinder.Unity.Environment;
using System;
using System.Collections.Generic;
using UnityEngine;
using NumVector3 = System.Numerics.Vector3;

namespace DroneSwarmPathfinder.Unity.Managers
{
    /// <summary>
    /// Script serving as a bridge between Unity/Core versions of the environment, similar to DroneManager.cs
    /// </summary>
    public class EnvironmentManager : MonoBehaviour
    {
        public static EnvironmentManager instance;
        private void Awake() => instance = this;

        [Header("Config")]
        [SerializeField] private GameObject _obstaclePrefab;
        [SerializeField] private Transform _obstaclesHolder;
        [SerializeField] private float _baseGridSize;

        public DiscreteGrid Grid { get; private set; }

        // Event for the UI to listen to
        public event Action OnObstacleRosterChanged;

        private Dictionary<int, ObstacleView> _activeObstacles = new(); // Visualisation obstacle data
        private Dictionary<int, IObstacleVolume> _obstacleModels = new(); // Config obstacle data

        // Config obstacle data for config editor
        public IEnumerable<IObstacleVolume> AllObstacleModels => _obstacleModels.Values;

        // World environment
        public WorldEnvironment CurrentWorldEnvironment { 
            get 
            { 
                if (_currentWorldEnvironment == null)
                {
                    _currentWorldEnvironment = new WorldEnvironment(Grid, AllObstacleModels);
                }
                return _currentWorldEnvironment;
            } 
        }
        private WorldEnvironment _currentWorldEnvironment;

        // Unique ID counter
        private int _nextObstacleId = 0;

        private void Start()
        {
            ChangeGridSize(_baseGridSize);
            OnObstacleRosterChanged += UpdateCurrentWorldEnvironment;
        }

        #region grid managment
        public void ChangeGridSize(float gridSize)
        {
            Grid = new DiscreteGrid(gridSize);
        }

        private void UpdateCurrentWorldEnvironment() { _currentWorldEnvironment = new WorldEnvironment(Grid, AllObstacleModels); }

        #endregion

        #region obstacle managment

        public void SpawnObstacles(IEnumerable<IObstacleVolume> coreObstacles)
        {
            ClearObstacles();
            foreach (var coreObstacle in coreObstacles)
            {
                AddExistingObstacle(coreObstacle);
                if (coreObstacle.ID >= _nextObstacleId) _nextObstacleId = coreObstacle.ID + 1;
            }
            OnObstacleRosterChanged?.Invoke();
        }

        public void ClearObstacles()
        {
            foreach (var obstacleView in _activeObstacles.Values)
            {
                Destroy(obstacleView.gameObject);
            }
            _activeObstacles.Clear();
            _obstacleModels.Clear();
            _nextObstacleId = 0;
            OnObstacleRosterChanged?.Invoke();
        }

        /// <summary>
        /// Creates a new obstacle in the configuration
        /// </summary>
        public BoxObstacle CreateNewObstacle(Vector3 spawnPosition)
        {
            int newId = _nextObstacleId++;

            var transformData = new TransformData(
                position: new NumVector3(spawnPosition.x, spawnPosition.y, spawnPosition.z));

            BoxObstacle newObstacle = new BoxObstacle(newId, transformData);

            AddExistingObstacle(newObstacle);

            OnObstacleRosterChanged?.Invoke();
            return newObstacle;
        }

        /// <summary>
        /// Removes an obstacle from the configuration and scene
        /// </summary>
        public void RemoveObstacle(int id)
        {
            if (_activeObstacles.TryGetValue(id, out ObstacleView view))
            {
                Destroy(view.gameObject);
                _activeObstacles.Remove(id);
            }
            _obstacleModels.Remove(id);
            OnObstacleRosterChanged?.Invoke();
        }

        /// <summary>
        /// Updates an obstacle's position in config and in the scene
        /// </summary>
        public void UpdateObstaclePosition(int id, Vector3 newPosition)
        {
            if (_obstacleModels.TryGetValue(id, out IObstacleVolume obstacle))
            {
                obstacle.Transform = obstacle.Transform.WithPosition(newPosition.ToNumerics());

                //v (not needed unless I switch obstacles to structs)
                _obstacleModels[id] = obstacle;

                if (_activeObstacles.TryGetValue(id, out ObstacleView view))
                {
                    view.transform.position = newPosition;
                }
            }
        }

        /// <summary>
        /// Updates an obstacle's size in config and in the scene
        /// </summary>
        public void UpdateObstacleSize(int id, Vector3 newSize)
        {
            if (_obstacleModels.TryGetValue(id, out IObstacleVolume obstacle))
            {
                obstacle.Transform = obstacle.Transform.WithSize(newSize.ToNumerics());
                _obstacleModels[id] = obstacle;

                if (_activeObstacles.TryGetValue(id, out ObstacleView view))
                {
                    view.transform.localScale = newSize;
                }
            }
        }

        public IObstacleVolume GetObstacleDataFromID(int id)
        {
            if (_obstacleModels.TryGetValue(id, out IObstacleVolume val))
            {
                return val;
            }
            else { return null; }
        }

        public ObstacleView GetObstacleView(int id)
        {
            return _activeObstacles.TryGetValue(id, out var view) ? view : null;
        }

        /// <summary>
        /// Adds an obstacle from config to the scene
        /// </summary>
        private void AddExistingObstacle(IObstacleVolume coreObstacle)
        {
            GameObject obsObj = Instantiate(_obstaclePrefab, _obstaclesHolder);

            obsObj.transform.position = coreObstacle.Transform.Position.ToUnity();
            obsObj.transform.localScale = coreObstacle.Transform.Size.ToUnity();
            obsObj.transform.rotation = Quaternion.identity; // TODO: no rotation currently allowed due to collision detection

            ObstacleView view = obsObj.GetComponent<ObstacleView>();

            // Pass only the ID to keep the view decoupled from the Core model
            view.Initialize(coreObstacle.ID);

            _activeObstacles.Add(coreObstacle.ID, view);
            _obstacleModels.Add(coreObstacle.ID, coreObstacle);
        }
    }
        #endregion
}