using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DroneSwarmPathfinder.Core.Simulation
{
    using Models;
    using Environment;
    using DroneSwarmPathfinder.Core.Serialization;

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
        public IReadOnlyDictionary<int, Drone> DroneSpecificTargets { get; init; }
        public IReadOnlyDictionary<string, List<DroneTargetPosition>> GroupTargets { get; init; }
        public WorldEnvironment Environment { get; init; }

        #region Helpers for convenience of use
        private Dictionary<string, int> _groupTargetIndexes;
        public bool TryGetTargetFromGroup(string groupName, out DroneTargetPosition? targetPosition) 
        { 
            if (_groupTargetIndexes == null)
            {
                _groupTargetIndexes = new Dictionary<string, int>();
                foreach(var kvp in GroupTargets)
                {
                    _groupTargetIndexes[kvp.Key] = kvp.Value.Count-1;
                }
            }
            if (_groupTargetIndexes.TryGetValue(groupName, out int index) && index >= 0)
            {
                targetPosition = GroupTargets[groupName][index];
                _groupTargetIndexes[groupName]--;
            }
            else { targetPosition = null; }

            return targetPosition != null;
        }
        #endregion
    }

    /// <summary>
    /// API for any external pathfinding script to implement
    /// </summary>
    public interface IPathfindingAlgorithm
    {
        string AlgorithmName { get; }
        string Description { get; }

        /// <summary>
        /// Calculates the paths for the swarm. Designed to run on a background thread
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