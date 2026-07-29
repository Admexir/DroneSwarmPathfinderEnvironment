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
        public float gridSize = 1f;

        private SelectionManager _selectionManager;
        private GizmoManager _gizmoController;

        public event Action<Vector3> OnGizmoDragged
        {
            add => _gizmoController.OnGizmoDragged += value;
            remove => _gizmoController.OnGizmoDragged -= value;
        }

        public IReadOnlyList<DroneView> SelectedDrones => _selectionManager.SelectedDrones;
        public event Action<List<int>> OnSelectionChanged
        {
            add => _selectionManager.OnSelectionChanged += value;
            remove => _selectionManager.OnSelectionChanged -= value;
        }

        private void Awake()
        {
            instance = this;
            _selectionManager = GetComponent<SelectionManager>();
            _gizmoController = GetComponent<GizmoManager>();
        }

        private void Start()
        {
            _gizmoController.InitializeGrid(gridSize);
            //_gizmoController.OnGizmoDragged += HandleGizmoDragged;
        }

        public void SetSelectionFromUI(IEnumerable<int> droneIds)
        {
            _selectionManager.SetSelectionFromUI(droneIds);
        }

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