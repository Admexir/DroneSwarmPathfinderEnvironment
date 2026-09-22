using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static UnityEditor.ShaderData;
using static UnityEngine.Application;
using static UnityEngine.UIElements.UxmlAttributeDescription;

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

        // Settings Elements
        private VisualElement _settingsOverlay;
        private ListView _groupColorsList;
        private Button _btnCloseSettings;

        // Pause menu events for other scripts to subscribe to
        public event Action OnResumeClickedEvent;
        public event Action OnSettingsClickedEvent;
        public event Action OnQuitClickedEvent;

        // Settings menu events for other scripts to subscribe to
        public event Action OnCloseSettingsClickedEvent;
        public event Action<string, Color> OnGroupColorChangedEvent;

        private void OnEnable()
        {
            _uiDocument = GetComponent<UIDocument>();
            var root = _uiDocument.rootVisualElement;

            BindPauseMenuUI(root);
            BindGroupColorsList();

            SetPauseMenuVisibility(false);
            SetSettingsMenuVisibility(false, false);
        }

        private void BindPauseMenuUI(VisualElement root)
        {
            _pauseOverlay = root.Q<VisualElement>("pause-background");

            _btnResume = root.Q<Button>("btn-resume");
            _btnSettings = root.Q<Button>("btn-settings");
            _btnQuit = root.Q<Button>("btn-quit");

            _settingsOverlay = root.Q<VisualElement>("settings-overlay");
            _btnCloseSettings = root.Q<Button>("btn-close-settings");
            _groupColorsList = root.Q<ListView>("list-group-colors");

            //Debug.Log($"{_pauseOverlay == null}, {_btnQuit == null}, {_btnResume == null}, {_btnSettings == null}");

            if (_btnResume != null) _btnResume.clicked += () => OnResumeClickedEvent?.Invoke();
            if (_btnSettings != null) _btnSettings.clicked += () => OnSettingsClickedEvent?.Invoke();
            if (_btnQuit != null) _btnQuit.clicked += () => OnQuitClickedEvent?.Invoke();
            if (_btnCloseSettings != null) _btnCloseSettings.clicked += () => OnCloseSettingsClickedEvent?.Invoke();

        }

        private void BindGroupColorsList()
        {
            if (_groupColorsList == null) return;

            // Create a row in the colors list
            _groupColorsList.makeItem = () =>
            {
                var container = new VisualElement();
                container.style.flexDirection = FlexDirection.Row;
                container.style.alignItems = Align.Center;
                container.style.marginBottom = 2;
                container.style.marginTop = 2;

                var label = new Label();
                label.name = "group-name";
                label.style.width = 150;
                label.style.color = Color.white;
                label.style.unityTextAlign = TextAnchor.MiddleLeft;

                var input = new TextField();
                input.name = "group-color-input";
                input.style.flexGrow = 1;

                container.Add(label);
                container.Add(input);
                return container;
            };

            // Bind the list to update with groups data
            _groupColorsList.bindItem = (VisualElement element, int index) =>
            {
                var groupName = (string)_groupColorsList.itemsSource[index];
                var label = element.Q<Label>("group-name");
                var input = element.Q<TextField>("group-color-input");

                label.text = groupName;

                // Load initial color
                if (Managers.DroneManager.instance.GetColorByGroup(groupName, out Color c))
                {
                    input.SetValueWithoutNotify("<" + ColorUtility.ToHtmlStringRGB(c));
                    input.style.color = new StyleColor(c);
                }

                // Clear old callbacks
                if (input.userData is EventCallback<ChangeEvent<string>> oldCallback)
                {
                    input.UnregisterCallback(oldCallback);
                }

                // Update text color to set color callback
                EventCallback<ChangeEvent<string>> callback = (ChangeEvent<string> evt) =>
                {
                    if (ColorUtility.TryParseHtmlString(evt.newValue, out Color parsedColor))
                    {
                        input.style.color = new StyleColor(parsedColor); // Update hex text color visually
                        OnGroupColorChangedEvent?.Invoke(groupName, parsedColor);
                    }
                    else
                    {
                        input.style.color = new StyleColor(Color.red); // Invalid format warning
                    }
                };

                input.userData = callback;
                input.RegisterCallback(callback);
            };
        }

        /// <summary>
        /// Shows or hides the main pause menu
        /// </summary>
        public void SetPauseMenuVisibility(bool isVisible)
        {
            _pauseOverlay.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;

        }

        public void SetSettingsMenuVisibility(bool isVisible, bool showPause = true)
        {
            if (_settingsOverlay != null) _settingsOverlay.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
            // Hide main pause menu
            if (_pauseOverlay != null && isVisible) _pauseOverlay.style.display = DisplayStyle.None;
            if (_pauseOverlay != null && !isVisible && _pauseOverlay.style.display == DisplayStyle.None && showPause) _pauseOverlay.style.display = DisplayStyle.Flex; // Restore pause menu
        }

        public void PopulateGroupColorsList(List<string> groups)
        {
            if (_groupColorsList != null)
            {
                _groupColorsList.itemsSource = groups;
                _groupColorsList.Rebuild();
            }
        }

    }
}