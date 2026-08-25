using DroneSwarmPathfinder.Unity.Environment;
using DroneSwarmPathfinder.Unity.Managers;
using DroneSwarmPathfinder.Unity.UI;
using DroneSwarmPathfinder.Unity.Visuals;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DroneSwarmPathfinder.Unity.EditorTools
{
    [RequireComponent(typeof(SelectionManager), typeof(GizmoManager), typeof(BoxSelectionVisualizer))]
    [RequireComponent(typeof(EditorInputManager))]
    public class ConfigEditorManager : MonoBehaviour
    {
        public static ConfigEditorManager instance;

        [Header("Editor Config")]

        private SelectionManager _selectionManager;
        private GizmoManager _gizmoController;

        public event Action<Vector3> OnGizmoDragged
        {
            add => _gizmoController.OnGizmoDragged += value;
            remove => _gizmoController.OnGizmoDragged -= value;
        }

        public IReadOnlyList<DroneView> SelectedDrones => _selectionManager.SelectedDrones;
        public IReadOnlyList<ObstacleView> SelectedObstacles => _selectionManager.SelectedObstacles;

        public event Action<List<int>> OnDroneSelectionChanged
        {
            add => _selectionManager.OnDroneSelectionChanged += value;
            remove => _selectionManager.OnDroneSelectionChanged -= value;
        }

        public event Action<List<int>> OnObstacleSelectionChanged
        {
            add => _selectionManager.OnObstacleSelectionChanged += value;
            remove => _selectionManager.OnObstacleSelectionChanged -= value;
        }

        private void Awake()
        {
            instance = this;
            _selectionManager = GetComponent<SelectionManager>();
            _gizmoController = GetComponent<GizmoManager>();
        }

        public void SetDroneSelectionFromUI(IEnumerable<int> ids) => _selectionManager.SetDroneSelectionFromUI(ids);
        public void SetObstacleSelectionFromUI(IEnumerable<int> ids) => _selectionManager.SetObstacleSelectionFromUI(ids);
        public void SetEditorToolMode(EditorToolMode mode) => _gizmoController.SetToolMode(mode);

        //private void HandleGizmoDragged(Vector3 newCenterPosition)
        //{
        //    // Update inspector UI values while dragging gizmos
        //    if (SelectedDrones.Count == 1 && UIController.instance != null)
        //    {
        //        UIController.instance.UpdatePositionFields(SelectedDrones[0].transform.position);
        //    }
        //}
    }
}