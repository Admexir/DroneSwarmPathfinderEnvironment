using DroneSwarmPathfinder.Unity.Environment;
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
        public List<DroneView> SelectedDrones { get; } = new();
        public List<ObstacleView> SelectedObstacles { get; } = new();

        public event Action<List<int>> OnDroneSelectionChanged;
        public event Action<List<int>> OnObstacleSelectionChanged;

        /// <summary>
        /// Checks if a generic view is currently in any of our selection lists
        /// </summary>
        public bool IsSelected(ISelectableView view)
        {
            if (view is DroneView drone) return SelectedDrones.Contains(drone);
            if (view is ObstacleView obstacle) return SelectedObstacles.Contains(obstacle);
            return false;
        }

        public void SelectSingle(ISelectableView view)
        {
            ClearSelection(notify: false);
            AddToSelection(view);
        }

        public void AddToSelection(ISelectableView view)
        {
            if (view is DroneView drone && !SelectedDrones.Contains(drone))
            {
                SelectedDrones.Add(drone);
                OnDroneSelectionChanged?.Invoke(SelectedDrones.Select(d => d.ID).ToList());
            }
            else if (view is ObstacleView obstacle && !SelectedObstacles.Contains(obstacle))
            {
                SelectedObstacles.Add(obstacle);
                OnObstacleSelectionChanged?.Invoke(SelectedObstacles.Select(o => o.ID).ToList());
            }
        }

        public void RemoveFromSelection(ISelectableView view)
        {
            if (view is DroneView drone)
            {
                SelectedDrones.Remove(drone);
                OnDroneSelectionChanged?.Invoke(SelectedDrones.Select(d => d.ID).ToList());
            }
            else if (view is ObstacleView obstacle)
            {
                SelectedObstacles.Remove(obstacle);
                OnObstacleSelectionChanged?.Invoke(SelectedObstacles.Select(o => o.ID).ToList());
            }
        }

        public void ClearSelection(bool notify = true)
        {
            bool dronesChanged = SelectedDrones.Count > 0;
            bool obstaclesChanged = SelectedObstacles.Count > 0;

            SelectedDrones.Clear();
            SelectedObstacles.Clear();

            if (notify)
            {
                if (dronesChanged) OnDroneSelectionChanged?.Invoke(new List<int>());
                if (obstaclesChanged) OnObstacleSelectionChanged?.Invoke(new List<int>());
            }
        }

        public void SetDroneSelectionFromUI(IEnumerable<int> ids)
        {
            SelectedDrones.Clear();
            if (ids != null)
            {
                foreach (int id in ids)
                {
                    var view = DroneManager.instance.GetDroneView(id);
                    if (view != null) SelectedDrones.Add(view);
                }
            }
            SendMessage("UpdateGizmoState", SendMessageOptions.DontRequireReceiver);
        }

        public void SetObstacleSelectionFromUI(IEnumerable<int> ids)
        {
            SelectedObstacles.Clear();
            if (ids != null)
            {
                foreach (int id in ids)
                {
                    var view = ObstacleManager.instance.GetObstacleView(id);
                    if (view != null) SelectedObstacles.Add(view);
                }
            }
            SendMessage("UpdateGizmoState", SendMessageOptions.DontRequireReceiver);
        }
    }
}