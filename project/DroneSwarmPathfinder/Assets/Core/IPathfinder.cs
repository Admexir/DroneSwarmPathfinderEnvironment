using System.Threading.Tasks;

namespace DroneSwampPathfiner.Simulation
{
    /// <summary>
    /// Class holding information about drones' transforms
    /// </summary>
    public class SimulationDronesTransformContext
    {
        public DroneTransform[] DronesTransformArray;
    }
    /// <summary>
    /// Record class to hold information about the simulations results
    /// </summary>
    public record SimulationResult
    {
        //TODO: Add info about result of the simulation ... JSON/strings/...
    }
    /// <summary>
    /// Record class holding information about the environment and drones for a simulation
    /// </summary>
    public record SimulationContext
    {
        public SimulationDronesTransformContext InitialDroneTransforms { get; set; }
        public SimulationDronesTransformContext TargetDroneTransforms { get; set; }

    }

    /// <summary>
    /// Interface with methods for external pathfinding scripts to call - API for actions of a drone
    /// </summary>
    public interface IDronePathfinder
    {

    }

    /// <summary>
    /// Interface for a simulation scenario, contains methods to calculate paths, run/pause simulation,...
    /// </summary>
    public interface ISimulation
    {
        Task<SimulationResult> CalculatePathsAsync(SimulationContext context);
    }

    /// <summary>
    /// Interface for individual drones participating in a simulation
    /// </summary>
    public interface IDrone
    {

    }

}
