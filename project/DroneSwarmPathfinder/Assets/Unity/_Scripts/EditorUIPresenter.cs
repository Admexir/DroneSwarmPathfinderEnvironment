using DroneSwarmPathfinder.Core.Environment;
using DroneSwarmPathfinder.Core.Models;
using DroneSwarmPathfinder.Core.Serialization;
using DroneSwarmPathfinder.Core.Simulation;
using DroneSwarmPathfinder.Unity.EditorTools;
using DroneSwarmPathfinder.Unity.Managers;
using DroneSwarmPathfinder.Unity.Services;
using DroneSwarmPathfinder.Unity.Visuals;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        private IDroneConfigItem _currentlySelectedDrone;
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
            // Subscribe to the result manager's selection state changes
            if (ConfigEditorManager.instance != null)
            {
                ConfigEditorManager.instance.OnDroneSelectionChanged += HandleDroneSceneSelectionChanged;
                ConfigEditorManager.instance.OnObstacleSelectionChanged += HandleObstacleSceneSelectionChanged;
                ConfigEditorManager.instance.OnGizmoDragged += HandleGizmoDragged;
                ConfigEditorManager.instance.OnGizmoDragged += OnDroneObstaclePositionChange;

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
            _view.OnAddDroneClickedEvent += OnDroneObstaclePositionChange;
            _view.OnAddDroneTargetPositionClickedEvent += OnAddDronePositionClicked;
            _view.OnAddDroneTargetPositionClickedEvent += OnDroneObstaclePositionChange;
            _view.OnAddObstacleClickedEvent += OnAddObstacleClicked;
            _view.OnAddObstacleClickedEvent += OnDroneObstaclePositionChange;
            _view.OnRemoveSelectedClickedEvent += OnRemoveSelectedClicked;
            _view.OnRemoveSelectedClickedEvent += OnDroneObstaclePositionChange;
            _view.OnGridSizeChangedEvent += OnGridSizeChanged;

            // Config and simulation
            _view.OnLoadConfigClickedEvent += OnLoadConfigClicked;
            _view.OnLoadConfigClickedEvent += OnDroneObstaclePositionChange;
            _view.OnRunAlgorithmClickedEvent += OnRunAlgorithmClicked;
            _view.OnExportConfigClickedEvent += OnExportConfigClicked;
            _view.OnLoadAlgorithmClickedEvent += OnLoadAlgorithmClicked;
            _view.OnAlgorithmSelectedEvent += OnAlgorithmSelected;
            _view.OnLoadEnvironmentClickedEvent += OnLoadEnvironmentClicked;
            _view.OnLoadEnvironmentClickedEvent += OnDroneObstaclePositionChange;
            _view.OnExportEnvironmentClickedEvent += OnExportEnvironmentClicked;

            // Details panel editing (Drones)
            _view.OnDroneGroupChangedEvent += OnDroneGroupChanged;
            _view.OnDronePositionChangedEvent += OnDronePositionChanged;
            _view.OnDronePositionChangedEvent += OnDroneObstaclePositionChange;
            _view.OnDroneListSelectionChangedEvent += OnDroneListSelectionChanged;
            _view.OnCreateNewGroupEvent += OnCreateNewGroup;
            _view.OnToggleRemoveGroupModeEvent += OnToggleRemoveGroupMode;
            _view.OnDroneNameChangedEvent += OnDroneNameChanged;
            _view.OnDroneDescriptionChangedEvent += OnDroneDescriptionChanged;
            _view.OnDroneColorChangedEvent += OnDroneColorChanged;

            // Details panel editing (Obstacles)
            _view.OnObstaclePositionChangedEvent += OnObstaclePositionChanged;
            _view.OnObstaclePositionChangedEvent += OnDroneObstaclePositionChange;
            _view.OnObstacleSizeChangedEvent += OnObstacleSizeChanged;
            _view.OnObstacleSizeChangedEvent += OnDroneObstaclePositionChange;
            _view.OnObstacleListSelectionChangedEvent += OnObstacleListSelectionChanged;

            // Scenario Selection
            _view.OnSelectStartConfigClickedEvent += OnSelectStartConfigClicked;
            _view.OnSelectTargetConfigClickedEvent += OnSelectTargetConfigClicked;
            _view.OnUseCurrentSceneToggledEvent += OnUseCurrentSceneToggled;
            _view.OnSelectEnvironmentConfigClickedEvent += OnSelectEnvironmentConfigClicked;

            // Results loading
            _view.OnLoadResultClickedEvent += OnLoadResultClicked;
            _view.OnExportResultClickedEvent += OnExportResultClicked;
            _view.OnResultSelectedEvent += OnResultSelected;
            _view.OnClearResultClickedEvent += OnClearResultClicked;

            // Transform tools panel selection
            _view.OnToolClickedEvent += OnToolClicked;

            // Environment settings
            _view.OnUsePhysicsToggledEvent += OnUsePhysicsToggled;

        }

        #region Helpers
        /// <summary>
        /// Helper method to refresh the drone list visually
        /// </summary>
        public void RefreshDroneList()
        {
            var drones = DroneManager.instance.AllDroneItems.ToList();
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
        #endregion

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

        /// <summary>
        /// Puts the given result to the simulation results cache so user can easily access it later without recalculating it
        /// </summary>
        private void CacheResultForDropdown(SimulationResult result, string originalFileName)
        {
            // Make a name for the dropdown based on the file name
            string uniqueName = originalFileName;
            int counter = 1;

            // Prevent key collisions
            while (_loadedResultsCache.ContainsKey(uniqueName)) uniqueName = $"{originalFileName} ({counter++})";

            // cache it
            _loadedResultsCache[uniqueName] = result;

            // Repopulate the UI
            _view.PopulateRecentResultsDropdown(_loadedResultsCache.Keys.ToList(), uniqueName);

            //Debug.Log($"Result '{uniqueName}' added to the results cache");
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

            var config = Core.Serialization.JSONSerializer.Deserialize<Core.Serialization.DroneConfigJson>(json);
            if (config != null)
            {
                ConfigSelectionManager.instance.LoadDroneConfigJson(config);
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

            //var config = new Core.Serialization.DroneConfigJson
            //{
            //    Drones = DroneManager.instance.AllDroneModelsOnly.ToList(),
            //    TargetPositions = DroneManager.instance.AllDroneTargetPositionsOnly.ToList(),
            //    GroupColors = DroneManager.instance._droneGroups.ToDictionary(k => k.Key, v => v.Value.ToSystemDrawing()) // Map the unity colors back to Core format
            //};
            var config = ConfigSelectionManager.CreateDroneConfigJson(
                drones: DroneManager.instance.AllDroneModelsOnly,
                targetPositions: DroneManager.instance.AllDroneTargetPositionsOnly,
                groupColors: DroneManager.instance._droneGroups
            );

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

            var config = Core.Serialization.JSONSerializer.Deserialize<Core.Serialization.EnvironmentConfigJson>(json);
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
            var config = new Core.Serialization.EnvironmentConfigJson
            (
                EnvironmentManager.instance.CurrentWorldEnvironment
            );

            string json = Core.Serialization.JSONSerializer.Serialize(config);
            System.IO.File.WriteAllText(path, json);
            Debug.Log("Environment exported successfully");
        }

        private async void OnRunAlgorithmClicked()
        {
            var algorithm = AlgorithmManager.instance.SelectedAlgorithm;
            if (algorithm == null) return;

            Debug.Log($"Preparing to run: {algorithm.AlgorithmName}...");
            _view.SetCalculationProgressVisibility(true);
            _view.UpdateCalculationProgress(0f, "Initializing...");

            if (Simulation.PathfindingRunner.instance != null)
            {
                try
                {
                    var progressTracker = new System.Progress<float>(p => { _view.UpdateCalculationProgress(p * 100f); });
                    SimulationResult result = await Simulation.PathfindingRunner.instance.RunAlgorithmAsync(algorithm, progressTracker);
                    if (result != null && result.IsSuccessful)
                    {
                        CacheResultForDropdown(result, $"{System.DateTime.Now:HH:mm} Result: {algorithm.AlgorithmName}");
                    }
                }
                finally
                {
                    _view.SetCalculationProgressVisibility(false);
                }
            }
            else { Debug.LogError("PathfindingRunner instance is missing from the scene :)"); _view.SetCalculationProgressVisibility(false); }
        }

        private void OnLoadResultClicked()
        {
            // Pause simulation if running
            if (SimulationPlaybackManager.instance != null && SimulationPlaybackManager.instance.isPlaying)
                SimulationPlaybackManager.instance.Pause();

            string path = _fileBrowser.RequestLoadPath("Load Results Configuration", "json");
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("Results loading canceled by user");
                return;
            }

            Debug.Log($"Loading results from \"{path}\"...");
            string json = System.IO.File.ReadAllText(path);

            var result = Core.Serialization.JSONSerializer.Deserialize<Core.Serialization.ResultsJson>(json);
            if (result != null && result.FullResultObject != null)
            {
                CacheResultForDropdown(result.FullResultObject, System.IO.Path.GetFileNameWithoutExtension(path));

                SimulationPlaybackManager.instance.LoadSimulationResult(result.FullResultObject);
                Debug.Log("Results loaded successfully");
            }

        }

        private void OnExportResultClicked()
        {
            string path = _fileBrowser.RequestSavePath("Export Pathfinding Result Paths", "paths", "json");
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("Results exporting canceled by user");
                return;
            }

            Debug.Log($"Exporting results to \"{path}\"...");

            // Construct the environment from the current scene state

            var result = new Core.Serialization.ResultsJson
            (
                SimulationPlaybackManager.instance.LatestResult
            );

            string json = Core.Serialization.JSONSerializer.Serialize(result);
            System.IO.File.WriteAllText(path, json);
            Debug.Log("Results exported successfully");
        }

        private void OnClearResultClicked()
        {
            SimulationPlaybackManager.instance.ClearSimulation();
        }

        private Dictionary<string, SimulationResult> _loadedResultsCache = new Dictionary<string, SimulationResult>();
        /// <summary>
        /// Called when the user selects a result from the recent results dropdown
        /// </summary>
        private void OnResultSelected(string resultName)
        {
            if (string.IsNullOrEmpty(resultName)) return;

            if (_loadedResultsCache.TryGetValue(resultName, out SimulationResult selectedResult))
            {
                Debug.Log($"Swapping to simulation result: {resultName}");

                // Pause simulation if running
                if (SimulationPlaybackManager.instance != null && SimulationPlaybackManager.instance.isPlaying)
                    SimulationPlaybackManager.instance.Pause();

                // Load the new paths into the visualizer/playback manager
                SimulationPlaybackManager.instance.LoadSimulationResult(selectedResult);

                //_view.SetActiveTab(UITabMode.Playback); // (switch to the playback tab)
            }
            else
            {
                Debug.LogWarning($"Could not find result '{resultName}' in the loaded cache..."); // Shouldn't happen
            }
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

        private void OnAddDronePositionClicked()
        {
            Debug.Log("Adding new drone target position...");
            var newDronePos = DroneManager.instance.CreateNewDronePosition(Vector3.zero);

            // auto-select the newly created drone
            var selectedIds = new List<int> { newDronePos.ID };
            ConfigEditorManager.instance.SetDroneSelectionFromUI(selectedIds);
            HandleDroneSceneSelectionChanged(selectedIds);
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

            var selectedDrone = (IDroneConfigItem)selectedItems.FirstOrDefault();
            _currentlySelectedDrone = selectedDrone;

            Vector3 currentPos = selectedDrone.Transform.Position.ToUnity();
            _view.ShowDroneDetails(selectedDrone, currentPos);

            var selectedIds = selectedItems.Cast<IDroneConfigItem>().Select(d => d.ID).ToList();
            ConfigEditorManager.instance.SetDroneSelectionFromUI(selectedIds);
        }

        /// <summary>
        /// Event called when a drone or obstacle moves used for detecting whether a drone overlaps with an obstacle to show a warning
        /// </summary>
        private void OnDroneObstaclePositionChange()
        {
            if(!DroneManager.instance.CheckDronePositionValidity(out var invalidPositionDrones))
            {
                _view.ShowDroneWarning($"These drones are in invalid positions: {string.Join(", ", invalidPositionDrones.Select(x => $"Drone {x.ID}"))}"); //TODO: pass names after names are implemented instead of hardcoded ID-based name
            }
            else
            {
                _view.HideDroneWarning();
            }
        }
        private void OnDroneObstaclePositionChange(Vector3 _) => OnDroneObstaclePositionChange();

        private bool groupRemoveModeActive = false;
        private void OnDroneGroupChanged(string selectedGroup)
        {
            if (_currentlySelectedDrone == null) return;

            // Remove mode active
            if (groupRemoveModeActive)
            {
                var currentGroup = _currentlySelectedDrone.GroupName;
                if (currentGroup == selectedGroup)
                {
                    currentGroup = "default";
                    DroneManager.instance.UpdateDroneGroup(_currentlySelectedDrone.ID, currentGroup);
                }

                DroneManager.instance.RemoveDroneGroup(selectedGroup);
                _view.PopulateDroneGroupDropdown(DroneManager.instance.DroneGroups, currentGroup);
            }
            else // Normal assignment
            {
                DroneManager.instance.UpdateDroneGroup(_currentlySelectedDrone.ID, selectedGroup);
                if (DroneManager.instance.GetColorByGroup(selectedGroup, out Color c)) // update color text
                    _view.SetInspectorColorTextWithoutNotify("#" + ColorUtility.ToHtmlStringRGB(c));
            }
        }

        private void OnCreateNewGroup(string newGroupName)
        {
            DroneManager.instance.CreateNewDroneGroup(newGroupName, new Color(Random.value, Random.value, Random.value)); // TODO: unrandomize color
            _view.PopulateDroneGroupDropdown(DroneManager.instance.DroneGroups, _currentlySelectedDrone.GroupName);
            //DroneManager.instance.UpdateDroneGroup(_currentlySelectedDrone.ID, newGroupName); // update the selected drones group too
        }

        private void OnToggleRemoveGroupMode(bool isRemoveMode)
        {
            groupRemoveModeActive = isRemoveMode;
        }

        private void OnDroneNameChanged(string newName)
        {
            if (_currentlySelectedDrone == null) return;
            DroneManager.instance.UpdateDroneName(_currentlySelectedDrone.ID, newName);
            RefreshDroneList();
        }

        private void OnDroneDescriptionChanged(string newDescription)
        {
            if (_currentlySelectedDrone == null) return;
            DroneManager.instance.UpdateDroneDescription(_currentlySelectedDrone.ID, newDescription);
        }

        private void OnDroneColorChanged(Color newColor)
        {
            if (_currentlySelectedDrone == null) return;
            DroneManager.instance.UpdateDroneColor(_currentlySelectedDrone.ID, newColor.ToSystemDrawing());
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

            bool hasEnvFile = !string.IsNullOrEmpty(scenario.EnvironmentConfigPath);
            string envFileName = hasEnvFile ? System.IO.Path.GetFileName(scenario.EnvironmentConfigPath) : null;
            _view.UpdateEnvironmentConfigVisuals(scenario.UseCurrentSceneForStart, hasEnvFile, envFileName);

            string compatibilityError;
            bool isCompatible = scenario.CheckConfigCompatibility(out compatibilityError);
            if (isCompatible) _view.HideConfigWarning();
            else _view.ShowConfigWarning(compatibilityError);

        }
        private void OnSelectEnvironmentConfigClicked()
        {
            string path = _fileBrowser.RequestLoadPath("Select Environment Configuration", "json");
            if (!string.IsNullOrEmpty(path))
            {
                ConfigSelectionManager.instance.SetEnvironmentConfigPath(path);
            }
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

                // Add this calculation to figure out the effective display color
                Color displayColor = drone.Color.A == 0
                    ? (DroneManager.instance.GetColorByGroup(drone.GroupName, out var groupCol) ? groupCol : UnityEngine.Color.white)
                    : drone.Color.ToUnity();

                //drone.Color = displayColor.ToSystemDrawing();
                _view.ShowDroneDetails(drone, drone.Transform.Position.ToUnity());

                // Select all selected drones in the UI list
                var models = DroneManager.instance.AllDroneItems.ToList();
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