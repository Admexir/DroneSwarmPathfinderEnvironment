using DroneSwarmPathfinder.Core.Models;

namespace DroneSwarmPathfinder.Core.Serialization
{
    /// <summary>
    /// Interface for any item that can be serialized into a configuration file
    /// </summary>
    public interface IConfigItem
    {
        int ID { get; }
        TransformData Transform { get; set; }
    }
}