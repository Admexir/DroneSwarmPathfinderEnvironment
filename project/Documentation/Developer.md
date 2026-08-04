# Developer Documentation

This document explains the project's architectural structure and assembly layout.

## Architectural Overview

This is strictly divided between pure C# application logic and the Unity Engine logic, where the pure C# part is compiled into the Core.dll used for writing custom pathfinding scripts (view next section).  
The Unity Engine part handles all of the user interaction with the editor, simulation and serialization/deserialization of JSON files.  
The code is written with **SOLID** principles in mind.

### Assembly Definition Separation
To enforce the strict division of Unity/Core parts, the codebase is split using Unity Assembly Definitions (`.asmdef`):
* **`Core`:** Contains pure C# models (`Drone`, `BoxObstacle`), mathematical structures (`TransformData`), spatial rules, and the `IPathfindingAlgorithm` interface. **This assembly has zero references to `UnityEngine`.**
* **`Algorithms`:** Contains implementations of the pathfinding algorithms (ex. `TrivialPathfinder`). References `Core`.
* **`Unity` (Main Project):** Contains the Unity-specific visualizers, UI Toolkit implementations, and manager components. References `Core`.

### Model-View-Presenter UI Pattern
The user interface is built using Unity's UI Toolkit (UXML) and is governed by a MVP pattern:
* **The View (`UIController.cs,`):** Responsible *only* for binding to UXML elements, handling visuals, and invoking events (`OnToolClickedEvent`, `OnPlayClickedEvent`). It contains no business logic.
* **The Presenter (`EditorUIPresenter.cs`):** Acts as the connecting piece between the Model and View. It listens to View events, delegates work to other managers and updates the View.
* **The Models (`DroneManager`, `ObstacleManager`, `AlgorithmManager`):** Hold the single source of truth for the applications state. They report state changes by invoking events (ex. `OnObstacleRosterChanged`) for the Presenter (which updates the View).

### Abstraction of User Editor Tools
User editor interactions (selection, gizmo dragging) use the `ISelectableView` interface. This allows other scripts (`EditorInputManager.cs`, `GizmoManager.cs`) to manipulate Drones, Obstacles, or any future entities without requiring code modifications in their respective classes.

### Namespace Layout
The project's architecture is divided into three primary namespace domains to separate the three parts of the project - Core, Algorithms and Unity.
* **`DroneSwarmPathfinder.Core`** namespaces (which include `.Models`, `.Environment`, `.Simulation`, and `.Serialization`) contain pure C# logic. They define the fundamental data structures (like `Drone` and `TransformData`), spatial grid rules, JSON serialization protocols, and the `IPathfindingAlgorithm` API.
* **`DroneSwarmPathfinder.Algorithms`** namespace is dedicated to the concrete implementations of these custom pathfinding plugins, such as the `TrivialPathfinder`, allowing them to execute entirely independently of the visual engine.
* **`DroneSwarmPathfinder.Unity`** namespaces act as the integration layer with the Unity Engine. This domain is further subdivided into `.Managers` (for managing application state, rosters, and simulation playback), `.UI` (housing the Model-View-Presenter logic described above for the UI), `.EditorTools` (handling mouse input, raycasting, and gizmo manipulation), and `.Visuals` / `.Environment` (managing the physical 3D GameObjects and rendering of the drones, grids, and obstacles).

---

## The Pathfinding API

Algorithms in this project run asynchronously on a background thread. This allows complex pathfinding algorithms to calculate without causing the Unity main thread (the user editor) to freeze.

### Using External Algorithms
All algorithms must implement the `IPathfindingAlgorithm` interface found in `Core/IPathfinder.cs`, view the [README](./README.md#creating-custom-scripts) for a template of an implementation. These algorithms are then discovered and called using **Reflection**.  
The definition of the `IPathfindingAlgorithm`:
```csharp
public interface IPathfindingAlgorithm
{
    string AlgorithmName { get; }
    string Description { get; }

    Task<SimulationResult> CalculatePathsAsync(
        SimulationContext context,
        IProgress<float> progress = null,
        CancellationToken cancellationToken = default);
}
```  
Parameters:
* SimulationContext: An immutable record containing the InitialState (Drones), TargetState (target positions for Drones), and the WorldEnvironment (Grid rules and readonly Obstacles).
* IProgress<float>: Used to send information about calculation progress back to the Unity UI thread.
* CancellationToken: Monitored via cancellationToken.ThrowIfCancellationRequested() to allow the user to abort heavy calculations mid-flight.
  
---

Or an example implementation of a trivial pathfinding script:  
```C#
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using DroneSwarmPathfinder.Core.Models;
using DroneSwarmPathfinder.Core.Simulation;

namespace DroneSwarmPathfinder.Algorithms
{
    /// <summary>
    /// A simple demo algorithm that draws a straight line to the target
    /// </summary>
    public class TrivialPathfinder : IPathfindingAlgorithm
    {
        public string AlgorithmName => "External Trivial Demo Algorithm";
        public string Description => "Moves drones in a straight line to their targets, ignores all collisions";

        public async Task<SimulationResult> CalculatePathsAsync(
            SimulationContext context,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var paths = new Dictionary<int, DronePath>();

            int totalDrones = context.InitialState.Count;
            int processedCount = 0;

            foreach (var kvp in context.InitialState)
            {
                cancellationToken.ThrowIfCancellationRequested(); // Check for cancellation token

                int droneId = kvp.Key;
                Drone startDrone = kvp.Value;

                // Drone stays still if it has no targets
                Drone targetDrone = context.TargetState.ContainsKey(droneId)
                    ? context.TargetState[droneId]
                    : startDrone;

                var path = new DronePath { DroneId = droneId };

                // Create a trivial 2-step path Start->End
                path.Waypoints.Add(new Waypoint(0, startDrone.Transform.Position, startDrone.Transform.Rotation));

                // Assign second step to "step 10" so simulation can interpolate nicely
                path.Waypoints.Add(new Waypoint(10, targetDrone.Transform.Position, targetDrone.Transform.Rotation));

                paths.Add(droneId, path);

                // Report progress back
                processedCount++;
                progress?.Report((float)processedCount / totalDrones);
            }

            stopwatch.Stop();

            return new SimulationResult
            {
                IsSuccessful = true,
                Message = "Trivial path calculated successfully",
                ComputationTime = stopwatch.Elapsed,
                Paths = paths
            };
        }
    }
}
```