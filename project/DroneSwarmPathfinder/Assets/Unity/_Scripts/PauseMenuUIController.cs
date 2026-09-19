using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace DroneSwarmPathfinder.Unity.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class PauseMenuUIController : MonoBehaviour
    {
        private UIDocument _uiDocument;
        private VisualElement _pauseOverlay;

        // Pause menu
        private Button _btnResume;
        private Button _btnSettings;
        private Button _btnQuit;

        // Pause menu events for other scripts to subscribe to
        public event Action OnResumeClickedEvent;
        public event Action OnSettingsClickedEvent;
        public event Action OnQuitClickedEvent;

        private void OnEnable()
        {
            _uiDocument = GetComponent<UIDocument>();
            var root = _uiDocument.rootVisualElement;

            BindPauseMenuUI(root);
            SetPauseMenuVisibility(false);
        }

        private void BindPauseMenuUI(VisualElement root)
        {
            _pauseOverlay = root.Q<VisualElement>("pause-background");

            _btnResume = root.Q<Button>("btn-resume");
            _btnSettings = root.Q<Button>("btn-settings");
            _btnQuit = root.Q<Button>("btn-quit");

            Debug.Log($"{_pauseOverlay == null}, {_btnQuit == null}, {_btnResume == null}, {_btnSettings == null}");

            if (_btnResume != null) _btnResume.clicked += () => OnResumeClickedEvent?.Invoke();
            if (_btnSettings != null) _btnSettings.clicked += () => OnSettingsClickedEvent?.Invoke();
            if (_btnQuit != null) _btnQuit.clicked += () => OnQuitClickedEvent?.Invoke();

        }

        /// <summary>
        /// Shows or hides the main pause menu
        /// </summary>
        public void SetPauseMenuVisibility(bool isVisible)
        {
            if (_pauseOverlay != null)
            {
                _pauseOverlay.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}