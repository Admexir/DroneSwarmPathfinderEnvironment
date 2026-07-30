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
        private GameObject _gizmoRoot;
        private Transform _gizmoX, _gizmoY, _gizmoZ;

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
            CreateRuntimeGizmo();

            // Listen to selection changes for (currently...) both arrays
            _selectionManager.OnDroneSelectionChanged += (_) => UpdateGizmoState();
            _selectionManager.OnObstacleSelectionChanged += (_) => UpdateGizmoState();
        }

        public void InitializeGrid(float gridSize)
        {
            _grid = new DiscreteGrid(gridSize);
        }

        private void CreateRuntimeGizmo()
        {
            _gizmoRoot = new GameObject("RuntimeGizmo");

            _gizmoX = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            SetupGizmoArrow(_gizmoX, Color.red, new Vector3(1, 0, 0));

            _gizmoY = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            SetupGizmoArrow(_gizmoY, Color.green, new Vector3(0, 1, 0));

            _gizmoZ = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            SetupGizmoArrow(_gizmoZ, Color.blue, new Vector3(0, 0, 1));

            _gizmoRoot.SetActive(false);
        }

        private void SetupGizmoArrow(Transform arrow, Color color, Vector3 direction)
        {
            arrow.SetParent(_gizmoRoot.transform);
            arrow.localScale = new Vector3(0.2f, 1f, 0.2f);
            arrow.up = direction;
            arrow.localPosition = direction * 1f;

            var col = arrow.GetComponent<Collider>();
            col.isTrigger = true;

            var mat = new Material(Shader.Find("Standard")) { color = color };
            arrow.GetComponent<Renderer>().material = mat;
        }

        public void UpdateGizmoState()
        {
            int totalSelectedCount = _selectionManager.SelectedDrones.Count + _selectionManager.SelectedObstacles.Count;

            if (totalSelectedCount == 0)
            {
                _gizmoRoot.SetActive(false);
                return;
            }

            _gizmoRoot.SetActive(true);

            Vector3 center = Vector3.zero;

            // Combine positions from both drones and obstacles
            foreach (var d in _selectionManager.SelectedDrones) center += d.transform.position;
            foreach (var o in _selectionManager.SelectedObstacles) center += o.transform.position;

            center /= totalSelectedCount;

            _gizmoRoot.transform.position = center;
        }

        public bool RaycastGizmo(out Vector3 axis)
        {
            axis = Vector3.zero;

            int totalSelectedCount = _selectionManager.SelectedDrones.Count + _selectionManager.SelectedObstacles.Count;
            if (totalSelectedCount == 0 || !_gizmoRoot.activeSelf) return false;

            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == _gizmoX.gameObject) { axis = Vector3.right; return true; }
                if (hit.collider.gameObject == _gizmoY.gameObject) { axis = Vector3.up; return true; }
                if (hit.collider.gameObject == _gizmoZ.gameObject) { axis = Vector3.forward; return true; }
            }
            return false;
        }

        public void StartGizmoDrag(Vector3 axis)
        {
            IsDraggingGizmo = true;
            _dragAxis = axis;

            Vector3 planeNormal = _cam.transform.forward * -1;
            if (axis == Vector3.up) planeNormal = Vector3.Cross(_cam.transform.right, Vector3.up);

            _dragPlane = new Plane(planeNormal, _gizmoRoot.transform.position);

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

                _gizmoRoot.transform.position = newCenter / _dragStartPositions.Count;

                // Fire event so UI or other systems can react
                OnGizmoDragged?.Invoke(_gizmoRoot.transform.position);
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