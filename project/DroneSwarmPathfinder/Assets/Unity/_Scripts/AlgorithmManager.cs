using DroneSwarmPathfinder.Core.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DroneSwarmPathfinder.Unity.Managers
{
    /// <summary>
    /// Class used as a holder (and manager) for pathfinding algorithms
    /// </summary>
    public class AlgorithmManager : MonoBehaviour
    {
        public static AlgorithmManager instance;
        private void Awake() => instance = this;

        public IReadOnlyList<IPathfindingAlgorithm> AvailableAlgorithms { get; private set; } = new List<IPathfindingAlgorithm>();
        public IPathfindingAlgorithm SelectedAlgorithm { get; private set; }

        // Events for the UIPresenter to listen to
        public event Action OnAlgorithmsRefreshed;
        public event Action<bool> OnSelectionValidityChanged;

        private void Start()
        {
            RefreshAlgorithmList();
        }

        public void RefreshAlgorithmList()
        {
            AvailableAlgorithms = AlgorithmRegistry.DiscoverAlgorithms().ToList();

            if (AvailableAlgorithms.Count > 0)
            {
                SelectedAlgorithm = AvailableAlgorithms[0];
                OnSelectionValidityChanged?.Invoke(true);
            }
            else
            {
                SelectedAlgorithm = null;
                OnSelectionValidityChanged?.Invoke(false);
            }

            OnAlgorithmsRefreshed?.Invoke();
        }

        public bool LoadExternalAlgorithm(string absolutePath)
        {
            if (AlgorithmRegistry.LoadExternalAlgorithmDll(absolutePath))
            {
                RefreshAlgorithmList();
                return true;
            }
            return false;
        }

        public void SelectAlgorithmByName(string algorithmName)
        {
            SelectedAlgorithm = AvailableAlgorithms.FirstOrDefault(a => a.AlgorithmName == algorithmName);
            OnSelectionValidityChanged?.Invoke(SelectedAlgorithm != null);
        }
    }
}