using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DroneSwampPathfiner.Unity.Managers;
using DroneSwampPathfiner.Unity.Visuals;
using UnityEditor.PackageManager;

namespace DroneSwampPathfiner.Unity.EditorTools
{
    public class ConfigEditorManager : MonoBehaviour
    {
        public static ConfigEditorManager instance;
        private void Awake() => instance = this;

        [Header("Editor Config")]
        public LayerMask droneLayer;

        public Color selectionBoxColor = new Color(0.2f, 0.6f, 1f, 0.3f);
        public Color selectionBoxBorderColor = new Color(0.2f, 0.6f, 1f, 1f);

        // Callback for UI
        public event Action<List<int>> OnSelectionChanged;

        private Camera _cam;
        private List<DroneView> _selectedDrones = new();

        // State variables
        private bool _isBoxSelecting;
        private Vector2 _boxStartPos;
        private Vector2 _boxEndPos;

        // Gizmos variables
        private GameObject _gizmoRoot;
        private Transform _gizmoX, _gizmoY, _gizmoZ;
        private bool _isDraggingGizmo;
        private Vector3 _dragAxis;
        private Plane _dragPlane;
        private Vector3 _dragStartIntersection;
        private Dictionary<DroneView, Vector3> _dragStartPositions = new();

        private void Start()
        {
            _cam = Camera.main;
            CreateRuntimeGizmo();
            UpdateGizmoState();
        }

        private void Update()
        {
            if (SimulationPlaybackManager.instance != null && SimulationPlaybackManager.instance.isPlaying)
            {
                if (_selectedDrones.Count > 0) ClearSelection();
                return;
            }

            HandleInput();
        }

        private void HandleInput()
        {
            // Single mouse click - select drone/start drag
            if (Input.GetMouseButtonDown(0))
            {
                // Ignore clicks through UI
                if (UIController.instance != null && UIController.instance.IsPointerOverUI())
                {
                    return;
                }

                // Clicked gizmo arrow
                if (RaycastGizmo(out Vector3 axis))
                {
                    StartGizmoDrag(axis);
                    return;
                }

                // Clicked a drone
                if (RaycastDrone(out DroneView drone))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        if (_selectedDrones.Contains(drone)) RemoveFromSelection(drone);
                        else AddToSelection(drone);
                    }
                    else
                    {
                        SelectSingle(drone);
                    }
                    return;
                }

                _isBoxSelecting = true;
                _boxStartPos = Input.mousePosition;
                _boxEndPos = Input.mousePosition;
            }
            // Mouse drag - box selection
            if (Input.GetMouseButton(0))
            {
                if (_isDraggingGizmo) UpdateGizmoDrag();
                else if (_isBoxSelecting) _boxEndPos = Input.mousePosition;
            }
            // Mouse up - end actions
            if (Input.GetMouseButtonUp(0))
            {
                if (_isDraggingGizmo) EndGizmoDrag();
                else if (_isBoxSelecting)
                {
                    _isBoxSelecting = false;
                    ApplyBoxSelection();
                }
            }
        }

        #region Raycasting and Selecting

        private bool RaycastGizmo(out Vector3 axis)
        {
            axis = Vector3.zero;
            if (_selectedDrones.Count == 0 || !_gizmoRoot.activeSelf) return false;

            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == _gizmoX.gameObject) { axis = Vector3.right; return true; }
                if (hit.collider.gameObject == _gizmoY.gameObject) { axis = Vector3.up; return true; }
                if (hit.collider.gameObject == _gizmoZ.gameObject) { axis = Vector3.forward; return true; }
            }
            return false;
        }

        private bool RaycastDrone(out DroneView drone)
        {
            drone = null;
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, droneLayer))
            {
                drone = hit.collider.GetComponentInParent<DroneView>();
                return drone != null;
            }
            return false;
        }

        private void SelectSingle(DroneView drone)
        {
            _selectedDrones.Clear();
            _selectedDrones.Add(drone);
            NotifySelectionChanged();
        }

        private void AddToSelection(DroneView drone)
        {
            if (!_selectedDrones.Contains(drone))
            {
                _selectedDrones.Add(drone);
                NotifySelectionChanged();
            }
        }

        private void RemoveFromSelection(DroneView drone)
        {
            _selectedDrones.Remove(drone);
            NotifySelectionChanged();
        }

        public void ClearSelection()
        {
            _selectedDrones.Clear();
            NotifySelectionChanged();
        }

        private void NotifySelectionChanged()
        {
            UpdateGizmoState();
            OnSelectionChanged?.Invoke(_selectedDrones.Select(d => d.DroneID).ToList());
        }

        #endregion

        #region Box Selection (group drone selection)

        private void ApplyBoxSelection()
        {
            if (!Input.GetKey(KeyCode.LeftShift)) ClearSelection();

            Vector2 min = Vector2.Min(_boxStartPos, _boxEndPos);
            Vector2 max = Vector2.Max(_boxStartPos, _boxEndPos);
            Rect selectionRect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);

            if (selectionRect.width < 10 && selectionRect.height < 10) return;

            // Go through all drones and get their views
            foreach (var coreDrone in DroneManager.instance.AllDroneModels)
            {
                var droneView = DroneManager.instance.GetDroneView(coreDrone.ID);
                if (droneView == null) continue;

                Vector3 screenPos = _cam.WorldToScreenPoint(droneView.transform.position);

                if (screenPos.z > 0 && selectionRect.Contains(new Vector2(screenPos.x, screenPos.y)))
                {
                    AddToSelection(droneView);
                }
            }
        }

        private void OnGUI()
        {
            if (_isBoxSelecting)
            {
                Rect rect = new Rect(_boxStartPos.x, Screen.height - _boxStartPos.y,
                                     _boxEndPos.x - _boxStartPos.x, -(_boxEndPos.y - _boxStartPos.y));

                DrawScreenRect(rect, selectionBoxColor);
                DrawScreenRectBorder(rect, 2, selectionBoxBorderColor);
            }
        }

        private static Texture2D _whiteTexture;
        private static Texture2D WhiteTexture
        {
            get
            {
                if (_whiteTexture == null)
                {
                    _whiteTexture = new Texture2D(1, 1);
                    _whiteTexture.SetPixel(0, 0, Color.white);
                    _whiteTexture.Apply();
                }
                return _whiteTexture;
            }
        }
        private static void DrawScreenRect(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, WhiteTexture);
            GUI.color = Color.white;
        }
        private static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
        {
            DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
            DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
            DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
            DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        }

        #endregion

        #region Gizmo Logic (movement arrows)

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

            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            arrow.GetComponent<Renderer>().material = mat;
        }

        private void UpdateGizmoState()
        {
            if (_selectedDrones.Count == 0)
            {
                _gizmoRoot.SetActive(false);
                return;
            }

            _gizmoRoot.SetActive(true);

            Vector3 center = Vector3.zero;
            foreach (var d in _selectedDrones) center += d.transform.position;
            center /= _selectedDrones.Count;

            _gizmoRoot.transform.position = center;
        }

        private void StartGizmoDrag(Vector3 axis)
        {
            _isDraggingGizmo = true;
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
            foreach (var d in _selectedDrones)
            {
                _dragStartPositions[d] = d.transform.position;
            }
        }

        private void UpdateGizmoDrag()
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
                    DroneView drone = kvp.Key;
                    Vector3 startPos = kvp.Value;

                    // Apply gizmos movement to the gameobject
                    drone.transform.position = startPos + constrainedMove;
                    newCenter += drone.transform.position;
                }

                _gizmoRoot.transform.position = newCenter / _selectedDrones.Count;
            }
        }

        private void EndGizmoDrag()
        {
            _isDraggingGizmo = false;

            // Change core data (Drone position)
            foreach (var drone in _selectedDrones)
            {
                DroneManager.instance.UpdateDronePosition(drone.DroneID, drone.transform.position);
            }

            //// Rebind ListView
            //UIController.instance.RefreshDroneList();
        }

        #endregion
    }
}