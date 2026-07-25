using DroneSwampPathfiner.Core.Models;
using DroneSwampPathfiner.Unity.Managers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UIController : MonoBehaviour
{
    public static UIController instance; private void Awake() => instance = this;
    private UIDocument _uiDocument;
    private Button _playButton;
    private Label _timeScaleLabel;
    private SliderInt _timeScaleSlider;
    private IntegerField _timeScaleInput;
    private Label _idInput;
    private IntegerField _groupInput;

    // --- NEW: Variables for visibility toggling ---
    private Button _stepForwardButton;
    private Button _stepBackButton;
    private Button _restartButton;
    private VisualElement _droneDetailsPanel;

    public Drone CurrentlySelectedDrone { get; private set; }

    private void OnEnable()
    {
        _uiDocument = GetComponent<UIDocument>();
        var root = _uiDocument.rootVisualElement;

        // Simulation controls UI
        _playButton = root.Q<Button>("btn-play");
        _timeScaleLabel = root.Q<Label>("time-scale-label");
        _timeScaleSlider = root.Q<SliderInt>("time-scale-slider");
        _stepForwardButton = root.Q<Button>("btn-step-forward");
        _stepBackButton = root.Q<Button>("btn-step-back");
        _restartButton = root.Q<Button>("btn-restart");

        if (_timeScaleSlider != null)
        {
            _timeScaleSlider.RegisterValueChangedCallback(OnTimeScaleChanged);
            if (_timeScaleLabel != null)
                _timeScaleLabel.text = $"Time Scale: {_timeScaleSlider.value}%";
        }

        if (_timeScaleLabel != null && this._timeScaleSlider != null)
        {
            _timeScaleInput = new IntegerField();
            _timeScaleInput.style.display = DisplayStyle.None;
            _timeScaleInput.style.marginBottom = 2;

            _timeScaleLabel.parent.Insert(_timeScaleLabel.parent.IndexOf(_timeScaleLabel), _timeScaleInput);

            _timeScaleLabel.RegisterCallback<PointerDownEvent>(OnLabelClicked);
            _timeScaleInput.RegisterCallback<KeyDownEvent>(OnInputKeyDown);
            _timeScaleInput.RegisterCallback<FocusOutEvent>(OnInputFocusOut);
        }

        if (_playButton != null) _playButton.clicked += OnPlayClicked;
        if (_stepForwardButton != null) _stepForwardButton.clicked += OnStepForwardClicked;
        if (_stepBackButton != null) _stepBackButton.clicked += OnStepBackClicked;
        if (_restartButton != null) _restartButton.clicked += OnRestartClicked;

        // Config editor UI
        var droneListView = root.Q<ListView>("drone-list-view");
        _idInput = root.Q<Label>("input-drone-id");
        _groupInput = root.Q<IntegerField>("input-drone-group");
        _droneDetailsPanel = root.Q<VisualElement>("drone-details-panel");

        droneListView.itemsSource = DroneManager.instance.ActiveDrones.ToList();
        droneListView.makeItem = () => new Label();
        droneListView.bindItem = (element, index) =>
        {
            var label = element as Label;
            var drone = DroneManager.instance.ActiveDrones.ElementAt(index);
            label.text = $"Drone {drone.DroneID} (group: {drone.DroneGroup})";
        };

        droneListView.selectionChanged += SelectDrone;

        _groupInput.RegisterValueChangedCallback(evt =>
        {
            CurrentlySelectedDrone.GroupId = evt.newValue;
            droneListView.RefreshItems();
        });

        var addDroneButton = root.Q<Button>("btn-add-drone");
        var removeDroneButton = root.Q<Button>("btn-remove-drone");
        var loadConfigButton = root.Q<Button>("btn-load-config");
        var playSimulationButton = root.Q<Button>("btn-play-sim");
        var exportConfigButton = root.Q<Button>("btn-export-config");

        if (addDroneButton != null) addDroneButton.clicked += OnAddDroneClicked;
        if (removeDroneButton != null) removeDroneButton.clicked += OnRemoveDroneClicked;
        if (loadConfigButton != null) loadConfigButton.clicked += OnLoadConfigClicked;
        if (playSimulationButton != null) playSimulationButton.clicked += OnPlaySimulationClicked;
        if (exportConfigButton != null) exportConfigButton.clicked += OnExportConfigClicked;

        if (_droneDetailsPanel != null) _droneDetailsPanel.style.display = DisplayStyle.None;
        SetPlaybackUIVisibility(false);
    }

    #region Event Handlers

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

    #region Simulation Control Handlers
    private void OnTimeScaleChanged(ChangeEvent<int> evt)
    {
        Debug.Log($"Time Scale changed from {evt.previousValue} to {evt.newValue}");
        SimulationPlaybackManager.instance.SetTimeScale(evt.newValue / 100f);
        if (_timeScaleLabel != null)
        {
            _timeScaleLabel.text = $"Time Scale: {evt.newValue}%";
        }
    }

    public void RefreshPlayButtonState() => _playButton.text = SimulationPlaybackManager.instance.isPlaying ? "Pause" : "Play";
    private void OnPlayClicked()
    {
        if (SimulationPlaybackManager.instance.isPlaying)
        {
            Debug.Log("Play Button Clicked - Pause");
            SimulationPlaybackManager.instance.Pause();
            _playButton.text = "Play";
        }
        else
        {
            Debug.Log("Play Button Clicked - Playing");
            SimulationPlaybackManager.instance.Play();
            _playButton.text = "Pause";
        }
    }

    private void OnStepForwardClicked()
    {
        Debug.Log("Step Forward Clicked");
        SimulationPlaybackManager.instance.StepForward();
    }

    private void OnStepBackClicked()
    {
        Debug.Log("Step Back Clicked");
        SimulationPlaybackManager.instance.StepBackward();
    }

    private void OnRestartClicked()
    {
        Debug.Log("Restart Clicked");
        SimulationPlaybackManager.instance.Restart();
    }

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
        _timeScaleSlider.value = clampedValue;
        _timeScaleInput.style.display = DisplayStyle.None;
        _timeScaleLabel.style.display = DisplayStyle.Flex;
    }
    #endregion

    #region Config editor Handlers
    private void SelectDrone(IEnumerable<object> selectedItems)
    {
        var selectedObject = selectedItems.FirstOrDefault();

        // If clicked empty space to deselect, hide panel
        if (selectedObject == null)
        {
            CurrentlySelectedDrone = null;
            if (_droneDetailsPanel != null) _droneDetailsPanel.style.display = DisplayStyle.None;
            return;
        }

        if (selectedObject is not Drone)
        {
            Debug.LogError($"Error selecting drone in hierarchy window: {(selectedObject == null ? "Selected drone is null" : selectedObject.ToString() + " is not of type Drone")}");
            return;
        }

        Drone selectedDrone = (Drone)selectedObject;

        _idInput.text = selectedDrone.ID.ToString();
        _groupInput.value = selectedDrone.GroupId;
        CurrentlySelectedDrone = selectedDrone;

        // Show panel when a drone is selected
        if (_droneDetailsPanel != null) _droneDetailsPanel.style.display = DisplayStyle.Flex;
    }

    private void OnAddDroneClicked()
    {
        Debug.Log("Add Drone Clicked");
    }

    private void OnRemoveDroneClicked()
    {
        Debug.Log("Remove Drone Clicked");
    }

    private void OnLoadConfigClicked()
    {
        Debug.Log("Load Configuration Clicked");
    }

    private void OnPlaySimulationClicked()
    {
        Debug.Log("Play Simulation Clicked");

        // Show playback controls when starting simulation
        SetPlaybackUIVisibility(true);
    }

    private void OnExportConfigClicked()
    {
        Debug.Log("Export Configuration Clicked");
    }
    #endregion

    #endregion
}