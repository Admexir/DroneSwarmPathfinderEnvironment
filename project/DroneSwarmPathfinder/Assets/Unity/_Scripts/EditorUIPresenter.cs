using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DroneSwarmPathfinder.Core.Models;
using DroneSwarmPathfinder.Unity.Managers;
using DroneSwarmPathfinder.Unity.EditorTools;
using DroneSwarmPathfinder.Unity.UI;

namespace DroneSwarmPathfinder.Unity.UI
{
    /// <summary>
    /// Links the UIController to the applications core logic and state
    /// (acts as the presenter in the MVP pattern)
    /// </summary>
    [RequireComponent(typeof(UIController))]
    public class EditorUIPresenter : MonoBehaviour
    {
        public static EditorUIPresenter instance;

        private UIController _view;
        private Drone _currentlySelectedDrone;

        private void Awake()
        {
            instance = this;
            _view = GetComponent<UIController>();
            SubscribeToViewEvents();
        }

        private void Start()
        {
            // Subscribe to the config manager's selection state changes
            if (ConfigEditorManager.instance != null)
            {
                ConfigEditorManager.instance.OnSelectionChanged += HandleSceneSelectionChanged;
                ConfigEditorManager.instance.OnGizmoDragged += HandleGizmoDragged;
            }

            // Subscribe to the playback manager's playback state changes
            if (SimulationPlaybackManager.instance != null)
                SimulationPlaybackManager.instance.OnPlaybackStateChanged += HandlePlaybackStateChanged;

            // Subscribe to the drone manager's drone list changes
            if (DroneManager.instance != null)
                DroneManager.instance.OnDroneRosterChanged += RefreshDroneList;

            RefreshDroneList();
        }

        private void SubscribeToViewEvents()
        {
            // Simulation controls
            _view.OnTimeScaleChangedEvent += OnTimeScaleChanged;
            _view.OnPlayClickedEvent += OnPlayClicked;
            _view.OnStepForwardClickedEvent += OnStepForwardClicked;
            _view.OnStepBackClickedEvent += OnStepBackClicked;
            _view.OnRestartClickedEvent += OnRestartClicked;

            // Editor actions
            _view.OnAddDroneClickedEvent += OnAddDroneClicked;
            _view.OnAddObstacleClickedEvent += OnAddObstacleClicked;
            _view.OnRemoveDroneClickedEvent += OnRemoveDroneClicked;

            // Config and simulation
            _view.OnLoadConfigClickedEvent += OnLoadConfigClicked;
            _view.OnPlaySimulationClickedEvent += OnPlaySimulationClicked;
            _view.OnExportConfigClickedEvent += OnExportConfigClicked;

            // Details panel editing
            _view.OnDroneGroupChangedEvent += OnDroneGroupChanged;
            _view.OnDronePositionChangedEvent += OnDronePositionChanged;
            _view.OnDroneListSelectionChangedEvent += OnDroneListSelectionChanged;
        }

        /// <summary>
        /// Updates the inspector fields for position (used by GizmoController)
        /// </summary>
        public void UpdatePositionFields(Vector3 newPos)
        {
            _view.UpdatePositionFields(newPos);
        }

        /// <summary>
        /// Helper method to refresh the drone list visually
        /// </summary>
        public void RefreshDroneList()
        {
            var drones = DroneManager.instance.AllDroneModels.ToList();
            _view.PopulateDroneList(drones);
        }

        #region View event callbacks

        private void OnTimeScaleChanged(int newValue)
        {
            Debug.Log($"Time Scale changed to {newValue}");
            SimulationPlaybackManager.instance.SetTimeScale(newValue / 100f);
            _view.UpdateTimeScaleDisplay(newValue);
        }

        private void OnPlayClicked()
        {
            if (SimulationPlaybackManager.instance.isPlaying)
            {
                Debug.Log("Play Button Clicked - Pause");
                SimulationPlaybackManager.instance.Pause();
            }
            else
            {
                Debug.Log("Play Button Clicked - Playing");
                SimulationPlaybackManager.instance.Play();
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

        private void OnDroneGroupChanged(int newGroup)
        {
            if (_currentlySelectedDrone == null) return;
            DroneManager.instance.UpdateDroneGroup(_currentlySelectedDrone.ID, newGroup);
            RefreshDroneList();
        }

        private void OnDronePositionChanged(Vector3 newPosition)
        {
            if (_currentlySelectedDrone == null) return;
            DroneManager.instance.UpdateDronePosition(_currentlySelectedDrone.ID, newPosition);
        }

        private void OnAddDroneClicked()
        {
            Debug.Log("Adding new drone...");
            DroneManager.instance.CreateNewDrone(Vector3.zero);
            RefreshDroneList();
        }

        private void OnAddObstacleClicked()
        {
            Debug.Log("Adding new obstacle...");
            ObstacleManager.instance.CreateNewObstacle(Vector3.zero);
            RefreshDroneList();
        }

        private void OnRemoveDroneClicked()
        {
            if (_currentlySelectedDrone == null) return;

            Debug.Log($"Removing drone {_currentlySelectedDrone.ID}");
            DroneManager.instance.RemoveDrone(_currentlySelectedDrone.ID);

            RefreshDroneList();
            _view.ClearListSelection();
        }

        private void OnLoadConfigClicked()
        {
            Debug.Log("Loading Configuration...");
            // ConfigSerializer.LoadFromFile("config.json");
            // RefreshDroneList();
        }

        private void OnPlaySimulationClicked()
        {
            Debug.Log("Starting Simulation Algorithm...");
            _view.SetPlaybackUIVisibility(true);

            // var config = DroneManager.instance.AllDroneModels.ToList();
            // var result = PathfindingEngine.Solve(config); 
            // SimulationPlaybackManager.instance.LoadSimulationResult(result); 
            // SimulationPlaybackManager.instance.Play(); 
            // _view.RefreshPlayButtonState(SimulationPlaybackManager.instance.isPlaying);
        }

        private void OnExportConfigClicked()
        {
            Debug.Log("Exporting Configuration...");
            // var config = DroneManager.instance.AllDroneModels.ToList();
            // ConfigSerializer.SaveToFile(config, "latest_config.json");
        }

        private void OnDroneListSelectionChanged(IEnumerable<object> selectedItems)
        {
            if (selectedItems == null || !selectedItems.Any())
            {
                _view.HideDroneDetails();
                _currentlySelectedDrone = null;
                ConfigEditorManager.instance.SetSelectionFromUI(new List<int>());
                return;
            }

            var selectedDrone = (Drone)selectedItems.FirstOrDefault();
            _currentlySelectedDrone = selectedDrone;

            Vector3 currentPos = selectedDrone.Transform.Position.ToUnity();
            _view.ShowDroneDetails(selectedDrone, currentPos);

            var selectedIds = selectedItems.Cast<Drone>().Select(d => d.ID).ToList();
            ConfigEditorManager.instance.SetSelectionFromUI(selectedIds);
        }

        #endregion

        #region External state callbacks

        /// <summary>
        /// Called when selecting drones by clicking on them/box selection in the 3D scene
        /// </summary>
        private void HandleSceneSelectionChanged(List<int> selectedDroneIds)
        {
            if (selectedDroneIds == null || selectedDroneIds.Count == 0)
            {
                _currentlySelectedDrone = null;
                _view.HideDroneDetails();
                _view.SetListSelectionWithoutNotify(new List<int>());
            }
            else
            {
                var drone = DroneManager.instance.GetDroneDataFromID(selectedDroneIds[0]);
                if (drone == null)
                {
                    Debug.LogError($"Tried selecting a drone with invalid ID {selectedDroneIds[0]}!");
                    return;
                }

                _currentlySelectedDrone = drone;
                _view.ShowDroneDetails(drone, drone.Transform.Position.ToUnity());

                // Select all selected drones in the UI list
                var indices = selectedDroneIds.Select(id => GetIndexOfDrone(id)).Where(index => index != -1).ToList();
                _view.SetListSelectionWithoutNotify(indices);
            }
        }

        private int GetIndexOfDrone(int id)
        {
            var models = DroneManager.instance.AllDroneModels.ToList();
            for (int i = 0; i < models.Count; i++)
            {
                if (models[i].ID == id) return i;
            }
            return -1;
        }

        private void HandlePlaybackStateChanged(bool isNowPlaying)
        {
            _view.RefreshPlayButtonState(isNowPlaying);
        }

        private void HandleGizmoDragged(Vector3 newPos)
        {
            // Update inspector UI values while dragging gizmos
            // (update the position field iff one drone is selected)
            if (_currentlySelectedDrone != null && ConfigEditorManager.instance.SelectedDrones.Count == 1)
            {
                _view.UpdatePositionFields(newPos);
            }
        }

        #endregion
    }
}