using DroneSwampPathfiner.Unity.Managers;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UIController : MonoBehaviour
{
    public static UIController instance; private void Awake() => instance = this;
    private UIDocument _uiDocument;
    private Button _playButton;

    private void OnEnable()
    {
        _uiDocument = GetComponent<UIDocument>();
        var root = _uiDocument.rootVisualElement;

        _playButton = root.Q<Button>("btn-play");
        var timeScaleSlider = root.Q<SliderInt>("time-scale-slider");
        var stepForwardButton = root.Q<Button>("btn-step-forward");
        var stepBackButton = root.Q<Button>("btn-step-back");

        if (timeScaleSlider != null)
        {
            timeScaleSlider.RegisterValueChangedCallback(OnTimeScaleChanged);
        }

        if (_playButton != null) _playButton.clicked += OnPlayClicked;
        if (stepForwardButton != null) stepForwardButton.clicked += OnStepForwardClicked;
        if (stepBackButton != null) stepBackButton.clicked += OnStepBackClicked;
    }

    private void OnDisable()
    {
        // According to AI, it's good practice to unregister callbacks when the object is disabled
        // I'm new to the UI Toolkit system, so I'll trust it for now... :)
        var root = _uiDocument.rootVisualElement;

        var timeScaleSlider = root.Q<SliderInt>("time-scale-slider");
        if (timeScaleSlider != null)
        {
            timeScaleSlider.UnregisterValueChangedCallback(OnTimeScaleChanged);
        }
    }

    #region Event Handlers

    private void OnTimeScaleChanged(ChangeEvent<int> evt)
    {
        Debug.Log($"Time Scale changed from {evt.previousValue} to {evt.newValue}");
        SimulationPlaybackManager.instance.SetTimeScale(evt.newValue/100f);
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

    #endregion
}