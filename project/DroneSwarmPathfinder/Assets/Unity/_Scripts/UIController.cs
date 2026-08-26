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

    /// <summary>
    /// Defines the active tab in the left UI panel
    /// </summary>
    public enum UITabMode
    {
        Editor,
        Playback,
        File
    }

    [RequireComponent(typeof(UIDocument))]
    public class UIController : MonoBehaviour, IPointerStateProvider
    {
        private UIDocument _uiDocument;

        // File panel UI
        private Button _btnStartConfig;
        private Button _btnTargetConfig;
        private Toggle _toggleCurrentScene;
        private VisualElement _iconStartConfig;
        private VisualElement _iconTargetConfig;
        private Toggle _usePhysicsToggle;
        [SerializeField] private Texture2D _fileIcon;
        private VisualElement _configWarningContainer;
        private Label _configWarningLabel;

        // Algorithm UI
        private DropdownField _algorithmDropdown;
        private Button _btnLoadAlgorithm;
        private Button _playSimulationButton;


        // Tab UI
        private Button _tabBtnEditor;
        private Button _tabBtnPlayback;
        private Button _tabBtnFile;
        private VisualElement _panelEditor;
        private VisualElement _panelPlayback;
        private VisualElement _panelFile;

        // Editor UI
        private IntegerField _gridSizeInput;
        private VisualElement _droneWarningContainer;
        private Label _droneWarningLabel;

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
        public event Action<int> OnGridSizeChangedEvent;

        // Drones events for other scripts to subscribe to
        public event Action<IEnumerable<object>> OnDroneListSelectionChangedEvent;
        public event Action<int> OnDroneGroupChangedEvent;
        public event Action<Vector3> OnDronePositionChangedEvent;

        // Obstacles events for other scripts to subscribe to
        public event Action<IEnumerable<object>> OnObstacleListSelectionChangedEvent;
        public event Action<Vector3> OnObstaclePositionChangedEvent;
        public event Action<Vector3> OnObstacleSizeChangedEvent;

        // File panel events for other scripts to subscribe to
        public event Action OnSelectStartConfigClickedEvent;
        public event Action OnSelectTargetConfigClickedEvent;
        public event Action<bool> OnUseCurrentSceneToggledEvent;
        public event Action OnLoadEnvironmentClickedEvent;
        public event Action OnExportEnvironmentClickedEvent;
        public event Action<bool> OnUsePhysicsToggledEvent;
        // (algorithm selection events)
        public event Action OnLoadAlgorithmClickedEvent;
        public event Action<string> OnAlgorithmSelectedEvent;

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

            BindTabsUI(root);
            BindSimulationUI(root);
            BindDroneUI(root);
            BindObstacleUI(root);
            BindConfigEditorUI(root);
            BindToolbarUI(root);
            BindFileUI(root);

            SetActiveTab(UITabMode.Editor); // Default view
        }

        private void BindFileUI(VisualElement root)
        {
            // Bind config to scene loading
            var loadConfigButton = root.Q<Button>("btn-load-config");
            var exportConfigButton = root.Q<Button>("btn-export-config");
            _configWarningContainer = root.Q<VisualElement>("config-warning-container");
            _configWarningLabel = root.Q<Label>("label-config-warning");
            
            if (loadConfigButton != null) loadConfigButton.clicked += () => OnLoadConfigClickedEvent?.Invoke();
            if (exportConfigButton != null) exportConfigButton.clicked += () => OnExportConfigClickedEvent?.Invoke();


            // Bind environment loading
            var loadEnvButton = root.Q<Button>("btn-load-env");
            var exportEnvButton = root.Q<Button>("btn-export-env");
            
            if (loadEnvButton != null) loadEnvButton.clicked += () => OnLoadEnvironmentClickedEvent?.Invoke();
            if (exportEnvButton != null) exportEnvButton.clicked += () => OnExportEnvironmentClickedEvent?.Invoke();

            // Bind algorithm loading
            _btnLoadAlgorithm = root.Q<Button>("btn-load-algorithm");
            _algorithmDropdown = root.Q<DropdownField>("dropdown-algorithms");
            _playSimulationButton = root.Q<Button>("btn-play-sim");
            
            if (_algorithmDropdown != null) _algorithmDropdown.RegisterValueChangedCallback(evt => OnAlgorithmSelectedEvent?.Invoke(evt.newValue));
            if (_btnLoadAlgorithm != null) _btnLoadAlgorithm.clicked += () => OnLoadAlgorithmClickedEvent?.Invoke();
            if (_playSimulationButton != null) _playSimulationButton.clicked += () => OnPlaySimulationClickedEvent?.Invoke();

            // Bind config for simulation loading
            _btnStartConfig = root.Q<Button>("btn-start-config");
            _btnTargetConfig = root.Q<Button>("btn-target-config");
            _toggleCurrentScene = root.Q<Toggle>("toggle-current-scene");
            _iconStartConfig = root.Q<VisualElement>("icon-start-config");
            _iconTargetConfig = root.Q<VisualElement>("icon-target-config");

            if (_btnStartConfig != null) _btnStartConfig.clicked += () => OnSelectStartConfigClickedEvent?.Invoke();
            if (_btnTargetConfig != null) _btnTargetConfig.clicked += () => OnSelectTargetConfigClickedEvent?.Invoke();
            if (_toggleCurrentScene != null) _toggleCurrentScene.RegisterValueChangedCallback(evt => OnUseCurrentSceneToggledEvent?.Invoke(evt.newValue));

            // Bind environment settings
            _usePhysicsToggle = root.Q<Toggle>("toggle-use-physics");

            if (_usePhysicsToggle != null) _usePhysicsToggle.RegisterValueChangedCallback(evt => OnUsePhysicsToggledEvent?.Invoke(evt.newValue));
        }

        private void BindTabsUI(VisualElement root)
        {
            _tabBtnEditor = root.Q<Button>("tab-btn-editor");
            _tabBtnPlayback = root.Q<Button>("tab-btn-playback");
            _tabBtnFile = root.Q<Button>("tab-btn-file");

            _panelEditor = root.Q<VisualElement>("panel-editor");
            _panelPlayback = root.Q<VisualElement>("panel-playback");
            _panelFile = root.Q<VisualElement>("panel-file");

            if (_tabBtnEditor != null) _tabBtnEditor.clicked += () => SetActiveTab(UITabMode.Editor);
            if (_tabBtnPlayback != null) _tabBtnPlayback.clicked += () => SetActiveTab(UITabMode.Playback);
            if (_tabBtnFile != null) _tabBtnFile.clicked += () => SetActiveTab(UITabMode.File);
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
                    var obs = (IObstacleVolume)_obstacleListView.itemsSource[index]; //ASDFGH
                    label.text = $"Obstacle {obs.ID}";
                };

                _obstacleListView.selectionChanged += (selection) => OnObstacleListSelectionChangedEvent?.Invoke(selection);
            }

            _obstaclePositionInput?.RegisterValueChangedCallback(evt => OnObstaclePositionChangedEvent?.Invoke(evt.newValue));
            _obstacleSizeInput?.RegisterValueChangedCallback(evt => OnObstacleSizeChangedEvent?.Invoke(evt.newValue));

            if (_obstacleDetailsPanel != null) _obstacleDetailsPanel.style.display = DisplayStyle.None;
        }

        private void BindConfigEditorUI(VisualElement root)
        {
            var addDroneButton = root.Q<Button>("btn-add-drone");
            var addObstacleButton = root.Q<Button>("btn-add-obstacle");
            var removeSelectedButton = root.Q<Button>("btn-remove-selected");
            _gridSizeInput = root.Q<IntegerField>("input-grid-size");
            _droneWarningContainer = root.Q<VisualElement>("drone-warning-container");
            _droneWarningLabel = root.Q<Label>("label-drone-warning");

            if (addDroneButton != null) addDroneButton.clicked += () => OnAddDroneClickedEvent?.Invoke();
            if (addObstacleButton != null) addObstacleButton.clicked += () => OnAddObstacleClickedEvent?.Invoke();
            if (removeSelectedButton != null) removeSelectedButton.clicked += () => OnRemoveSelectedClickedEvent?.Invoke();
            if (_gridSizeInput != null) _gridSizeInput.RegisterValueChangedCallback(evt => OnGridSizeChangedEvent?.Invoke(evt.newValue));
        }

        private void BindToolbarUI(VisualElement root)
        {
            _floatingToolbar = root.Q<VisualElement>("floating-toolbar");
            _dragHandle = root.Q<VisualElement>("drag-handle");
            _btnSelect = root.Q<Button>("tool-btn-select");
            _btnMove = root.Q<Button>("tool-btn-move");
            _btnScale = root.Q<Button>("tool-btn-scale");

            // Bind toolbar dragging logic
            if (_dragHandle != null && _floatingToolbar != null)
            {
                _dragHandle.RegisterCallback<PointerDownEvent>(OnToolbarPointerDown);
                _dragHandle.RegisterCallback<PointerMoveEvent>(OnToolbarPointerMove);
                _dragHandle.RegisterCallback<PointerUpEvent>(OnToolbarPointerUp);
                _dragHandle.RegisterCallback<PointerCaptureOutEvent>(OnToolbarPointerUp);
            }

            // Bind tool buttons
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

        public void UpdateStartConfigVisuals(bool useCurrentScene, bool hasFile, string fileName = null)
        {
            if (_btnStartConfig != null) _btnStartConfig.SetEnabled(!useCurrentScene);

            if (useCurrentScene)
            {
                UpdateIconVisuals(_iconStartConfig, true, "Using current scene");
                _btnStartConfig.text = "S: Current Scene";
            }
            else
            {
                UpdateIconVisuals(_iconStartConfig, hasFile, hasFile ? fileName : "No File Selected");
                _btnStartConfig.text = hasFile ? "S: " + fileName : "Select Start Config";
            }
        }

        public void UpdateTargetConfigVisuals(bool hasFile, string fileName = null)
        {
            UpdateIconVisuals(_iconTargetConfig, hasFile, hasFile ? fileName : "No file selected");
            _btnTargetConfig.text = hasFile ? "T: " + fileName : "Select Target Config";
        }

        private void UpdateIconVisuals(VisualElement containerElement, bool isSuccess, string tooltipText)
        {
            if (containerElement == null) return;

            var label = containerElement.Q<Label>();
            var iconElement = containerElement.Q<VisualElement>(className: "scenario-icon-img");

            //containerElement.tooltip = tooltipText;
            //iconElement.tooltip = tooltipText;

            if (isSuccess)
            {
                iconElement.style.backgroundImage = new StyleBackground(_fileIcon);
                containerElement.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.2f)); // turn to a green file icon
                label.style.display = DisplayStyle.None;
            }
            else
            {
                iconElement.style.backgroundImage = default(StyleBackground);
                containerElement.style.backgroundColor = new StyleColor(new Color(0.6f, 0.3f, 0.3f)); // turn to a red X
                label.style.display = DisplayStyle.Flex;
                label.text = "X";
            }
        }

        public void PopulateAlgorithmDropdown(List<string> algorithmNames, string defaultSelection = null)
        {
            if (_algorithmDropdown == null) return;

            _algorithmDropdown.choices = algorithmNames;

            if (!string.IsNullOrEmpty(defaultSelection) && algorithmNames.Contains(defaultSelection))
                _algorithmDropdown.SetValueWithoutNotify(defaultSelection);
            else if (algorithmNames.Count > 0)
                _algorithmDropdown.SetValueWithoutNotify(algorithmNames[0]);
            else
                _algorithmDropdown.SetValueWithoutNotify("No algorithms found...");
        }

        public void SetPlaySimulationEnabled(bool isEnabled)
        {
            if (_playSimulationButton != null)
            {
                _playSimulationButton.SetEnabled(isEnabled);
            }
        }

        /// <summary>
        /// Changes the grid size UI value without triggering the change event
        /// </summary>
        public void SetGridSizeWithoutNotify(int size)
        {
            _gridSizeInput?.SetValueWithoutNotify(size);
        }

        public void SetActiveTab(UITabMode tab)
        {
            // Toggle panel visibility
            if (_panelEditor != null) _panelEditor.style.display = (tab == UITabMode.Editor) ? DisplayStyle.Flex : DisplayStyle.None;
            if (_panelPlayback != null) _panelPlayback.style.display = (tab == UITabMode.Playback) ? DisplayStyle.Flex : DisplayStyle.None;
            if (_panelFile != null) _panelFile.style.display = (tab == UITabMode.File) ? DisplayStyle.Flex : DisplayStyle.None;

            // Highlighting
            var activeColor = new StyleColor(new Color(0.27f, 0.27f, 0.27f, 1f));
            var defaultColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 0f));

            if (_tabBtnEditor != null) _tabBtnEditor.style.backgroundColor = (tab == UITabMode.Editor) ? activeColor : defaultColor;
            if (_tabBtnPlayback != null) _tabBtnPlayback.style.backgroundColor = (tab == UITabMode.Playback) ? activeColor : defaultColor;
            if (_tabBtnFile != null) _tabBtnFile.style.backgroundColor = (tab == UITabMode.File) ? activeColor : defaultColor;

            // Edge case for when switching panels when editing an input field
            if (tab != UITabMode.Playback && _timeScaleInput != null)
            {
                _timeScaleInput.style.display = DisplayStyle.None;
                if (_timeScaleLabel != null) _timeScaleLabel.style.display = DisplayStyle.Flex;
            }
        }

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

        /// <summary>
        /// Selects the given indices in the drones list without triggering the selection event
        /// </summary>
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
        /// Updates the inspector fields UI for drone position without triggering the change event
        /// </summary>
        public void UpdateDronePositionField(Vector3 newPos)
        {
            _dronePositionInput?.SetValueWithoutNotify(newPos);
        }

        public void PopulateObstacleList(List<IObstacleVolume> obstacles)
        {
            if (_obstacleListView == null) return;
            _obstacleListView.itemsSource = obstacles;
            _obstacleListView.Rebuild();
        }

        /// <summary>
        /// Selects the given indices in the obstacle list without triggering the selection event
        /// </summary>
        public void SetObstacleListSelectionWithoutNotify(List<int> indices)
        {
            _obstacleListView?.SetSelectionWithoutNotify(indices);
        }

        public void ClearObstacleListSelection()
        {
            _obstacleListView?.ClearSelection();
        }

        public void ShowObstacleDetails(IObstacleVolume obstacle, Vector3 unityPosition, Vector3 unitySize)
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

        /// <summary>
        /// Updates the "use physics" checkbox UI without triggering the change event
        /// </summary>
        public void SetUsePhysicsWithoutNotify(bool usePhysics)
        {
            _usePhysicsToggle?.SetValueWithoutNotify(usePhysics);
        }

        public void ShowConfigWarning(string reason)
        {
            if (_configWarningLabel != null) _configWarningLabel.text = $"These configurations are incompatible: {reason}";
            if (_configWarningContainer != null) _configWarningContainer.style.display = DisplayStyle.Flex;
        }

        public void HideConfigWarning()
        {
            if (_configWarningContainer != null) _configWarningContainer.style.display = DisplayStyle.None;
        }

        public void ShowDroneWarning(string message)
        {
            if (_droneWarningContainer != null)
            {
                _droneWarningContainer.style.display = DisplayStyle.Flex;
                _droneWarningLabel.text = message;
            }

        }

        public void HideDroneWarning()
        {
            if (_droneWarningContainer != null)
                _droneWarningContainer.style.display = DisplayStyle.None;
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