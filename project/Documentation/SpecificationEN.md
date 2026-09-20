# Specification

### Simulation Environment for Drone Swarm Pathfinding

The goal of the project is to create a robust simulation and visualization environment
in the Unity game engine (C#) for simulating drone swarm pathfinding.
The application allows loading and saving configurations to JSON files
and provides an API for connecting pathfinding algorithms, which will be
the primary subject of the subsequent bachelor's thesis.


## 1. Basic Information

### 1.1. Description and Focus of the Software Work
It is a simulation environment developed in the C# language using the Unity engine, which serves for the visualization and testing of algorithms for drone swarm navigation (ex., quadcopters). The software is primarily focused on creating a *"sandbox"* - the current iteration focuses exclusively on functional visualization, state management, and the preparation of an API for external scripts. The complex navigation algorithms themselves will only be implemented as part of the subsequent bachelor's thesis. The target audience is developers of 3D pathfinding algorithms.

### 1.2. Used Technologies
* **Language:** C#
* **Unity Engine:** 2022 LTS or newer
* **Data Serialization:** JSON (or a similar data format)

### 1.3. Conventions of this Document
Requirements marked as **[Extension]** represent functionality that will be added in case of sufficient time or during the work on the bachelor's thesis.

---

## 2. Brief Description of the Software Work

### 2.1. Reason for the Creation of the Software Work, its Basic Parts, and Solution Goals
The work is being created as preparation for a bachelor's thesis. The goal is to develop a tool for the development of pathfinding algorithms along with visualization and simulation in a unified environment. The basic parts of the system are:
* **Core:** Definition of interfaces for agents and navigation algorithms (pure C#, independent of Unity).
* **Simulator (Unity):** A 3D scene that interprets data from the Core and visualizes it.
* **State Manager:** A module for loading, saving, and interactively editing initial/target swarm configurations.

### 2.2. Main Functions
* Visualization of agent movement in a 3D grid (**[Extension]** with an architecture prepared for a transition to continuous space).
* Saving and loading simulation states to/from a human-readable format (JSON).
* Visual scene editor allowing adding/removing agents, changing their position, and setting their specific properties in a 3D environment.
* Providing an API for connecting external C# pathfinding algorithms.
* Management of specialized agent properties (groups, colors, physical constraints, etc.).

### 2.3. Motivational Use Case
The user creates a target configuration in the visual editor by adding 10 drones on the left and right sides of the room. They set the drones on the left side to a blue color and on the right side to a red color. They save the state to a JSON file and create an initial configuration so that the blue and red drones must swap positions. The user launches the application, loads the JSON file with the target configuration, and selects the "Trivial Demo Algorithm" from the list. After pressing the "Simulate" button, the application calculates the routes and smoothly visualizes the drones flying past each other in 3D space to their target destination.

### 2.4. Application Environment
The application will be compiled as a standalone executable program for the Windows 10/11 operating system, or possibly OS X or Linux.

### 2.5. Limitations of the Work
* The application at this stage will not contain advanced algorithms for pathfinding (only a trivial demo example).
* Physics will be simplified (use of Unity Physics, simulation of rigidbody collision detection, inertia, acceleration, etc... It will not simulate air resistance, ground effect, turbulence, and so on).

### 2.6. Documentation of the Work
In addition to the standard user documentation (how to operate the program), an *API Development Documentation* will be created, which will exactly describe what interface (e.g., `IPathfinder`) a future algorithm must implement in order to be loaded and run in this visualization environment.

---

## 3. External Interfaces

### 3.1. User Interface, Inputs, and Outputs
The UI will be implemented using Unity Canvas. It will contain:
* Buttons for loading and saving the configuration (invokes a system dialog for file selection).
* Simulation controls: Play, Pause, Stop, a slider for playback speed, and an option for stepping.
* Scene editing tools: Drone selection, movement, a panel for changing properties, adding, and removing drones.

The **Input** is the basic configuration in which the drones start, the algorithm (or algorithms) they are to use, and the target configuration.

The **Output** is a visual 3D representation of the flight, an optionally exported JSON file with the resulting calculated route, and an optionally exported JSON file with the current simulation configuration.

### 3.2. Hardware Interfaces
The application does not require any specific hardware. Control is done using a standard mouse and keyboard (navigation in the 3D scene).

### 3.3. Software Interfaces
The work will communicate with external `.dll` libraries (or `.cs` scripts) containing navigation algorithms using the Reflection mechanism or predefined C# interfaces. This architecture will ensure that the algorithms can be developed completely separately from the Unity project.

### 3.4. Communication Interfaces
The application only uses the local file system for reading and writing data formats (JSON).

---

## 4. Detailed Description of Functionality

### 4.1. Visualization and Movement in 3D Space
The simulation core will operate on a 3D grid, but the internal data structures for representing positions and the actual implementation in Unity are designed so that they can be smoothly extended to continuous space (arbitrary angle and distance) without the need to rewrite the visualization core.

The user will be able to move in all directions within the simulation space using the keyboard and mouse.

### 4.2. State Serialization and Deserialization
The system can export the current layout of agents in the scene to JSON format and, conversely, build the scene from it. These files are human-readable, which allows the user to create the initial (configuration 1) and target (configuration 2) state manually in a text editor (although using the visual editor is preferred). The positions, orientations, and unique IDs of the agents are saved.

### 4.3. API for External Pathfinding Scripts
The basic building block for the future bachelor's thesis. The visualization contains a bridge that accepts objects implementing a common interface. Unity will pass the initial (or current) and target state to this interface and expects the return of a set of routes for individual drones. Currently, only a trivial demo algorithm will be implemented.

### 4.4. Extensible Agent Properties
The agent structure will be designed polymorphically to allow the assignment of special properties and group behavior. Supported properties include:
* **Identification:** Indistinguishable agents vs. distinguishable (e.g., 10 blue drones can fly to any of the 10 blue targets).
* **[Extension] Physical Limits:** Restrictions on acceleration, maximum power, or the creation of forbidden zones (ex. a high-power agent creates turbulence beneath it, which prevents another agent from flying through).

---

## 5. Other (Non-Functional) Requirements

### 5.1. Performance Requirements
Simulation and visualization must run smoothly (minimum 30 FPS) for a swarm of up to 50 drones in a space of up to 100x100x100 units. The cost of visualization should be negligible compared to the runtime cost of the navigation algorithms. The pathfinding algorithms can run asynchronously in the background so as not to block the engine's rendering thread.

### 5.2. Requirements for Extensibility and Integrability
The grid data structures must be easily replaceable with a continuous space, and the method of passing data between the visualization and computational scripts must be separated by an interface so that no intervention in the visualization code is necessary when changing the algorithm in the future.

---

## 6. Out of Scope

The following are not part of this work:
* The development of the optimal navigation algorithm itself for the bachelor's thesis (only the testing environment and a trivial demo are being addressed).
* Data export to physical drones.
* Physically accurate simulation of the environment (ex. exact air resistance or other aerodynamic phenomena, weather).
