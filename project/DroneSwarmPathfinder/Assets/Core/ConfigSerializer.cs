using DroneSwarmPathfinder.Core.Environment;
using DroneSwarmPathfinder.Core.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace DroneSwarmPathfinder.Core.Serialization
{
    /// <summary>
    /// Class representing the entire saveable state of a simulation scene
    /// </summary>
    public class SimulationConfig
    {
        public List<Drone> Drones { get; set; } = new();
        public List<BoxObstacle> Obstacles { get; set; } = new();

        /// <summary>
        /// Concats all item lists into a single generic collection
        /// </summary>
        [JsonIgnore]
        public IEnumerable<IConfigItem> AllConfigItems =>
            Drones.Cast<IConfigItem>().Concat(Obstacles.Cast<IConfigItem>());
    }

    /// <summary>
    /// Class for serializing configurations to and from JSON
    /// </summary>
    public static class ConfigSerializer
    {
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Formatting = Newtonsoft.Json.Formatting.Indented, // nicer :)
            // TypeNameHandling allows for polymorphic saving later (in case I'd need for example different types of drones...)
            TypeNameHandling = TypeNameHandling.Auto
        };

        public static string Serialize(SimulationConfig config)
        {
            return JsonConvert.SerializeObject(config, _settings);
        }

        public static SimulationConfig Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<SimulationConfig>(json, _settings);
        }
    }
}