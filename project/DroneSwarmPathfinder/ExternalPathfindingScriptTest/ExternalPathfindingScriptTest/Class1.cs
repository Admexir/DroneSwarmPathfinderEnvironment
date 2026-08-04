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
            var stopwatch = Stopwatch.StartNew(); // For getting calculation time TODO: maybe implement straight into the itnerfaces
            var paths = new Dictionary<int, DronePath>();

            int totalDrones = context.InitialState.Count;
            int processedCount = 0;

            // TODO: REMOVE THIS - simulate heavy calculating load by sleeping for 500ms to test asynchronity
            await Task.Delay(500, cancellationToken);

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

                // TODO: REMOVE THIS - simulate calculating intermediate nodes
                await Task.Delay(100, cancellationToken);

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