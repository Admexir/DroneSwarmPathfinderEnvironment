using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DroneSwarmPathfinder.Core.Models;
using DroneSwarmPathfinder.Core.Serialization;
using DroneSwarmPathfinder.Core.Simulation;

namespace DroneSwarmPathfinder.Algorithms
{
    /// <summary>
    /// A simple demo algorithm that draws a straight line to the target with 10 inbetween steps
    /// </summary>
    public class TrivialPathfinder : IPathfindingAlgorithm
    {
        public string AlgorithmName => "Trivial Demo Algorithm";
        public string Description => "Moves drones in a straight line to their targets, ignores all collisions";

        public async Task<SimulationResult> CalculatePathsAsync(
            SimulationContext context,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew(); // For getting calculation time
            var paths = new Dictionary<int, DronePath>();

            int totalDrones = context.InitialDrones.Count;
            int processedCount = 0;

            // Doing the pathfinding calculations asynchronously is heavily recommended unless trivial like bellow

            foreach (var kvp in context.InitialDrones)
            {
                cancellationToken.ThrowIfCancellationRequested(); // Check for cancellation token

                int droneId = kvp.Key;
                Drone startDrone = kvp.Value;

                // Drone looks if it has exact index match, if not then for a group match, and if neither stays still (should always have at least 1 so configs pass compatibility checks)
                IDroneConfigItem targetDrone = context.DroneSpecificTargets.ContainsKey(droneId)
                    ? context.DroneSpecificTargets[droneId]
                    : (context.TryGetTargetFromGroup(startDrone.GroupName, out var target) 
                        ? target : startDrone);

                var path = new DronePath { DroneId = droneId };

                // Create a trivial 2-step path Start->End
                path.Waypoints.Add(new Waypoint(0, startDrone.Transform.Position, startDrone.Transform.Rotation));

                // Assign second step to "step 10" so simulation can interpolate nicely
                path.Waypoints.Add(new Waypoint(10, targetDrone.Transform.Position, targetDrone.Transform.Rotation));

                paths.Add(droneId, path);

                // Report progress back - not required, but will allow the progress bar to show calculation progress
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