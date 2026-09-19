using DroneSwarmPathfinder.Core.Environment;
using DroneSwarmPathfinder.Unity.Managers;
using DroneSwarmPathfinder.Unity.UI;
using DroneSwarmPathfinder.Unity.Visuals;
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
        private EnvironmentManager _environmentManager;
        private Camera _cam;

        // Gizmos variables
        [SerializeField] private GameObject _transformGizmoRoot;
        [SerializeField] private float _gizmoSize;
        [SerializeField] private LayerMask gizmoLayer;

        // Roots for toggling visibility
        private GameObject _movementGizmosRoot;
        private GameObject _sizeGizmosRoot;

        private Transform _movementGizmoX, _movementGizmoY, _movementGizmoZ;
        private Transform _movementGizmoXY, _movementGizmoXZ, _movementGizmoYZ;
        private Transform _sizeGizmoX, _sizeGizmoY, _sizeGizmoZ;

        public bool IsDraggingGizmo { get; private set; }
        private Vector3 _dragAxisOrNormal;
        private bool _isPlaneDrag;
        private Plane _dragPlane;
        private Vector3 _dragStartIntersection;

        public EditorToolMode CurrentToolMode { get; private set; } = EditorToolMode.Move;

        private Dictionary<ISelectableView, (Vector3 Position, Vector3 Scale)> _dragStartStates = new();

        private void Awake()
        {
            _selectionManager = GetComponent<SelectionManager>();
            _environmentManager = GetComponent<EnvironmentManager>();
            _cam = Camera.main;
        }

        private void Start()
        {
            // Listen to selection changes for (currently...) both arrays
            _selectionManager.OnDroneSelectionChanged += (_) => UpdateGizmoState();
            _selectionManager.OnObstacleSelectionChanged += (_) => UpdateGizmoState();

            InitializeGizmo();
        }

        private void InitializeGizmo()
        {
            // Not sure if it has any impact on performance, but search only the children (TODO: maybe revert to GameObject.Find to prevent it from breaking when changing the scene hierarchy?)
            _movementGizmosRoot = _transformGizmoRoot.transform.Find("Movement Gizmos").gameObject; 
            _sizeGizmosRoot = _transformGizmoRoot.transform.Find("Size Gizmos").gameObject;

            var mArrowsRoot = _movementGizmosRoot.transform.Find("Movement Arrows");
            _movementGizmoX = mArrowsRoot.GetChild(0);
            _movementGizmoY = mArrowsRoot.GetChild(1);
            _movementGizmoZ = mArrowsRoot.GetChild(2);

            var sArrowsRoot = _sizeGizmosRoot.transform.Find("Size Arrows");
            _sizeGizmoX = sArrowsRoot.GetChild(0);
            _sizeGizmoY = sArrowsRoot.GetChild(1);
            _sizeGizmoZ = sArrowsRoot.GetChild(2);

            var mSquaresRoot = _movementGizmosRoot.transform.Find("Movement Squares");
            if (mSquaresRoot != null)
            {
                _movementGizmoXY = mSquaresRoot.GetChild(0);
                _movementGizmoXZ = mSquaresRoot.GetChild(1);
                _movementGizmoYZ = mSquaresRoot.GetChild(2);
            }

            _movementGizmosRoot.SetActive(false);
            _sizeGizmosRoot.SetActive(false);
        }

        /// <summary>
        /// Sets the currently selected tool
        /// </summary>
        public void SetToolMode(EditorToolMode mode)
        {
            CurrentToolMode = mode;
            UpdateGizmoState(); // Force a visual update
        }

        public void UpdateGizmoState()
        {
            int totalSelectedCount = _selectionManager.SelectedDrones.Count + _selectionManager.SelectedObstacles.Count;

            if (totalSelectedCount == 0 || CurrentToolMode == EditorToolMode.Select)
            {
                _transformGizmoRoot.SetActive(false);
                return;
            }

            _transformGizmoRoot.SetActive(true);

            // Toggle tool gizmo roots based on current mode
            _movementGizmosRoot.SetActive(CurrentToolMode == EditorToolMode.Move);
            _sizeGizmosRoot.SetActive(CurrentToolMode == EditorToolMode.Scale);

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

        public bool RaycastGizmo(out Vector3 constraint, out bool isPlane)
        {
            constraint = Vector3.zero;
            isPlane = false;

            int totalSelectedCount = _selectionManager.SelectedDrones.Count + _selectionManager.SelectedObstacles.Count;
            if (totalSelectedCount == 0 || !_transformGizmoRoot.activeSelf) return false;

            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, gizmoLayer))
            {
                // Route raycast logic based on the active tool
                if (CurrentToolMode == EditorToolMode.Move)
                {
                    if (hit.collider.gameObject == _movementGizmoX.gameObject) { constraint = Vector3.right; return true; }
                    if (hit.collider.gameObject == _movementGizmoY.gameObject) { constraint = Vector3.up; return true; }
                    if (hit.collider.gameObject == _movementGizmoZ.gameObject) { constraint = Vector3.forward; return true; }

                    if (_movementGizmoXY != null && hit.collider.gameObject == _movementGizmoXY.gameObject) { constraint = Vector3.forward; isPlane = true; return true; }
                    if (_movementGizmoXZ != null && hit.collider.gameObject == _movementGizmoXZ.gameObject) { constraint = Vector3.up; isPlane = true; return true; }
                    if (_movementGizmoYZ != null && hit.collider.gameObject == _movementGizmoYZ.gameObject) { constraint = Vector3.right; isPlane = true; return true; }
                }
                else if (CurrentToolMode == EditorToolMode.Scale)
                {
                    if (hit.collider.gameObject == _sizeGizmoX.gameObject) { constraint = Vector3.right; return true; }
                    if (hit.collider.gameObject == _sizeGizmoY.gameObject) { constraint = Vector3.up; return true; }
                    if (hit.collider.gameObject == _sizeGizmoZ.gameObject) { constraint = Vector3.forward; return true; }
                }
            }
            return false;
        }

        public void StartGizmoDrag(Vector3 constraint, bool isPlane)
        {
            IsDraggingGizmo = true;
            _dragAxisOrNormal = constraint;
            _isPlaneDrag = isPlane;

            Vector3 planeNormal;
            // If dragging a plane, the constraint is the normal of the drag plane
            if (isPlane)
            {
                planeNormal = constraint;
            }
            // If dragging an axis, create a virtual plane facing the camera to raycast against
            else
            {
                planeNormal = _cam.transform.forward * -1;
                if (constraint == Vector3.up) planeNormal = Vector3.Cross(_cam.transform.right, Vector3.up);
            }

            _dragPlane = new Plane(planeNormal, _transformGizmoRoot.transform.position);

            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (_dragPlane.Raycast(ray, out float enter))
            {
                _dragStartIntersection = ray.GetPoint(enter);
            }

            _dragStartStates.Clear();

            // Store start positions and scales
            foreach (var d in _selectionManager.SelectedDrones)
            {
                _dragStartStates[d] = (d.transform.position, d.transform.localScale);
            }
            foreach (var o in _selectionManager.SelectedObstacles)
            {
                _dragStartStates[o] = (o.transform.position, o.transform.localScale);
            }
        }

        public void UpdateGizmoDrag()
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (_dragPlane.Raycast(ray, out float enter))
            {
                Vector3 currentIntersection = ray.GetPoint(enter);
                Vector3 moveDelta = currentIntersection - _dragStartIntersection;

                float moveAmount = Vector3.Dot(moveDelta, _dragAxisOrNormal);
                Vector3 newCenter = Vector3.zero;

                foreach (var kvp in _dragStartStates)
                {
                    ISelectableView view = kvp.Key;

                    if (CurrentToolMode == EditorToolMode.Move)
                    {
                        Vector3 constrainedMove;
                        if (_isPlaneDrag)
                        {
                            // Project movement on the dragging plane
                            constrainedMove = Vector3.ProjectOnPlane(moveDelta, _dragAxisOrNormal);
                        }
                        else
                        {
                            // Project movement on the axis
                            constrainedMove = _dragAxisOrNormal * moveAmount;
                        }

                        Vector3 targetPos = kvp.Value.Position + constrainedMove;
                        if (_environmentManager.Grid != null) targetPos = _environmentManager.Grid.ConstrainPosition(targetPos.ToNumerics()).ToUnity(); // Constrain the movement to the grid

                        view.transform.position = targetPos;
                    }
                    else if (CurrentToolMode == EditorToolMode.Scale)
                    {
                        // Only obstacles (and not drones) can be scaled
                        if (view is ObstacleView)
                        {
                            // Directly access the tuple value without a dictionary lookup
                            Vector3 startScale = kvp.Value.Scale;

                            Vector3 constrainedScale = _dragAxisOrNormal * (moveAmount * 2f);
                            Vector3 targetScale = startScale + constrainedScale;

                            // Prevent edge case scales
                            targetScale.x = Mathf.Max(0.1f, targetScale.x);
                            targetScale.y = Mathf.Max(0.1f, targetScale.y);
                            targetScale.z = Mathf.Max(0.1f, targetScale.z);

                            view.transform.localScale = targetScale;
                        }
                    }

                    newCenter += view.transform.position;
                }

                _transformGizmoRoot.transform.position = newCenter / _dragStartStates.Count;

                // Fire event so UI or other systems can react (passing center pos)
                OnGizmoDragged?.Invoke(_transformGizmoRoot.transform.position);
            }
        }

        public void EndGizmoDrag()
        {
            IsDraggingGizmo = false;

            // Update core data by pattern matching types of views
            foreach (var view in _dragStartStates.Keys)
            {
                if (CurrentToolMode == EditorToolMode.Move)
                {
                    if (view is DroneView drone)
                        DroneManager.instance.UpdateDronePosition(drone.ID, drone.transform.position);
                    else if (view is ObstacleView obstacle)
                        EnvironmentManager.instance.UpdateObstaclePosition(obstacle.ID, obstacle.transform.position);
                }
                else if (CurrentToolMode == EditorToolMode.Scale)
                {
                    // (No scaling for drones)
                    if (view is ObstacleView obstacle)
                        EnvironmentManager.instance.UpdateObstacleSize(obstacle.ID, obstacle.transform.localScale);
                }
            }
            OnGizmoDragged?.Invoke(_transformGizmoRoot.transform.position); // Update the onGizmoDragged logic callback one last time
        }
    }
}