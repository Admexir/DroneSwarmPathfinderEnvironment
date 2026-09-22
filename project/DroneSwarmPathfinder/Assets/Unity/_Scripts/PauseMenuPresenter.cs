using System.Collections.Generic;
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

        private void Start()
        {
            // Autoupdate the settings group list when groups are changed
            var uiController = FindAnyObjectByType<UIController>();
            if (uiController != null)
            {
                uiController.OnCreateNewGroupEvent += (_) => RefreshSettingsList();
                uiController.OnDroneGroupChangedEvent += (_) => RefreshSettingsList();
            }
        }

        private void SubscribeToViewEvents()
        {
            _view.OnResumeClickedEvent += ResumeApplication;
            _view.OnSettingsClickedEvent += OpenSettings;
            _view.OnQuitClickedEvent += QuitApplication;

            // Settings menu hooks
            _view.OnCloseSettingsClickedEvent += CloseSettings;
            _view.OnGroupColorChangedEvent += HandleGroupColorChanged;
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
            _view.SetSettingsMenuVisibility(false, false);
        }

        private void OpenSettings()
        {
            RefreshSettingsList();
            _view.SetSettingsMenuVisibility(true);
        }
        private void CloseSettings()
        {
            _view.SetSettingsMenuVisibility(false);
        }
        private void RefreshSettingsList()
        {
            if (Managers.DroneManager.instance != null)
            {
                _view.PopulateGroupColorsList(new List<string>(Managers.DroneManager.instance.DroneGroups));
            }
        }

        private void HandleGroupColorChanged(string groupName, Color newColor)
        {
            Managers.DroneManager.instance.UpdateDroneGroupColor(groupName, newColor);
        }

        private void QuitApplication()
        {
            Debug.Log("Quitting application...");
            Application.Quit();
        }
    }
}