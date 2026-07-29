using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using DroneSwarmPathfinder.Core.Models;

namespace DroneSwarmPathfinder.Unity.UI
{
    /// <summary>
    /// Interface for any class that can determine if mouse is currently over it (TODO: currently only for UIController to improve architecture, maybe remove?)
    /// </summary>
    public interface IPointerStateProvider
    {
        bool IsPointerOverUI();
    }
    [RequireComponent(typeof(UIDocument))]
    public class UIController : MonoBehaviour, IPointerStateProvider
    {
        private UIDocument _uiDocument;

        // UI Elements
        private Button _playButton;
        private Label _timeScaleLabel;
        private SliderInt _timeScaleSlider;
        private IntegerField _timeScaleInput;
        private TextField _idInput;
        private ListView _droneListView;
        private IntegerField _groupInput;
        private Vector3Field _positionInput;
        private Button _stepForwardButton;
        private Button _stepBackButton;
        private Button _restartButton;
        private VisualElement _droneDetailsPanel;

        // Events for other scripts to subscribe to
        public event Action<int> OnTimeScaleChangedEvent;
        public event Action OnPlayClickedEvent;
        public event Action OnStepForwardClickedEvent;
        public event Action OnStepBackClickedEvent;
        public event Action OnRestartClickedEvent;

        public event Action<int> OnDroneGroupChangedEvent;
        public event Action<Vector3> OnDronePositionChangedEvent;

        public event Action OnAddDroneClickedEvent;
        public event Action OnAddObstacleClickedEvent;
        public event Action OnRemoveDroneClickedEvent;
        public event Action OnLoadConfigClickedEvent;
        public event Action OnPlaySimulationClickedEvent;
        public event Action OnExportConfigClickedEvent;

        public event Action<IEnumerable<object>> OnDroneListSelectionChangedEvent;

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

            // Config editor UI
            _droneListView = root.Q<ListView>("drone-list-view");
            _idInput = root.Q<TextField>("input-drone-id");
            _groupInput = root.Q<IntegerField>("input-drone-group");
            _positionInput = root.Q<Vector3Field>("input-drone-position");
            _droneDetailsPanel = root.Q<VisualElement>("drone-details-panel");

            _droneListView.makeItem = () => new Label();
            _droneListView.bindItem = (element, index) =>
            {
                var label = element as Label;
                var drone = (Drone)_droneListView.itemsSource[index];
                label.text = $"Drone {drone.ID} (group: {drone.GroupId})";
            };

            _droneListView.selectionChanged += (selection) => OnDroneListSelectionChangedEvent?.Invoke(selection);

            _groupInput.RegisterValueChangedCallback(evt => OnDroneGroupChangedEvent?.Invoke(evt.newValue));
            _positionInput.RegisterValueChangedCallback(evt => OnDronePositionChangedEvent?.Invoke(evt.newValue));

            var addDroneButton = root.Q<Button>("btn-add-drone");
            var addObstacleButton = root.Q<Button>("btn-add-obstacle");
            var removeDroneButton = root.Q<Button>("btn-remove-drone");
            var loadConfigButton = root.Q<Button>("btn-load-config");
            var playSimulationButton = root.Q<Button>("btn-play-sim");
            var exportConfigButton = root.Q<Button>("btn-export-config");

            if (addDroneButton != null) addDroneButton.clicked += () => OnAddDroneClickedEvent?.Invoke();
            if (addObstacleButton != null) addObstacleButton.clicked += () => OnAddObstacleClickedEvent?.Invoke();
            if (removeDroneButton != null) removeDroneButton.clicked += () => OnRemoveDroneClickedEvent?.Invoke();
            if (loadConfigButton != null) loadConfigButton.clicked += () => OnLoadConfigClickedEvent?.Invoke();
            if (playSimulationButton != null) playSimulationButton.clicked += () => OnPlaySimulationClickedEvent?.Invoke();
            if (exportConfigButton != null) exportConfigButton.clicked += () => OnExportConfigClickedEvent?.Invoke();

            if (_droneDetailsPanel != null) _droneDetailsPanel.style.display = DisplayStyle.None;
            SetPlaybackUIVisibility(false);
        }

        #region Public View API (For Presenter)

        public void PopulateDroneList(List<Drone> drones)
        {
            _droneListView.itemsSource = drones;
            _droneListView.Rebuild();
        }

        public void SetListSelectionWithoutNotify(List<int> indices)
        {
            _droneListView.SetSelectionWithoutNotify(indices);
        }

        public void ClearListSelection()
        {
            _droneListView.ClearSelection();
        }

        public void ShowDroneDetails(Drone drone, Vector3 unityPosition)
        {
            _idInput.value = drone.ID.ToString();
            _groupInput.SetValueWithoutNotify(drone.GroupId);
            _positionInput.SetValueWithoutNotify(unityPosition);
            _droneDetailsPanel.style.display = DisplayStyle.Flex;
        }

        public void HideDroneDetails()
        {
            _droneDetailsPanel.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Updates the inspector fields for position without triggering the change event
        /// </summary>
        public void UpdatePositionFields(Vector3 newPos)
        {
            if (_positionInput != null) _positionInput.SetValueWithoutNotify(newPos);
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