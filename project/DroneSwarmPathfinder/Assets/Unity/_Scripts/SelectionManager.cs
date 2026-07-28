using DroneSwarmPathfinder.Unity.Managers;
using DroneSwarmPathfinder.Unity.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DroneSwarmPathfinder.Unity.EditorTools
{
    public class SelectionManager : MonoBehaviour
    {
        private List<DroneView> _selectedDrones = new();
        public IReadOnlyList<DroneView> SelectedDrones => _selectedDrones;

        // Event for when selection changes (used by UI and Gizmos)
        public event Action<List<int>> OnSelectionChanged;

        public void SelectSingle(DroneView drone)
        {
            _selectedDrones.Clear();
            _selectedDrones.Add(drone);
            NotifySelectionChanged();
        }

        public void AddToSelection(DroneView drone)
        {
            if (!_selectedDrones.Contains(drone))
            {
                _selectedDrones.Add(drone);
                NotifySelectionChanged();
            }
        }

        public void RemoveFromSelection(DroneView drone)
        {
            _selectedDrones.Remove(drone);
            NotifySelectionChanged();
        }

        public void ClearSelection()
        {
            if (_selectedDrones.Count == 0) return;

            _selectedDrones.Clear();
            NotifySelectionChanged();
        }

        /// <summary>
        /// Selects drones in the scene by interacting with the UI (drone list)
        /// </summary>
        public void SetSelectionFromUI(IEnumerable<int> droneIds)
        {
            _selectedDrones.Clear();

            if (droneIds != null)
            {
                foreach (int id in droneIds)
                {
                    var view = DroneManager.instance.GetDroneView(id);
                    if (view != null)
                    {
                        _selectedDrones.Add(view);
                    }
                }
            }

            // Invoke an update here so Gizmos update without calling OnSelectionChanged to prevent UI cycle
            SendMessage("UpdateGizmoState", SendMessageOptions.DontRequireReceiver);
        }

        private void NotifySelectionChanged()
        {
            OnSelectionChanged?.Invoke(_selectedDrones.Select(d => d.DroneID).ToList());
        }
    }
}