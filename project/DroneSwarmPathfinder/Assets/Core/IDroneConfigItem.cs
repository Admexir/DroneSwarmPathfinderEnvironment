using DroneSwarmPathfinder.Core.Models;
using System.Drawing;

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
    /// <summary>
    /// Interface for a drone item (drone or position) that can be serialized into a configuration file
    /// </summary>
    public interface IDroneConfigItem : IConfigItem
    {
        string GroupName { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        Color Color { get; set; }
        bool IsUsingGroupColor { get; set; }
    }

    /// <summary>
    /// Interface for an obstacle volume item that can be serialized into a configuration file
    /// </summary>
    public interface IEnvironmentConfigItem : IConfigItem
    {
        // Nothing unique required yet, but in the future for example bool for whether it is passable,...
    }
}