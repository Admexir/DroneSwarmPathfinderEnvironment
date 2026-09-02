# Drone Swarm Pathfinder

## Overview
Drone Swarm Pathfinder is a 3D sandbox simulation environment built in the Unity Engine (C#) used for the visualisation and testing of drone pathfinding algorithms (ex. quadrocopters). 
This application is a developer tool. It provides a visual editor to construct 3D configurations of drones and obstacles, an engine to execute external pathfinding algorithms asynchronously, and a playback system to visualize the calculated trajectories.

## Getting Started
Open the DroneSwarmPathfinder.exe file to run the application. You will load into a 3D environment with interactible panels on the sides of the screen.

### Camera Controls
Navigate the 3D environment using these fly-camera controls:
* **Hold Right Mouse Button -** Look around
* **W, A, S, D -** Move forward, left, backward, right
* **Q / E (or Space/Ctrl) -** Move down / up
* **Left Shift -** Move faster
### Other controls
* **Escape -** Opens/closes the pause menu

## User Interface and Workflow

The user interface is divided into a *Left Panel* (tools and simulation playback), a *Right Panel* (inspector), a floating *Toolbar* for in-scene gizmo manipulation and a *pause screen*.

### Editor Tab (scene construction)
Use this tab to build your environment and agent starting points.
* **Add Drone / Add Obstacle:** Spawns a new entity at the center of the scene.
* **Add Drone Target Position:** Spawns a target position - a position for any drone from a certain group (usable only in target configurations).
* **Inspector (Right Panel):** When you select an entity, its properties appear here. Allows you to assign Drones to specific Groups (teams/colors) or modify exact XYZ coordinates and obstacle scales.
* **Gizmo Toolbar:** Allows you to switch the currently used tool to manipulate the scene - **Select**, **Move**, and **Scale** modes.

### File Tab (configuration loading and algorithms)
This tab handles loading/saving states and running external algorithms.
* **Use physics:** A toggle for whether the simulation works in a physics based environment (not yet implemented).
* **Export / Load Swarm:** Saves your current 3D Drone layout to a local JSON file, or loads an existing one.
* **Export / Load Environment:** Saves your current 3D Obstacle layout and grid settings to a local JSON file, or loads an existing one.
* **Algorithm Selection:** * Select the used algorithm from the dropdown. The `Trivial Demo Algorithm` is included by default.
  * **Import External DLL:** Click this to browse your operating system for a custom compiled `.dll` containing a new pathfinding algorithm. On how to create these, check the [Developer Documentation](./Developer.md).
* **Run Algorithm:** Executes the selected algorithm on the selected starting and target configurations in the background.
* **Scenario settings:** Select the used environment, starting swarm configuration (or use the current scene), and target swarm configuration.
* **Simulation results:** Saves your current simulation results (created by running an algorithm) to a local JSON file, or loads an existing one. Recently calculated and loaded results are cached into the *Recent Results* dropdown.

### Playback Tab (visualization)
Once an algorithm successfully calculates a path, the application will automatically switch to this tab.
* **Controls:** 
  * **Play/Pause:** Plays or pauses playback of the visualization.
  * **Step Forward:** Skips to the next step given by the pathfinding algorithm.
  * **Step Backward:** Skips to the previous step given by the pathfinding algorithm.
  * **Restart:** Skips to the first step of the playback.
* **Time Scale:** Adjust the slider to speed up or slow down the visual interpolation of the drones moving along their computed paths. Double click the percentage to enter an exact number.

## Configuration Data Format (.json)
Configurations are saved in human-readable JSON files. You can safely edit these files manually using any text editor. The system uses `Newtonsoft.Json` to handle serialization.

Example Drone swarm configuration structure:
```json
{
  "Drones": [
    {
      "ID": 0,
      "GroupName": "Group 1",
      "Transform": {
        "Position": { "X": 0.0, "Y": 0.0, "Z": 0.0 },
        "Rotation": { "X": 0.0, "Y": 0.0, "Z": 0.0, "W": 1.0, "IsIdentity": true },
        "Size": { "X": 1.0, "Y": 1.0, "Z": 1.0 }
      }
    }],
    "TargetPositions": []
}
```  
  
Example environment configuration structure:
```json
{
  "Obstacles": [
    {
    "$type": "DroneSwarmPathfinder.Core.Environment.BoxObstacle, Core",
    "ID": 1,
    "Transform": {
      "Position": { "X": 0.0, "Y": 4.0, "Z": 0.0 },
      "Rotation": { "X": 0.0, "Y": 0.0, "Z": 0.0, "W": 1.0, "IsIdentity": true },
      "Size": { "X": 7.30899668, "Y": 1.0, "Z": 4.90738869 }
    }
  }],
  "SpatialRules": {
    "$type": "DroneSwarmPathfinder.Core.Environment.DiscreteGrid, Core",
    "CellSize": 1.0
  }
}


```

## Creating Custom Scripts
The application uses **Reflection** to load external DLLs at runtime. This allows you to write pathfinding algorithms in standard IDEs without interacting with the Unity project.
* **Create a New Visual Studio Project:** Create a new **Class Library** template project for C# and choose the .NET Standard 2.1. framework.
    * *Note: Set the Language Version to C# 9.0 (or later) in your .csproj file to support init-only setters and records used by the Core API.*
* **Add Reference To The Dll:** In the solution explorer, right click **Dependencies** and select **Add Project Reference**. Select browse and navigate to the "Algorithms API" folder in the DroneSwarmPathfinder repository. Select the **Core.dll** file.
* **Write the Script:** Now create the pathfinding script by filling out the template at the bottom of this file. Then build it (ctrl + shift + b) and take the produced .dll to the next step.
* **Import the Script:** Launch the Drone Swarm Pathfinder application, navigate to the **File** tab, click **Import External DLL...** and find the .dll from the previous step. After selecting it, it should appear in the dropdown above.
* **Run the Simulation:** Select the configurations you want to run your script on (or create them inside the editor). After selecting the starting and target configuration, simply click the **Run Algorithm** button. It will execute the calculations and switch to the Playback tab.
* **Watch the Playback:** Finally use the playback tools to watch the results of your script!

---

### Pathfinding Script Template
```C#
using System;
using System.Threading;
using System.Threading.Tasks;
using DroneSwarmPathfinder.Core.Models;
using DroneSwarmPathfinder.Core.Simulation;

namespace DroneSwarmPathfinder.Algorithms
{
    public class TrivialPathfinder : IPathfindingAlgorithm
    {
        public string AlgorithmName => "Name for your algorithm";
        public string Description => "Description for your algorithm";

        public async Task<SimulationResult> CalculatePathsAsync(
            SimulationContext context,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default)
        {
            // var paths = new Dictionary<int, DronePath>(); // The main "result" of the simulation - a dictionary of drone IDs -> paths

            // SimulationContext contains all information about the configuration:
            // public record SimulationContext
            // {
            //     public IReadOnlyDictionary<int, Drone> InitialState { get; init; }
            //     public IReadOnlyDictionary<int, Drone> DroneSpecificTargets { get; init; }
            //     public IReadOnlyDictionary<string, List<DroneTargetPosition>> GroupTargets { get; init; }
            //     public WorldEnvironment Environment { get; init; }

            //     #region Helpers for convenience of use
            //     private Dictionary<string, int> _groupTargetIndexes;
            //     public bool TryGetTargetFromGroup(string groupName, out DroneTargetPosition? targetPosition) 
            //     { 
            //         if (_groupTargetIndexes == null)
            //         {
            //             _groupTargetIndexes = new Dictionary<string, int>();
            //             foreach(var kvp in GroupTargets)
            //             {
            //                 _groupTargetIndexes[kvp.Key] = kvp.Value.Count-1;
            //             }
            //         }
            //         if (_groupTargetIndexes.TryGetValue(groupName, out int index) && index >= 0)
            //         {
            //             targetPosition = GroupTargets[groupName][index];
            //             _groupTargetIndexes[groupName]--;
            //         }
            //         else { targetPosition = null; }

            //         return targetPosition != null;
            //     }
            //     #endregion
            // }
            //
            // InitialState and TargetState are dictionaries of IDs->Drone, where each Drone has an ID (int), a GroupName (string), and a Transform (containing Position, Rotation and Size))
            // GroupTargets is a dictionary of Group Name->List<DroneTargetPosition>, where a GroupTargetPosition has an ID, a GroupName, and a Transform
            // WorldEnvironment has SpatialRules - a representation of a grid/special coordinate system, which allows you to call ConstrainPosition to snap a Vector3 position to the grid
            //                  and a list of Obstacles along with a method IsWalkable(Vector3), which returns whether the given position is empty
            // TryGetTargetFromGroup is a helper method for working with drone group targets - it returns (in the out variable) the first not-yet-returned group target position of the given group


            // Pathfinding logic -------
            
            // You should allow the calculations to be aborted using the CancellationToken: 
            // cancellationToken.ThrowIfCancellationRequested(); 

            // As this is a "Task", it is heavily recommended to asynchronize the pathfinding calculations if they're not trivial

            // var path = new DronePath { DroneId = droneId }; // Create a path for the drone to take
            // path.Waypoints.Add(new Waypoint(0, targetDrone.Transform.Position, targetDrone.Transform.Rotation)); // Create a "waypoint" (a place the drone will go through) on the path of a drone
            // Waypoint constructor has 3 parameters: Index, aka a timestamp, when will the drone reach this position, a target position and a target rotation
            // paths.Add(droneId, path); // Register the path for the given drone

            // You can report progress percentage like so:
            // progress?.Report((float)completionFraction);

            // Return the results
            return new SimulationResult
            {
                IsSuccessful = true,
                Message = "Path calculated successfully",
                // ComputationTime = stopwatch.Elapsed, // You can track the time taken to compute the paths
                Paths = paths
            };
        }
    }
}
```

---
To see the by default included `Trivial Demo Algorithm`, it is included in the documentation [here](./TrivialDemoAlgorithm.cs)