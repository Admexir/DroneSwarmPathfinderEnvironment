using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DroneSwarmPathfinder.Core.Simulation
{
    using Models;
    using Environment;

    /// <summary>
    /// Record class to hold information about the simulations results
    /// </summary>
    public record SimulationResult
    {
        public bool IsSuccessful { get; init; }
        public string Message { get; init; }
        public TimeSpan ComputationTime { get; init; }
        public IReadOnlyDictionary<int, DronePath> Paths { get; init; }
    }

    /// <summary>
    /// Record class holding information about the environment and drones for a simulation
    /// </summary>
    public record SimulationContext
    {
        public IReadOnlyDictionary<int, Drone> InitialState { get; init; }
        public IReadOnlyDictionary<int, Drone> TargetState { get; init; }
        public WorldEnvironment Environment { get; init; }
    }

    /// <summary>
    /// API for any external pathfinding script to implement
    /// </summary>
    public interface IPathfindingAlgorithm
    {
        string AlgorithmName { get; }
        string Description { get; }

        /// <summary>
        /// Calculates the paths for the swarm. Designed to run on a background thread.
        /// </summary>
        /// <param name="context">The simulation layout</param>
        /// <param name="progress">Delegate wrapper for reporting percentual progress</param>
        /// <param name="cancellationToken">Token to abort the calculation if the user stops it</param>
        Task<SimulationResult> CalculatePathsAsync(
            SimulationContext context,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default);
    }
}