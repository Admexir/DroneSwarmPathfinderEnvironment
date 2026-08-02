using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DroneSwarmPathfinder.Core.Models;
using DroneSwarmPathfinder.Core.Environment;

namespace DroneSwarmPathfinder.Unity.UI
{
    /// <summary>
    /// Interface for any class that can determine if mouse is currently over it
    /// </summary>
    public interface IPointerStateProvider
    {
        bool IsPointerOverUI();
    }

    /// <summary>
    /// Defines the active transform tool in the 3D scene editor
    /// </summary>
    public enum EditorToolMode
    {
        Select,
        Move,
        Scale
    }

    [RequireComponent(typeof(UIDocument))]
    public class UIController : MonoBehaviour, IPointerStateProvider
    {
        private UIDocument _uiDocument;

        // Simulation controls
        private Button _playButton;
        private Label _timeScaleLabel;
        private SliderInt _timeScaleSlider;
        private IntegerField _timeScaleInput;
        private Button _stepForwardButton;
        private Button _stepBackButton;
        private Button _restartButton;

        // Drone inspector
        private ListView _droneListView;
        private VisualElement _droneDetailsPanel;
        private TextField _droneIdInput;
        private IntegerField _droneGroupInput;
        private Vector3Field _dronePositionInput;

        // Obstacle inspector
        private ListView _obstacleListView;
        private VisualElement _obstacleDetailsPanel;
        private TextField _obstacleIdInput;
        private Vector3Field _obstaclePositionInput;
        private Vector3Field _obstacleSizeInput;

        // Toolbar UI
        private VisualElement _floatingToolbar;
        private VisualElement _dragHandle;
        private Button _btnSelect;
        private Button _btnMove;
        private Button _btnScale;

        // Toolbar dragging state
        private bool _isDraggingToolbar = false;
        private Vector2 _toolbarDragStartMousePos;
        private Vector2 _toolbarDragStartPos;

        // Toolbar events for other scripts to subscribe to
        public event Action<EditorToolMode> OnToolClickedEvent;

        // Simulation events for other scripts to subscribe to 
        public event Action<int> OnTimeScaleChangedEvent;
        public event Action OnPlayClickedEvent;
        public event Action OnStepForwardClickedEvent;
        public event Action OnStepBackClickedEvent;
        public event Action OnRestartClickedEvent;

        // Editor actions events for other scripts to subscribe to
        public event Action OnAddDroneClickedEvent;
        public event Action OnAddObstacleClickedEvent;
        public event Action OnRemoveSelectedClickedEvent;
        public event Action OnLoadConfigClickedEvent;
        public event Action OnPlaySimulationClickedEvent;
        public event Action OnExportConfigClickedEvent;

        // Drones events for other scripts to subscribe to
        public event Action<IEnumerable<object>> OnDroneListSelectionChangedEvent;
        public event Action<int> OnDroneGroupChangedEvent;
        public event Action<Vector3> OnDronePositionChangedEvent;

        // Obstacles events for other scripts to subscribe to
        public event Action<IEnumerable<object>> OnObstacleListSelectionChangedEvent;
        public event Action<Vector3> OnObstaclePositionChangedEvent;
        public event Action<Vector3> OnObstacleSizeChangedEvent;

        /// <summary>
        /// Gets if mouse is currently over an interactible UI element (equivalent of EventSystem.Current.IsPointerOverGameObject())
        /// </summary>
        public bool IsPointerOverUI()
        {
            if (_uiDocument == null || _uiDocument.rootVisualElement == null || _uiDocument.rootVisualElement.panel == null)
                return false;

            // Input.mousePosition coordinates are from bottom left corner, while UI coordinates from top left -> transform
            Vector2 mousePos = Input.mousePosition;
            Vector2 uiPos = new Vector2(mousePos.x, Screen.height - mousePos.y);

            VisualElement picked = _uiDocument.rootVisualElement.panel.Pick(uiPos);
            return picked != null;
        }

        private void OnEnable()
        {
            _uiDocument = GetComponent<UIDocument>();
            var root = _uiDocument.rootVisualElement;
            root.RegisterCallback<NavigationMoveEvent>(evt => evt.PreventDefault()); // Should make all UI ignore arrow key-navigation

            BindSimulationUI(root);
            BindDroneUI(root);
            BindObstacleUI(root);
            BindActionButtons(root);
            BindToolbarUI(root);

            SetPlaybackUIVisibility(false);
        }

        private void BindSimulationUI(VisualElement root)
        {
            // Simulation controls UI
            _playButton = root.Q<Button>("btn-play");
            _timeScaleLabel = root.Q<Label>("time-scale-label");
            _timeScaleSlider = root.Q<SliderInt>("time-scale-slider");
            _stepForwardButton = root.Q<Button>("btn-step-forward");
            _stepBackButton = root.Q<Button>("btn-step-back");
            _restartButton = root.Q<Button>("btn-restart");

            if (_timeScaleSlider != null)
            {
                _timeScaleSlider.RegisterValueChangedCallback(evt => OnTimeScaleChangedEvent?.Invoke(evt.newValue));
                if (_timeScaleLabel != null)
                    _timeScaleLabel.text = $"Time Scale: {_timeScaleSlider.value}%";
            }

            if (_timeScaleLabel != null && _timeScaleSlider != null)
            {
                _timeScaleInput = new IntegerField();
                _timeScaleInput.style.display = DisplayStyle.None;
                _timeScaleInput.style.marginBottom = 2;

                _timeScaleLabel.parent.Insert(_timeScaleLabel.parent.IndexOf(_timeScaleLabel), _timeScaleInput);

                _timeScaleLabel.RegisterCallback<PointerDownEvent>(OnLabelClicked);
                _timeScaleInput.RegisterCallback<KeyDownEvent>(OnInputKeyDown);
                _timeScaleInput.RegisterCallback<FocusOutEvent>(OnInputFocusOut);
            }

            if (_playButton != null) _playButton.clicked += () => OnPlayClickedEvent?.Invoke();
            if (_stepForwardButton != null) _stepForwardButton.clicked += () => OnStepForwardClickedEvent?.Invoke();
            if (_stepBackButton != null) _stepBackButton.clicked += () => OnStepBackClickedEvent?.Invoke();
            if (_restartButton != null) _restartButton.clicked += () => OnRestartClickedEvent?.Invoke();
        }

        private void BindDroneUI(VisualElement root)
        {
            // Config editor UI - Drones
            _droneListView = root.Q<ListView>("drone-list-view");
            _droneDetailsPanel = root.Q<VisualElement>("drone-details-panel");
            _droneIdInput = root.Q<TextField>("input-drone-id");
            _droneGroupInput = root.Q<IntegerField>("input-drone-group");
            _dronePositionInput = root.Q<Vector3Field>("input-drone-position");

            if (_droneListView != null)
            {
                _droneListView.makeItem = () => new Label();
                _droneListView.bindItem = (element, index) =>
                {
                    var label = element as Label;
                    var drone = (Drone)_droneListView.itemsSource[index];
                    label.text = $"Drone {drone.ID} (group: {drone.GroupId})";
                };

                _droneListView.selectionChanged += (selection) => OnDroneListSelectionChangedEvent?.Invoke(selection);
            }

            _droneGroupInput?.RegisterValueChangedCallback(evt => OnDroneGroupChangedEvent?.Invoke(evt.newValue));
            _dronePositionInput?.RegisterValueChangedCallback(evt => OnDronePositionChangedEvent?.Invoke(evt.newValue));

            if (_droneDetailsPanel != null) _droneDetailsPanel.style.display = DisplayStyle.None;
        }

        private void BindObstacleUI(VisualElement root)
        {
            // Config editor UI - Obstacles
            _obstacleListView = root.Q<ListView>("obstacle-list-view");
            _obstacleDetailsPanel = root.Q<VisualElement>("obstacle-details-panel");
            _obstacleIdInput = root.Q<TextField>("input-obstacle-id");
            _obstaclePositionInput = root.Q<Vector3Field>("input-obstacle-position");
            _obstacleSizeInput = root.Q<Vector3Field>("input-obstacle-size");

            if (_obstacleListView != null)
            {
                _obstacleListView.makeItem = () => new Label();
                _obstacleListView.bindItem = (element, index) =>
                {
                    var label = element as Label;
                    var obs = (BoxObstacle)_obstacleListView.itemsSource[index];
                    label.text = $"Obstacle {obs.ID}";
                };

                _obstacleListView.selectionChanged += (selection) => OnObstacleListSelectionChangedEvent?.Invoke(selection);
            }

            _obstaclePositionInput?.RegisterValueChangedCallback(evt => OnObstaclePositionChangedEvent?.Invoke(evt.newValue));
            _obstacleSizeInput?.RegisterValueChangedCallback(evt => OnObstacleSizeChangedEvent?.Invoke(evt.newValue));

            if (_obstacleDetailsPanel != null) _obstacleDetailsPanel.style.display = DisplayStyle.None;
        }

        private void BindActionButtons(VisualElement root)
        {
            var addDroneButton = root.Q<Button>("btn-add-drone");
            var addObstacleButton = root.Q<Button>("btn-add-obstacle");
            var removeSelectedButton = root.Q<Button>("btn-remove-selected");
            var loadConfigButton = root.Q<Button>("btn-load-config");
            var playSimulationButton = root.Q<Button>("btn-play-sim");
            var exportConfigButton = root.Q<Button>("btn-export-config");

            if (addDroneButton != null) addDroneButton.clicked += () => OnAddDroneClickedEvent?.Invoke();
            if (addObstacleButton != null) addObstacleButton.clicked += () => OnAddObstacleClickedEvent?.Invoke();
            if (removeSelectedButton != null) removeSelectedButton.clicked += () => OnRemoveSelectedClickedEvent?.Invoke();
            if (loadConfigButton != null) loadConfigButton.clicked += () => OnLoadConfigClickedEvent?.Invoke();
            if (playSimulationButton != null) playSimulationButton.clicked += () => OnPlaySimulationClickedEvent?.Invoke();
            if (exportConfigButton != null) exportConfigButton.clicked += () => OnExportConfigClickedEvent?.Invoke();
        }

        private void BindToolbarUI(VisualElement root)
        {
            _floatingToolbar = root.Q<VisualElement>("floating-toolbar");
            _dragHandle = root.Q<VisualElement>("drag-handle");
            _btnSelect = root.Q<Button>("tool-btn-select");
            _btnMove = root.Q<Button>("tool-btn-move");
            _btnScale = root.Q<Button>("tool-btn-scale");

            // 1. Bind Toolbar Dragging Logic (Purely visual, stays entirely in the View)
            if (_dragHandle != null && _floatingToolbar != null)
            {
                _dragHandle.RegisterCallback<PointerDownEvent>(OnToolbarPointerDown);
                _dragHandle.RegisterCallback<PointerMoveEvent>(OnToolbarPointerMove);
                _dragHandle.RegisterCallback<PointerUpEvent>(OnToolbarPointerUp);
                _dragHandle.RegisterCallback<PointerCaptureOutEvent>(OnToolbarPointerUp);
            }

            // 2. Bind Tool Buttons
            if (_btnSelect != null) _btnSelect.clicked += () => OnToolClickedEvent?.Invoke(EditorToolMode.Select);
            if (_btnMove != null) _btnMove.clicked += () => OnToolClickedEvent?.Invoke(EditorToolMode.Move);
            if (_btnScale != null) _btnScale.clicked += () => OnToolClickedEvent?.Invoke(EditorToolMode.Scale);
        }

        #region Toolbar Dragging Logic (UI Toolkit)

        private void OnToolbarPointerDown(PointerDownEvent evt)
        {
            _isDraggingToolbar = true;
            _dragHandle.CapturePointer(evt.pointerId);
            _toolbarDragStartMousePos = evt.position;
            // resolvedStyle gets the current position 
            _toolbarDragStartPos = new Vector2(_floatingToolbar.resolvedStyle.left, _floatingToolbar.resolvedStyle.top);
            evt.StopPropagation();
        }

        private void OnToolbarPointerMove(PointerMoveEvent evt)
        {
            if (!_isDraggingToolbar || !_dragHandle.HasPointerCapture(evt.pointerId)) return;

            Vector2 delta = (Vector2)evt.position - _toolbarDragStartMousePos;
            _floatingToolbar.style.left = _toolbarDragStartPos.x + delta.x;
            _floatingToolbar.style.top = _toolbarDragStartPos.y + delta.y;

            evt.StopPropagation();
        }

        private void OnToolbarPointerUp(EventBase evt)
        {
            if (evt is IPointerEvent pointerEvent && _isDraggingToolbar && _dragHandle.HasPointerCapture(pointerEvent.pointerId))
            {
                _isDraggingToolbar = false;
                _dragHandle.ReleasePointer(pointerEvent.pointerId);
                evt.StopPropagation();
            }
        }

        #endregion

        #region Public view API (for presenter)
        /// <summary>
        /// Updates the highlighted button of the active tool button
        /// </summary>
        public void SetActiveToolVisual(EditorToolMode mode)
        {
            // Reset all to default color
            // TODO: unhardcode color
            var defaultColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 0f));
            var activeColor = new StyleColor(new Color(0.27f, 0.27f, 0.27f, 1f));

            if (_btnSelect != null) _btnSelect.style.backgroundColor = (mode == EditorToolMode.Select) ? activeColor : defaultColor;
            if (_btnMove != null) _btnMove.style.backgroundColor = (mode == EditorToolMode.Move) ? activeColor : defaultColor;
            if (_btnScale != null) _btnScale.style.backgroundColor = (mode == EditorToolMode.Scale) ? activeColor : defaultColor;
        }

        public void PopulateDroneList(List<Drone> drones)
        {
            if (_droneListView == null) return;
            _droneListView.itemsSource = drones;
            _droneListView.Rebuild();
        }

        public void SetDroneListSelectionWithoutNotify(List<int> indices)
        {
            _droneListView?.SetSelectionWithoutNotify(indices);
        }

        public void ClearDroneListSelection()
        {
            _droneListView?.ClearSelection();
        }

        public void ShowDroneDetails(Drone drone, Vector3 unityPosition)
        {
            if (_droneIdInput != null) _droneIdInput.value = drone.ID.ToString();
            _droneGroupInput?.SetValueWithoutNotify(drone.GroupId);
            _dronePositionInput?.SetValueWithoutNotify(unityPosition);
            if (_droneDetailsPanel != null) _droneDetailsPanel.style.display = DisplayStyle.Flex;
        }

        public void HideDroneDetails()
        {
            if (_droneDetailsPanel != null) _droneDetailsPanel.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Updates the inspector fields for drone position without triggering the change event
        /// </summary>
        public void UpdateDronePositionField(Vector3 newPos)
        {
            _dronePositionInput?.SetValueWithoutNotify(newPos);
        }

        public void PopulateObstacleList(List<BoxObstacle> obstacles)
        {
            if (_obstacleListView == null) return;
            _obstacleListView.itemsSource = obstacles;
            _obstacleListView.Rebuild();
        }

        public void SetObstacleListSelectionWithoutNotify(List<int> indices)
        {
            _obstacleListView?.SetSelectionWithoutNotify(indices);
        }

        public void ClearObstacleListSelection()
        {
            _obstacleListView?.ClearSelection();
        }

        public void ShowObstacleDetails(BoxObstacle obstacle, Vector3 unityPosition, Vector3 unitySize)
        {
            if (_obstacleIdInput != null) _obstacleIdInput.value = obstacle.ID.ToString();
            _obstaclePositionInput?.SetValueWithoutNotify(unityPosition);
            _obstacleSizeInput?.SetValueWithoutNotify(unitySize);
            if (_obstacleDetailsPanel != null) _obstacleDetailsPanel.style.display = DisplayStyle.Flex;
        }

        public void HideObstacleDetails()
        {
            if (_obstacleDetailsPanel != null) _obstacleDetailsPanel.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Updates the inspector fields for obstacle position without triggering the change event
        /// </summary>
        public void UpdateObstaclePositionField(Vector3 newPos)
        {
            _obstaclePositionInput?.SetValueWithoutNotify(newPos);
        }

        public void UpdateTimeScaleDisplay(int value)
        {
            if (_timeScaleLabel != null) _timeScaleLabel.text = $"Time Scale: {value}%";
        }

        public void RefreshPlayButtonState(bool isPlaying)
        {
            if (_playButton != null) _playButton.text = isPlaying ? "Pause" : "Play";
        }

        public void SetPlaybackUIVisibility(bool isVisible)
        {
            var displayState = isVisible ? DisplayStyle.Flex : DisplayStyle.None;

            if (_playButton != null) _playButton.style.display = displayState;
            if (_timeScaleLabel != null) _timeScaleLabel.style.display = displayState;
            if (_timeScaleSlider != null) _timeScaleSlider.style.display = displayState;
            if (_stepForwardButton != null) _stepForwardButton.style.display = displayState;
            if (_stepBackButton != null) _stepBackButton.style.display = displayState;
            if (_restartButton != null) _restartButton.style.display = displayState;

            // If we are hiding UI while currently editing the time scale, hide the input field too
            if (!isVisible && _timeScaleInput != null) _timeScaleInput.style.display = DisplayStyle.None;
        }

        #endregion

        #region Internal Time Scale Input Logic

        private void OnLabelClicked(PointerDownEvent evt)
        {
            if (evt.clickCount == 2)
            {
                _timeScaleLabel.style.display = DisplayStyle.None;
                _timeScaleInput.style.display = DisplayStyle.Flex;
                _timeScaleInput.value = _timeScaleSlider.value;
                _timeScaleInput.schedule.Execute(() =>
                {
                    _timeScaleInput.Focus();
                    _timeScaleInput.SelectAll();
                }).StartingIn(10);
            }
        }

        private void OnInputKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                CommitTimeScaleInput();
            }
        }

        private void OnInputFocusOut(FocusOutEvent evt)
        {
            CommitTimeScaleInput();
        }

        private void CommitTimeScaleInput()
        {
            if (_timeScaleInput.style.display == DisplayStyle.None) return;
            int clampedValue = Mathf.Clamp(_timeScaleInput.value, 0, 100);
            _timeScaleSlider.value = clampedValue; // This triggers the slider callback automatically
            _timeScaleInput.style.display = DisplayStyle.None;
            _timeScaleLabel.style.display = DisplayStyle.Flex;
        }

        #endregion
    }
}