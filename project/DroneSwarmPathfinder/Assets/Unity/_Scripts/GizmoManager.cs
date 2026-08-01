using DroneSwarmPathfinder.Core.Environment;
using DroneSwarmPathfinder.Unity.Managers;
using DroneSwarmPathfinder.Unity.Visuals;
using DroneSwarmPathfinder.Unity.Environment;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DroneSwarmPathfinder.Unity.EditorTools
{
    [RequireComponent(typeof(SelectionManager))]
    public class GizmoManager : MonoBehaviour
    {
        public event Action<Vector3> OnGizmoDragged;

        private SelectionManager _selectionManager;
        private Camera _cam;
        private DiscreteGrid _grid;

        // Gizmos variables
        [SerializeField] private GameObject _transformGizmoRoot;
        [SerializeField] private float _gizmoSize;
        private Transform _movementGizmoX, _movementGizmoY, _movementGizmoZ;
        private Transform _sizeGizmoX, _sizeGizmoY, _sizeGizmoZ;

        public bool IsDraggingGizmo { get; private set; }
        private Vector3 _dragAxis;
        private Plane _dragPlane;
        private Vector3 _dragStartIntersection;

        private Dictionary<ISelectableView, Vector3> _dragStartPositions = new();

        private void Awake()
        {
            _selectionManager = GetComponent<SelectionManager>();
            _cam = Camera.main;
        }

        private void Start()
        {
            //CreateRuntimeGizmo();

            // Listen to selection changes for (currently...) both arrays
            _selectionManager.OnDroneSelectionChanged += (_) => UpdateGizmoState();
            _selectionManager.OnObstacleSelectionChanged += (_) => UpdateGizmoState();

            InitializeGizmo();
        }

        public void InitializeGrid(float gridSize)
        {
            _grid = new DiscreteGrid(gridSize);
        }

        private void InitializeGizmo()
        {
            var mArrowsRoot = GameObject.Find("Movement Arrows").transform;
            _movementGizmoX = mArrowsRoot.GetChild(0);
            _movementGizmoY = mArrowsRoot.GetChild(1);
            _movementGizmoZ = mArrowsRoot.GetChild(2);

            var sArrowsRoot = GameObject.Find("Size Arrows").transform;
            _sizeGizmoX = mArrowsRoot.GetChild(0);
            _sizeGizmoY = mArrowsRoot.GetChild(1);
            _sizeGizmoZ = mArrowsRoot.GetChild(2);
        }

        public void UpdateGizmoState()
        {
            int totalSelectedCount = _selectionManager.SelectedDrones.Count + _selectionManager.SelectedObstacles.Count;

            if (totalSelectedCount == 0)
            {
                _transformGizmoRoot.SetActive(false);
                return;
            }

            _transformGizmoRoot.SetActive(true);

            Vector3 center = Vector3.zero;

            // Combine positions from both drones and obstacles
            foreach (var d in _selectionManager.SelectedDrones) center += d.transform.position;
            foreach (var o in _selectionManager.SelectedObstacles) center += o.transform.position;

            center /= totalSelectedCount;

            _transformGizmoRoot.transform.position = center;

            // Update scale based on distance from the camera
            float distance = Vector3.Distance(_cam.transform.position, _transformGizmoRoot.transform.position);
            float scaleFactor = distance * Mathf.Tan(_cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            _transformGizmoRoot.transform.localScale = Vector3.one * (_gizmoSize * scaleFactor);
        }

        public bool RaycastGizmo(out Vector3 axis)
        {
            axis = Vector3.zero;

            int totalSelectedCount = _selectionManager.SelectedDrones.Count + _selectionManager.SelectedObstacles.Count;
            if (totalSelectedCount == 0 || !_transformGizmoRoot.activeSelf) return false;

            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == _movementGizmoX.gameObject) { axis = Vector3.right; return true; }
                if (hit.collider.gameObject == _movementGizmoY.gameObject) { axis = Vector3.up; return true; }
                if (hit.collider.gameObject == _movementGizmoZ.gameObject) { axis = Vector3.forward; return true; }
            }
            return false;
        }

        public void StartGizmoDrag(Vector3 axis)
        {
            IsDraggingGizmo = true;
            _dragAxis = axis;

            Vector3 planeNormal = _cam.transform.forward * -1;
            if (axis == Vector3.up) planeNormal = Vector3.Cross(_cam.transform.right, Vector3.up);

            _dragPlane = new Plane(planeNormal, _transformGizmoRoot.transform.position);

            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (_dragPlane.Raycast(ray, out float enter))
            {
                _dragStartIntersection = ray.GetPoint(enter);
            }

            _dragStartPositions.Clear();

            // Store start positions
            foreach (var d in _selectionManager.SelectedDrones) _dragStartPositions[d] = d.transform.position;
            foreach (var o in _selectionManager.SelectedObstacles) _dragStartPositions[o] = o.transform.position;
        }

        public void UpdateGizmoDrag()
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (_dragPlane.Raycast(ray, out float enter))
            {
                Vector3 currentIntersection = ray.GetPoint(enter);
                Vector3 moveDelta = currentIntersection - _dragStartIntersection;

                float moveAmount = Vector3.Dot(moveDelta, _dragAxis);
                Vector3 constrainedMove = _dragAxis * moveAmount;

                Vector3 newCenter = Vector3.zero;

                foreach (var kvp in _dragStartPositions)
                {
                    ISelectableView view = kvp.Key;
                    Vector3 startPos = kvp.Value;

                    Vector3 targetPos = startPos + constrainedMove;

                    if (_grid != null) targetPos = _grid.ConstrainPosition(targetPos.ToNumerics()).ToUnity();

                    view.transform.position = targetPos;
                    newCenter += view.transform.position;
                }

                _transformGizmoRoot.transform.position = newCenter / _dragStartPositions.Count;

                // Fire event so UI or other systems can react
                OnGizmoDragged?.Invoke(_transformGizmoRoot.transform.position);
            }
        }

        public void EndGizmoDrag()
        {
            IsDraggingGizmo = false;

            // Update core data by pattern matching types of views
            foreach (var view in _dragStartPositions.Keys)
            {
                if (view is DroneView drone)
                {
                    DroneManager.instance.UpdateDronePosition(drone.ID, drone.transform.position);
                }
                else if (view is ObstacleView obstacle)
                {
                    ObstacleManager.instance.UpdateObstaclePosition(obstacle.ID, obstacle.transform.position);
                }
            }
        }
    }
}