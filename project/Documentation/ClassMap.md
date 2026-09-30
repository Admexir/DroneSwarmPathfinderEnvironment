# Map of namespaces and their classes and interfaces  
  
## DroneSwarmPathfinder.Core
`Core` assembly  
This namespace contains pure C# objects and interfaces, completely independent of the Unity Engine

* **Namespace: `DroneSwarmPathfinder.Core.Models`**
    * `DataStructures.cs`: Defines the fundamental data structures like `TransformData`, `Waypoint`, `DronePath` classes/structs and core models like `Drone` and `DroneTargetPosition`

* **Namespace: `DroneSwarmPathfinder.Core.Environment`**
    * `Environment.cs`: Defines the `WorldEnvironment`, `DiscreteGrid` and `BoxObstacle` used for describing the simulation environment, also defines these interfaces: `ISpatialVolume`, `IObstacleVolume`, and `ISpatialEnvironment`

* **Namespace: `DroneSwarmPathfinder.Core.Simulation`**
    * `IPathfinder.cs`: Defines the pathfinding API through the `IPathfindingAlgorithm` interface and the `SimulationContext` and `SimulationResult` records
    * `AlgorithmRegistry.cs`: Implements the `DiscoverAlgorithms` and `LoadExternalAlgorithmDll` functions used to discover and load external algorithms at runtime using Reflection

* **Namespace: `DroneSwarmPathfinder.Core.Serialization`**
    * `IDroneConfigItem.cs`: Defines the JSON serialization interfaces - `IConfigItem`, `IDroneConfigItem` and `IEnvironmentConfigItem` interfaces
    * `JSONSerializer.cs`: Implements the `JSONSerializer` utility and the *Data Transfer Objects* for save states: `DroneConfigJson`, `EnvironmentConfigJson`, `ResultsJson`



## DroneSwarmPathfinder.Algorithms
`Algorithms` assembly  
This namespace contains the built-in concrete implementations of the custom pathfinding plugins

* **Namespace: `DroneSwarmPathfinder.Algorithms`**
    * `TrivialTestAlgorithm.cs`: Implements `TrivialPathfinder` - a basic demo implementation of `IPathfindingAlgorithm` that moves drones in straight lines



## DroneSwarmPathfinder.Unity
`Assembly-CSharp` assembly  
This namespace contains the integration of the pure C# Core logic with the Unity Engine's rendering, inputs, and UI

* **Namespace: `DroneSwarmPathfinder.Unity.Managers`**
    * `AlgorithmManager.cs`: Holds the state of available and selected pathfinding algorithms
    * `ConfigSelectionManager.cs`: Holds file paths and verifies compatibility for the start, target, and environment configurations
    * `DroneManager.cs`: Maps Core `Drone` and `DroneTargetPosition` models to their respective Unity visual GameObjects
    * `EnvironmentManager.cs`: Maps Core spatial rules and `BoxObstacle` models to the Unity 3D scene
    * `SimulationPlaybackManager.cs`: Responsible for the playback and interpolation of a calculated `SimulationResult`

* **Namespace: `DroneSwarmPathfinder.Unity.UI`**
    * `EditorUIPresenter.cs`: The MVP presenter responsible for connecting the main UI View (`UIController.cs`), Editor tools, and Core models
    * `UIController.cs`: The MVP view responsible for managing the UI Toolkit elements and calling their respective delegates, defines `EditorToolMode` and `UITabMode`, and implements `IPointerStateProvider`
    * `PauseMenuPresenter.cs`: The MVP presenter for the pause menu overlay
    * `PauseMenuUIController.cs`: The MVP view for the pause menu overlay

* **Namespace: `DroneSwarmPathfinder.Unity.EditorTools`**
    * `ConfigEditorManager.cs`: A facade responsible for interaction with selected objects
    * `EditorInputManager.cs`: Class responsible for handling mouse input inside the editor scene (selection and gizmo managment), passes info to Selection Manager
    * `SelectionManager.cs`: Manages active selection lists for drones and obstacles
    * `GizmoManager.cs`: Manages editor gizmo dragging math and movement/scale manipulators for selected objects
    * `BoxSelectionVisualizer.cs`: Responsible for visualizing drag-box-selection boxes with it's associated 2D math and `OnGUI` rendering

* **Namespace: `DroneSwarmPathfinder.Unity.Visuals`**
    * `ISelectableView.cs`: Interface `ISelectableView` implemented by any visual component that can be interacted with through the Editor Tools
    * `DroneView.cs`: Visual representation of a drone, is a component on every drone GameObject, implements the `ISelectableView` interface
    * `ObstacleView.cs`: Visual representation of an obstacle, is a component on every obstacle GameObject, implements the `ISelectableView` interface
    * `GridVisualizer.cs`: Uses `GL` graphics library to render a dynamic transparent grid around selected drones
    * `PathVisualizer.cs`: Uses Unity `LineRenderer` components to draw calculated drone paths

* **Namespace: `DroneSwarmPathfinder.Unity.Simulation`**
    * `PathfindingRunner.cs`: Responsible for asynchronous execution of pathfinding algorithms and aggregating all environment and swarm data into `SimulationContext`

* **Namespace: `DroneSwarmPathfinder.Unity.Services`**
    * `IFileBrowserService.cs`: Interface defining the contract for system OS file browser dialogs
    * `DesktopFileBrowserService.cs`: Platform-specific implementation using Unity Editor tools or Windows APIs

* **Namespace: `DroneSwarmPathfinder.Unity.Controls`**
    * `CameraMovement.cs`: Responsible for managing the free-fly camera movement and rotation

* **Namespace: `DroneSwarmPathfinder.Unity.Testing`**
    * `TestRunner.cs`: Temporary MonoBehaviour (`SimulationTester`) for bootstrapping initial test states for the purposes of debugging, not used in the final build

* **Namespace: `DroneSwarmPathfinder.Unity`**
    * `CoreUnityConversionExtensions.cs`: Extension methods to convert between `UnityEngine.Vector3`/`UnityEngine.Quaternion` and other UnityEngine data structures and `System.Numerics.Vector3`/`System.Numerics.Quaternion` and other System data structures