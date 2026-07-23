using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Numerics;

namespace DroneSwampPathfiner.Core.Simulation
{
    using Models;

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
        public DiscreteGrid EnvironmentGrid { get; init; }
    }

    /// <summary>
    /// Interface with methods for external pathfinding scripts to call - API for actions of a drone
    /// </summary>
    public interface IPathfindingAlgorithm
    {
        string AlgorithmName { get; }
        Task<SimulationResult> CalculatePathsAsync(SimulationContext context);
    }

}
