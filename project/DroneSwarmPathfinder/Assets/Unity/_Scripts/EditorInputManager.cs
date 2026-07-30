using DroneSwarmPathfinder.Unity.Managers;
using DroneSwarmPathfinder.Unity.UI;
using DroneSwarmPathfinder.Unity.Visuals;
using System.Linq;
using UnityEngine;

namespace DroneSwarmPathfinder.Unity.EditorTools
{
    [RequireComponent(typeof(SelectionManager), typeof(GizmoManager), typeof(BoxSelectionVisualizer))]
    public class EditorInputManager : MonoBehaviour
    {
        [Header("Input Config")]
        public LayerMask selectableLayers;

        private SelectionManager _selectionManager;
        private GizmoManager _gizmoController;
        private BoxSelectionVisualizer _boxSelectionVisualizer;
        private Camera _cam;

        private IPointerStateProvider _pointerStateProvider;

        private void Awake()
        {
            _selectionManager = GetComponent<SelectionManager>();
            _gizmoController = GetComponent<GizmoManager>();
            _boxSelectionVisualizer = GetComponent<BoxSelectionVisualizer>();
            _cam = Camera.main;
            _pointerStateProvider = FindAnyObjectByType<UIController>();
        }

        private void Update()
        {
            // Block editor input if simulation is playing
            if (SimulationPlaybackManager.instance != null && SimulationPlaybackManager.instance.isPlaying)
            {
                if (_selectionManager.SelectedDrones.Count > 0 || _selectionManager.SelectedObstacles.Count > 0)
                    _selectionManager.ClearSelection();
                return;
            }

            HandleInput();

            // Moves gizmo with the drone in case user moves it using other ways
            if (!_gizmoController.IsDraggingGizmo && (_selectionManager.SelectedDrones.Count > 0 || _selectionManager.SelectedObstacles.Count > 0))
            {
                _gizmoController.UpdateGizmoState();
            }
        }

        private void HandleInput()
        {
            // Ignore clicks through UI
            if (Input.GetMouseButtonDown(0) && _pointerStateProvider != null && _pointerStateProvider.IsPointerOverUI())
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (_gizmoController.RaycastGizmo(out Vector3 axis))
                {
                    _gizmoController.StartGizmoDrag(axis);
                    return;
                }

                if (RaycastSelectable(out ISelectableView selectable))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        if (_selectionManager.IsSelected(selectable))
                            _selectionManager.RemoveFromSelection(selectable);
                        else
                            _selectionManager.AddToSelection(selectable);
                    }
                    else
                    {
                        _selectionManager.SelectSingle(selectable);
                    }
                    return;
                }

                _boxSelectionVisualizer.StartBoxSelection(Input.mousePosition);
            }

            if (Input.GetMouseButton(0))
            {
                if (_gizmoController.IsDraggingGizmo) _gizmoController.UpdateGizmoDrag();
                else if (_boxSelectionVisualizer.IsBoxSelecting) _boxSelectionVisualizer.UpdateBoxSelection(Input.mousePosition);
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (_gizmoController.IsDraggingGizmo) _gizmoController.EndGizmoDrag();
                else if (_boxSelectionVisualizer.IsBoxSelecting) _boxSelectionVisualizer.EndBoxSelection();
            }
        }

        private bool RaycastSelectable(out ISelectableView selectable)
        {
            selectable = null;
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, selectableLayers))
            {
                selectable = hit.collider.GetComponentInParent<ISelectableView>();
                return selectable != null;
            }
            return false;
        }
    }
}