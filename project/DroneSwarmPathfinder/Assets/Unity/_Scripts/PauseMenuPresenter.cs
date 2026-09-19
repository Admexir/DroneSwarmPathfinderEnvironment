using UnityEngine;

namespace DroneSwarmPathfinder.Unity.UI
{
    /// <summary>
    /// Class responsible for executing logic from the pause menu UI - Presenter from the MVP pattern
    /// </summary>
    [RequireComponent(typeof(PauseMenuUIController))]
    public class PauseMenuPresenter : MonoBehaviour
    {
        private PauseMenuUIController _view;
        private bool _isPaused = false;

        private void Awake()
        {
            _view = GetComponent<PauseMenuUIController>();
            SubscribeToViewEvents();
        }

        private void SubscribeToViewEvents()
        {
            _view.OnResumeClickedEvent += ResumeApplication;
            _view.OnSettingsClickedEvent += OpenSettings;
            _view.OnQuitClickedEvent += QuitApplication;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_isPaused) ResumeApplication();
                else PauseApplication();
            }
        }

        private void PauseApplication()
        {
            _isPaused = true;
            _view.SetPauseMenuVisibility(true);
            
            // Pause simulation playback
            if (Managers.SimulationPlaybackManager.instance != null && Managers.SimulationPlaybackManager.instance.isPlaying)
            {
                Managers.SimulationPlaybackManager.instance.Pause();
            }
        }

        private void ResumeApplication()
        {
            _isPaused = false;
            _view.SetPauseMenuVisibility(false);
        }

        private void OpenSettings()
        {
            throw new System.NotImplementedException(); // ...button is disabled in the UI
        }

        private void QuitApplication()
        {
            Debug.Log("Quitting application...");
            Application.Quit();
        }
    }
}