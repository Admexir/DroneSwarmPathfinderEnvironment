using DroneSwampPathfiner.Core.Environment;
using DroneSwampPathfiner.Core.Models;
using DroneSwampPathfiner.Unity.Environment;
using System.Collections.Generic;
using UnityEngine;
using NumVector3 = System.Numerics.Vector3;

namespace DroneSwampPathfiner.Unity.Managers
{
    /// <summary>
    /// Script serving as a bridge between Unity/Core versions of obstacles, mostly mirrors DroneManager.cs
    /// </summary>
    public class ObstacleManager : MonoBehaviour
    {
        public static ObstacleManager instance;
        private void Awake() => instance = this;

        [Header("Config")]
        [SerializeField] private GameObject obstaclePrefab;
        [SerializeField] private Transform obstaclesHolder;

        private Dictionary<int, ObstacleView> _activeObstacles = new(); // Visualisation obstacle data
        private Dictionary<int, BoxObstacle> _obstacleModels = new(); // Config obstacle data

        // Config obstacle data for config editor
        public IEnumerable<BoxObstacle> AllObstacleModels => _obstacleModels.Values;

        // Unique ID counter
        private int _nextObstacleId = 0;

        public void SpawnObstacles(IEnumerable<BoxObstacle> coreObstacles)
        {
            ClearObstacles();
            foreach (var coreObstacle in coreObstacles)
            {
                AddExistingObstacle(coreObstacle);
                if (coreObstacle.ID >= _nextObstacleId) _nextObstacleId = coreObstacle.ID + 1;
            }
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
        }

        /// <summary>
        /// Updates an obstacle's position in config and in the scene
        /// </summary>
        public void UpdateObstaclePosition(int id, Vector3 newPosition)
        {
            if (_obstacleModels.TryGetValue(id, out BoxObstacle obstacle))
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
            if (_obstacleModels.TryGetValue(id, out BoxObstacle obstacle))
            {
                obstacle.Transform = obstacle.Transform.WithSize(newSize.ToNumerics());
                _obstacleModels[id] = obstacle;

                if (_activeObstacles.TryGetValue(id, out ObstacleView view))
                {
                    view.transform.localScale = newSize;
                }
            }
        }

        public BoxObstacle GetObstacleDataFromID(int id)
        {
            if (_obstacleModels.TryGetValue(id, out BoxObstacle val))
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
        private void AddExistingObstacle(BoxObstacle coreObstacle)
        {
            GameObject obsObj = Instantiate(obstaclePrefab, obstaclesHolder);

            obsObj.transform.position = coreObstacle.Transform.Position.ToUnity();
            obsObj.transform.localScale = coreObstacle.Transform.Size.ToUnity();
            obsObj.transform.rotation = Quaternion.identity; // TODO: no rotation currently allowed due to collision detection

            ObstacleView view = obsObj.GetComponent<ObstacleView>();
            view.Initialize(coreObstacle);

            _activeObstacles.Add(coreObstacle.ID, view);
            _obstacleModels.Add(coreObstacle.ID, coreObstacle);
        }
    }
}