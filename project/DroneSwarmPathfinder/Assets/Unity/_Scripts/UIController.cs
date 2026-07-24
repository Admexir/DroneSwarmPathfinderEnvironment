using DroneSwampPathfiner.Unity.Managers;
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

    private void OnEnable()
    {
        _uiDocument = GetComponent<UIDocument>();
        var root = _uiDocument.rootVisualElement;

        _playButton = root.Q<Button>("btn-play");
        _timeScaleLabel = root.Q<Label>("time-scale-label");
        _timeScaleSlider = root.Q<SliderInt>("time-scale-slider");
        var stepForwardButton = root.Q<Button>("btn-step-forward");
        var stepBackButton = root.Q<Button>("btn-step-back");

        if (_timeScaleSlider != null)
        {
            _timeScaleSlider.RegisterValueChangedCallback(OnTimeScaleChanged);
            if (_timeScaleLabel != null)
                _timeScaleLabel.text = $"Time Scale: {_timeScaleSlider.value}%";
        }

        if (_timeScaleLabel != null && this._timeScaleSlider != null)
        {
            // Insert time input field for exact time scale input
            _timeScaleInput = new IntegerField();
            _timeScaleInput.style.display = DisplayStyle.None;
            _timeScaleInput.style.marginBottom = 2;

            _timeScaleLabel.parent.Insert(_timeScaleLabel.parent.IndexOf(_timeScaleLabel), _timeScaleInput);

            _timeScaleLabel.RegisterCallback<PointerDownEvent>(OnLabelClicked);
            _timeScaleInput.RegisterCallback<KeyDownEvent>(OnInputKeyDown);
            _timeScaleInput.RegisterCallback<FocusOutEvent>(OnInputFocusOut);
        }

        if (_playButton != null) _playButton.clicked += OnPlayClicked;
        if (stepForwardButton != null) stepForwardButton.clicked += OnStepForwardClicked;
        if (stepBackButton != null) stepBackButton.clicked += OnStepBackClicked;
    }

    //// According to AI, it's good practice to unregister callbacks when the object is disabled
    //// I'm new to the UI Toolkit system, so I'll trust it for now... :)
    //private void OnDisable()
    //{
    //    if (_uiDocument == null) return;

    //    var root = _uiDocument.rootVisualElement;

    //    if (_timeScaleSlider != null)
    //    {
    //        _timeScaleSlider.UnregisterValueChangedCallback(OnTimeScaleChanged);
    //    }

    //    if (_timeScaleLabel != null)
    //    {
    //        _timeScaleLabel.UnregisterCallback<PointerDownEvent>(OnLabelClicked);
    //    }

    //    if (_timeScaleInput != null)
    //    {
    //        _timeScaleInput.UnregisterCallback<KeyDownEvent>(OnInputKeyDown);
    //        _timeScaleInput.UnregisterCallback<FocusOutEvent>(OnInputFocusOut);

    //        if (_timeScaleInput.parent != null)
    //            _timeScaleInput.parent.Remove(_timeScaleInput);
    //    }
    //}

    #region Event Handlers

    private void OnTimeScaleChanged(ChangeEvent<int> evt)
    {
        Debug.Log($"Time Scale changed from {evt.previousValue} to {evt.newValue}");
        SimulationPlaybackManager.instance.SetTimeScale(evt.newValue/100f);
        if (_timeScaleLabel != null)
        {
            _timeScaleLabel.text = $"Time Scale: {evt.newValue}%";
        }
    }

    private void OnPlayClicked()
    {
        if (SimulationPlaybackManager.instance.isPlaying)
        {
            Debug.Log("Play Button Clicked - Stopping");
            SimulationPlaybackManager.instance.Stop();
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

    // Double click on time scale number detection ----------------------
    private void OnLabelClicked(PointerDownEvent evt)
    {
        if (evt.clickCount == 2)
        {
            _timeScaleLabel.style.display = DisplayStyle.None;
            _timeScaleInput.style.display = DisplayStyle.Flex;
            _timeScaleInput.value = _timeScaleSlider.value;
            // delay refocus so that it runs after a renderer run (otherwise input field isn't rendered - can't be focused)
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
        //var previousValue = _timeScaleSlider.value;
        _timeScaleSlider.value = clampedValue;
        _timeScaleInput.style.display = DisplayStyle.None;
        _timeScaleLabel.style.display = DisplayStyle.Flex;
        //OnTimeScaleChanged(new ChangeEvent<int>() { newValue = clampedValue, previousValue = previousValue });
    }
    // --------------------------------------------------------------

    #endregion
}