using DroneSwarmPathfinder.Core.Environment;
using DroneSwarmPathfinder.Core.Models;
using DroneSwarmPathfinder.Core.Simulation;
using DroneSwarmPathfinder.Unity.EditorTools;
using DroneSwarmPathfinder.Unity.Managers;
using DroneSwarmPathfinder.Unity.Services;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        private IObstacleVolume _currentlySelectedObstacle; //ASDFGH
        private IFileBrowserService _fileBrowser;

        private List<IPathfindingAlgorithm> _availableAlgorithms = new();
        private IPathfindingAlgorithm _selectedAlgorithm;

        private void Awake()
        {
            instance = this;
            _view = GetComponent<UIController>();
            _fileBrowser = new DesktopFileBrowserService();
            SubscribeToViewEvents();
        }

        private void Start()
        {
            // Subscribe to the config manager's selection state changes
            if (ConfigEditorManager.instance != null)
            {
                ConfigEditorManager.instance.OnDroneSelectionChanged += HandleDroneSceneSelectionChanged;
                ConfigEditorManager.instance.OnObstacleSelectionChanged += HandleObstacleSceneSelectionChanged;
                ConfigEditorManager.instance.OnGizmoDragged += HandleGizmoDragged;
            }

            // Subscribe to the playback manager's playback state changes
            if (SimulationPlaybackManager.instance != null)
                SimulationPlaybackManager.instance.OnPlaybackStateChanged += HandlePlaybackStateChanged;

            // Subscribe to the managers' roster list changes
            if (DroneManager.instance != null)
                DroneManager.instance.OnDroneRosterChanged += RefreshDroneList;
            if (EnvironmentManager.instance != null)
                EnvironmentManager.instance.OnObstacleRosterChanged += RefreshObstacleList;

            // Subscribe to the algorithm managers' roster list changes
            if (AlgorithmManager.instance != null)
            {
                AlgorithmManager.instance.OnAlgorithmsRefreshed += HandleAlgorithmsRefreshed;
                AlgorithmManager.instance.OnSelectionValidityChanged += HandleAlgorithmSelectionValidity;
                HandleAlgorithmsRefreshed();
            }

            if (ConfigSelectionManager.instance != null)
            {
                ConfigSelectionManager.instance.OnScenarioStateChanged += HandleScenarioStateChanged;
                HandleScenarioStateChanged(); // Initialize UI state on startup
            }

            _view.SetActiveToolVisual(EditorToolMode.Move); // set the default TODO: unhardcode

            RefreshDroneList();
            RefreshObstacleList();
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
            _view.OnRemoveSelectedClickedEvent += OnRemoveSelectedClicked;
            _view.OnGridSizeChangedEvent += OnGridSizeChanged;

            // Config and simulation
            _view.OnLoadConfigClickedEvent += OnLoadConfigClicked;
            _view.OnPlaySimulationClickedEvent += OnPlaySimulationClicked;
            _view.OnExportConfigClickedEvent += OnExportConfigClicked;
            _view.OnLoadAlgorithmClickedEvent += OnLoadAlgorithmClicked;
            _view.OnAlgorithmSelectedEvent += OnAlgorithmSelected;
            _view.OnLoadEnvironmentClickedEvent += OnLoadEnvironmentClicked;
            _view.OnExportEnvironmentClickedEvent += OnExportEnvironmentClicked;

            // Details panel editing (Drones)
            _view.OnDroneGroupChangedEvent += OnDroneGroupChanged;
            _view.OnDronePositionChangedEvent += OnDronePositionChanged;
            _view.OnDroneListSelectionChangedEvent += OnDroneListSelectionChanged;

            // Details panel editing (Obstacles)
            _view.OnObstaclePositionChangedEvent += OnObstaclePositionChanged;
            _view.OnObstacleSizeChangedEvent += OnObstacleSizeChanged;
            _view.OnObstacleListSelectionChangedEvent += OnObstacleListSelectionChanged;

            // Scenario Selection
            _view.OnSelectStartConfigClickedEvent += OnSelectStartConfigClicked;
            _view.OnSelectTargetConfigClickedEvent += OnSelectTargetConfigClicked;
            _view.OnUseCurrentSceneToggledEvent += OnUseCurrentSceneToggled;

            // Transform tools panel selection
            _view.OnToolClickedEvent += OnToolClicked;

            // Environment settings
            _view.OnUsePhysicsToggledEvent += OnUsePhysicsToggled;

        }

        /// <summary>
        /// Helper method to refresh the drone list visually
        /// </summary>
        public void RefreshDroneList()
        {
            var drones = DroneManager.instance.AllDroneModels.ToList();
            _view.PopulateDroneList(drones);
        }

        /// <summary>
        /// Helper method to refresh the obstacle list visually
        /// </summary>
        public void RefreshObstacleList()
        {
            var obstacles = EnvironmentManager.instance.AllObstacleModels.ToList();
            _view.PopulateObstacleList(obstacles);
        }
        

        /// <summary>
        /// Method to remove the currently selected drone (singular)
        /// </summary>
        private void OnRemoveSelectedClicked()
        {
            if (_currentlySelectedDrone != null)
            {
                Debug.Log($"Removing drone {_currentlySelectedDrone.ID}");
                DroneManager.instance.RemoveDrone(_currentlySelectedDrone.ID);
                _view.ClearDroneListSelection();
            }

            if (_currentlySelectedObstacle != null)
            {
                Debug.Log($"Removing obstacle {_currentlySelectedObstacle.ID}");
                EnvironmentManager.instance.RemoveObstacle(_currentlySelectedObstacle.ID);
                _view.ClearObstacleListSelection();
            }
        }

        #region View event callbacks (environment settings)
        /// <summary>
        /// Changes the environment grid size to the given value
        /// </summary>
        private void OnGridSizeChanged(int newSize)
        {
            EnvironmentManager.instance.ChangeGridSize(newSize);
        }
        private void OnUsePhysicsToggled(bool usePhysics)
        {
            Debug.Log($"Use physics toggle set to {usePhysics}. \nWarning: physics simulation is not yet implemented...");
        }
        #endregion

        #region View event callbacks (Simulation)

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

        private void OnPlaySimulationClicked()
        {
            var algorithm = AlgorithmManager.instance.SelectedAlgorithm;
            if (algorithm == null) return;

            Debug.Log($"Preparing to run: {algorithm.AlgorithmName}...");
            _view.SetActiveTab(UITabMode.Playback);

            if (Simulation.PathfindingRunner.instance != null)
            {
                _ = Simulation.PathfindingRunner.instance.RunAlgorithmAsync(algorithm);
            }
            else { Debug.LogError("PathfindingRunner instance is missing from the scene :)"); }
        }
        #endregion

        #region View event callbacks (file managment)

        private string GetConfigFilePath()
        {
            return System.IO.Path.Combine(Application.dataPath, "latest_config.json");
        }

        private void OnLoadConfigClicked()
        {
            // Pause simulation if running
            if (SimulationPlaybackManager.instance != null && SimulationPlaybackManager.instance.isPlaying)
                SimulationPlaybackManager.instance.Pause();

            string path = _fileBrowser.RequestLoadPath("Load Swarm Configuration", "json");
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("Swarm configuration loading canceled by user");
                return;
            }

            Debug.Log($"Loading swarm configuration from \"{path}\"...");
            string json = System.IO.File.ReadAllText(path);

            var config = Core.Serialization.JSONSerializer.Deserialize<Core.Serialization.DroneJSONConfig>(json);
            if (config != null)
            {
                DroneManager.instance.SpawnDrones(config.Drones);
                ConfigEditorManager.instance.SetDroneSelectionFromUI(new List<int>());
                Debug.Log("Swarm configuration loaded successfully");
            }
        }

        private void OnExportConfigClicked()
        {
            string path = _fileBrowser.RequestSavePath("Export Swarm Configuration", "swarm_config", "json");
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("Swarm configuration exporting canceled by user");
                return;
            }

            Debug.Log($"Exporting Swarm Configuration to \"{path}\"...");

            var config = new Core.Serialization.DroneJSONConfig
            {
                Drones = DroneManager.instance.AllDroneModels.ToList()
            };

            // Use the generic serializer
            string json = Core.Serialization.JSONSerializer.Serialize(config);
            System.IO.File.WriteAllText(path, json);
            Debug.Log("Swarm configuration exported successfully");
        }

        private void OnLoadEnvironmentClicked()
        {
            // Pause simulation if running
            if (SimulationPlaybackManager.instance != null && SimulationPlaybackManager.instance.isPlaying)
                SimulationPlaybackManager.instance.Pause();

            string path = _fileBrowser.RequestLoadPath("Load Environment Configuration", "json");
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("Environment loading canceled by user");
                return;
            }

            Debug.Log($"Loading environment from \"{path}\"...");
            string json = System.IO.File.ReadAllText(path);

            var config = Core.Serialization.JSONSerializer.Deserialize<Core.Serialization.EnvironmentJSONConfig>(json);
            if (config != null)
            {
                // Spawn the obstacles
                EnvironmentManager.instance.SpawnObstacles(config.Obstacles);

                // Load the spatial rules (Grid)
                if (config.SpatialRules is DiscreteGrid grid)
                {
                    EnvironmentManager.instance.ChangeGridSize(grid.CellSize);
                }

                // Clear UI selection state
                ConfigEditorManager.instance.SetObstacleSelectionFromUI(new List<int>());
                Debug.Log("Environment loaded successfully");
            }
        }

        private void OnExportEnvironmentClicked()
        {
            string path = _fileBrowser.RequestSavePath("Export Environment Configuration", "environment_config", "json");
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("Environment exporting canceled by user");
                return;
            }

            Debug.Log($"Exporting Environment to \"{path}\"...");

            // Construct the environment from the current scene state
            var config = new Core.Serialization.EnvironmentJSONConfig
            (
                EnvironmentManager.instance.GetCurrentEnvironment()
            );

            string json = Core.Serialization.JSONSerializer.Serialize(config);
            System.IO.File.WriteAllText(path, json);
            Debug.Log("Environment exported successfully");
        }
        #endregion

        #region View event callbacks (drones)

        private void OnAddDroneClicked()
        {
            Debug.Log("Adding new drone...");
            var newDrone = DroneManager.instance.CreateNewDrone(Vector3.zero);

            // auto-select the newly created drone
            var selectedIds = new List<int> { newDrone.ID };
            ConfigEditorManager.instance.SetDroneSelectionFromUI(selectedIds);
            HandleDroneSceneSelectionChanged(selectedIds);
        }

        private void OnDroneGroupChanged(int newGroup)
        {
            if (_currentlySelectedDrone == null) return;
            DroneManager.instance.UpdateDroneGroup(_currentlySelectedDrone.ID, newGroup);
        }

        private void OnDronePositionChanged(Vector3 newPosition)
        {
            if (_currentlySelectedDrone == null) return;
            DroneManager.instance.UpdateDronePosition(_currentlySelectedDrone.ID, newPosition);
        }

        private void OnDroneListSelectionChanged(IEnumerable<object> selectedItems)
        {
            if (selectedItems == null || !selectedItems.Any())
            {
                _view.HideDroneDetails();
                _currentlySelectedDrone = null;
                ConfigEditorManager.instance.SetDroneSelectionFromUI(new List<int>());
                return;
            }

            var selectedDrone = (Drone)selectedItems.FirstOrDefault();
            _currentlySelectedDrone = selectedDrone;

            Vector3 currentPos = selectedDrone.Transform.Position.ToUnity();
            _view.ShowDroneDetails(selectedDrone, currentPos);

            var selectedIds = selectedItems.Cast<Drone>().Select(d => d.ID).ToList();
            ConfigEditorManager.instance.SetDroneSelectionFromUI(selectedIds);
        }

        #endregion

        #region View event callbacks (obstacles)

        private void OnAddObstacleClicked()
        {
            Debug.Log("Adding new obstacle...");
            var newObs = EnvironmentManager.instance.CreateNewObstacle(Vector3.zero);

            // auto-select the newly created obstacle
            var selectedIds = new List<int> { newObs.ID };
            ConfigEditorManager.instance.SetObstacleSelectionFromUI(selectedIds);
            HandleObstacleSceneSelectionChanged(selectedIds);
        }

        private void OnObstaclePositionChanged(Vector3 newPosition)
        {
            if (_currentlySelectedObstacle == null) return;
            EnvironmentManager.instance.UpdateObstaclePosition(_currentlySelectedObstacle.ID, newPosition);
        }

        private void OnObstacleSizeChanged(Vector3 newSize)
        {
            if (_currentlySelectedObstacle == null) return;
            EnvironmentManager.instance.UpdateObstacleSize(_currentlySelectedObstacle.ID, newSize);
        }

        private void OnObstacleListSelectionChanged(IEnumerable<object> selectedItems)
        {
            if (selectedItems == null || !selectedItems.Any())
            {
                _view.HideObstacleDetails();
                _currentlySelectedObstacle = null;
                ConfigEditorManager.instance.SetObstacleSelectionFromUI(new List<int>());
                return;
            }

            var selectedObstacle = (IObstacleVolume)selectedItems.FirstOrDefault(); //ASDFGH
            _currentlySelectedObstacle = selectedObstacle;

            Vector3 currentPos = selectedObstacle.Transform.Position.ToUnity();
            Vector3 currentSize = selectedObstacle.Transform.Size.ToUnity();
            _view.ShowObstacleDetails(selectedObstacle, currentPos, currentSize);

            var selectedIds = selectedItems.Cast<BoxObstacle>().Select(o => o.ID).ToList();
            ConfigEditorManager.instance.SetObstacleSelectionFromUI(selectedIds);
        }

        #endregion

        #region View event callbacks (toolbar)

        private void OnToolClicked(EditorToolMode mode)
        {
            if (ConfigEditorManager.instance != null)
            {
                ConfigEditorManager.instance.SetEditorToolMode(mode); // Internal logic
            }

            _view.SetActiveToolVisual(mode); // Change highlight

            Debug.Log($"Switched active editor tool to: {mode}");
        }

        #endregion

        #region View event callbacks (algorithms)
        private void OnLoadAlgorithmClicked()
        {
            string path = _fileBrowser.RequestLoadPath("Load External Pathfinding Algorithm", "dll");

            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("Algorithm loading canceled by user");
                return;
            }

            Debug.Log($"Loading external algorithm from \"{path}\"...");

            if (AlgorithmManager.instance.LoadExternalAlgorithm(path))
            {
                Debug.Log($"Successfully loaded DLL from \"{path}\"");
            }
            else
            {
                Debug.LogError($"Failed to load the selected DLL from \"{path}\"");
            }
        }

        private void OnAlgorithmSelected(string algorithmName)
        {
            AlgorithmManager.instance.SelectAlgorithmByName(algorithmName);
        }
        #endregion

        #region View event callbacks (config selection)
        private void OnSelectStartConfigClicked()
        {
            string path = _fileBrowser.RequestLoadPath("Select Starting Configuration", "json");
            if (!string.IsNullOrEmpty(path))
            {
                ConfigSelectionManager.instance.SetStartConfigPath(path);
            }
        }

        private void OnSelectTargetConfigClicked()
        {
            string path = _fileBrowser.RequestLoadPath("Select Target Configuration", "json");
            if (!string.IsNullOrEmpty(path))
            {
                ConfigSelectionManager.instance.SetTargetConfigPath(path);
            }
        }

        private void OnUseCurrentSceneToggled(bool useCurrent)
        {
            ConfigSelectionManager.instance.SetUseCurrentScene(useCurrent);
        }

        private void HandleScenarioStateChanged()
        {
            var scenario = ConfigSelectionManager.instance;

            bool hasStartFile = !string.IsNullOrEmpty(scenario.StartConfigPath);
            string startFileName = hasStartFile ? System.IO.Path.GetFileName(scenario.StartConfigPath) : null;
            _view.UpdateStartConfigVisuals(scenario.UseCurrentSceneForStart, hasStartFile, startFileName);

            bool hasTargetFile = !string.IsNullOrEmpty(scenario.TargetConfigPath);
            string targetFileName = hasTargetFile ? System.IO.Path.GetFileName(scenario.TargetConfigPath) : null;
            _view.UpdateTargetConfigVisuals(hasTargetFile, targetFileName);
        }
        #endregion

        #region External state callbacks (events called without interacting with the UI)

        /// <summary>
        /// Called when selecting drones by clicking on them/box selection in the 3D scene
        /// </summary>
        private void HandleDroneSceneSelectionChanged(List<int> selectedDroneIds)
        {
            if (selectedDroneIds == null || selectedDroneIds.Count == 0)
            {
                _currentlySelectedDrone = null;
                _view.HideDroneDetails();
                _view.SetDroneListSelectionWithoutNotify(new List<int>());
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
                var models = DroneManager.instance.AllDroneModels.ToList();
                var indices = selectedDroneIds.Select(id => models.FindIndex(m => m.ID == id)).Where(index => index != -1).ToList();
                _view.SetDroneListSelectionWithoutNotify(indices);
            }
        }

        /// <summary>
        /// Called when selecting obstacles by clicking on them/box selection in the 3D scene
        /// </summary>
        private void HandleObstacleSceneSelectionChanged(List<int> selectedObstacleIds)
        {
            if (selectedObstacleIds == null || selectedObstacleIds.Count == 0)
            {
                _currentlySelectedObstacle = null;
                _view.HideObstacleDetails();
                _view.SetObstacleListSelectionWithoutNotify(new List<int>());
            }
            else
            {
                var obstacle = EnvironmentManager.instance.GetObstacleDataFromID(selectedObstacleIds[0]);
                if (obstacle == null)
                {
                    Debug.LogError($"Tried selecting an obstacle with invalid ID {selectedObstacleIds[0]}!");
                    return;
                }

                _currentlySelectedObstacle = obstacle;
                _view.ShowObstacleDetails(obstacle, obstacle.Transform.Position.ToUnity(), obstacle.Transform.Size.ToUnity());

                // Select all selected obstacles in the UI list
                var models = EnvironmentManager.instance.AllObstacleModels.ToList();
                var indices = selectedObstacleIds.Select(id => models.FindIndex(m => m.ID == id)).Where(index => index != -1).ToList();
                _view.SetObstacleListSelectionWithoutNotify(indices);
            }
        }

        /// <summary>
        /// Called when the the simulation playback starts or stops
        /// </summary>
        /// <param name="isNowPlaying"></param>
        private void HandlePlaybackStateChanged(bool isNowPlaying)
        {
            _view.RefreshPlayButtonState(isNowPlaying);
        }

        /// <summary>
        /// Called when the user drags a gizmo
        /// </summary>
        private void HandleGizmoDragged(Vector3 newPos)
        {
            // Update inspector UI values while dragging gizmos
            // (update the position field iff exactly one item of a type is selected)
            if (_currentlySelectedDrone != null && ConfigEditorManager.instance.SelectedDrones.Count == 1)
            {
                _view.UpdateDronePositionField(newPos);
            }

            if (_currentlySelectedObstacle != null && ConfigEditorManager.instance.SelectedObstacles.Count == 1)
            {
                _view.UpdateObstaclePositionField(newPos);
            }
        }

        /// <summary>
        /// Called when the collection of loaded algorithms is changed
        /// </summary>
        private void HandleAlgorithmsRefreshed()
        {
            var algorithms = AlgorithmManager.instance.AvailableAlgorithms;
            var names = algorithms.Select(a => a.AlgorithmName).ToList();

            string defaultSelection = AlgorithmManager.instance.SelectedAlgorithm?.AlgorithmName;
            _view.PopulateAlgorithmDropdown(names, defaultSelection);
        }

        /// <summary>
        /// Called when a new pathfinding algorithm is selected
        /// </summary>
        private void HandleAlgorithmSelectionValidity(bool isValid)
        {
            _view.SetPlaySimulationEnabled(isValid);
        }

        #endregion
    }
}