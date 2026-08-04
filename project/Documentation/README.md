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

## User Interface and Workflow

The user interface is divided into a *Left Panel* (tools and simulation playback), a *Right Panel* (inspector), and a floating *Toolbar* for in-scene gizmo manipulation.

### Editor Tab (scene construction)
Use this tab to build your environment and agent starting points.
* **Add Drone / Add Obstacle:** Spawns a new entity at the center of the scene.
* **Inspector (Right Panel):** When you select an entity, its properties appear here. Allows you to assign Drones to specific Groups (teams/colors) or modify exact XYZ coordinates and obstacle scales.
* **Gizmo Toolbar:** Allows you to switch the currently used tool to manipulate the scene - **Select**, **Move**, and **Scale** modes.

### File Tab (configuration loading and algorithms)
This tab handles loading/saving states and running external algorithms.
* **Export / Load Configuration:** Saves your current 3D layout to a local JSON file, or loads an existing one.
* **Scenario Settings:** Define the scenario you want to simulate. You must select a **Target Config** (where the drones need to go). You can either select a **Starting Config** file or check **Use Current Scene** to use the layout currently visible in the editor.
* **Algorithm Selection:** * Select the used algorithm from the dropdown. The `Trivial Demo Algorithm` is included by default.
  * **Import External DLL:** Click this to browse your operating system for a custom compiled `.dll` containing a new pathfinding algorithm. On how to create these, check the [Developer Documentation](./Developer.md).
* **Run Algorithm:** Executes the selected algorithm on the selected starting and target configurations in the background.

### Playback Tab (visualization)
Once an algorithm successfully calculates a path, the application will automatically switch to this tab.
* **Controls:** 
  * **Play/Pause:** Plays or pauses playback of the visualization.
  * **Step Forward:** Skips to the next step given by the pathfinding algorithm.
  * **Step Backward:** Skips to the previous step given by the pathfinding algorithm.
  * **Restart:** Skips to the first step of the playback.
* **Time Scale:** Adjust the slider to speed up or slow down the visual interpolation of the drones moving along their computed paths.

## Configuration Data Format (.json)
Configurations are saved in human-readable JSON files. You can safely edit these files manually using any text editor. The system uses `Newtonsoft.Json` to handle serialization.

Example structure:
```json
{
  "Drones": [
    {
      "ID": 0,
      "GroupId": 0,
      "Transform": {
        "Position": { "X": 9.0, "Y": -9.0, "Z": -3.0 },
        "Rotation": { "X": 0.0, "Y": 0.0, "Z": 0.0, "W": 1.0, "IsIdentity": true },
        "Size": { "X": 1.0, "Y": 1.0, "Z": 1.0 }}
    },
    {
      "ID": 1,
      "GroupId": 1,
      "Transform": {
        "Position": { "X": 0.0, "Y": 6.0, "Z": 24.0 },
        "Rotation": { "X": 0.0, "Y": 0.0, "Z": 0.0, "W": 1.0, "IsIdentity": true },
        "Size": { "X": 1.0, "Y": 1.0, "Z": 1.0 }}
    }
  ],
  "Obstacles": []
}
```