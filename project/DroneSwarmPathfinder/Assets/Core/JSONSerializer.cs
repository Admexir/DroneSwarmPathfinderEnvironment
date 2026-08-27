using DroneSwarmPathfinder.Core.Environment;
using DroneSwarmPathfinder.Core.Models;
using DroneSwarmPathfinder.Core.Simulation;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace DroneSwarmPathfinder.Core.Serialization
{
    /// <summary>
    /// Class representing the saveable state of the drones
    /// </summary>
    public class DroneConfigJson
    {
        public List<Drone> Drones { get; set; } = new();

        [JsonIgnore]
        public IEnumerable<IConfigItem> AllConfigItems => Drones.Cast<IConfigItem>();
    }

    /// <summary>
    /// Class representing the saveable state of the environment
    /// </summary>
    public class EnvironmentConfigJson
    {
        public List<IObstacleVolume> Obstacles { get; set; } = new();
        public ISpatialEnvironment SpatialRules { get; set; }

        public EnvironmentConfigJson(WorldEnvironment env)
        {
            Obstacles = env.Obstacles.ToList();
            SpatialRules = env.SpatialRules;
        }

        [JsonConstructor] private EnvironmentConfigJson() { }
        // TODO: physics toggle and other environment specific variables go here
    }

    /// <summary>
    /// Class representing the saveable state of a simulation result
    /// </summary>
    public class ResultsJson
    {
        // In case this needs to hold more info, just add it as a variable and set the value in the FullResultObject conversion
        [JsonIgnore] public SimulationResult FullResultObject => new SimulationResult() { Paths = this.Paths };
        public IReadOnlyDictionary<int, DronePath> Paths { get; init; }
        public ResultsJson(SimulationResult result) { this.Paths = result.Paths; }
        [JsonConstructor] private ResultsJson() { }
    }

    /// <summary>
    /// Class for serializing (primarily) configurations to and from JSON
    /// </summary>
    public static class JSONSerializer
    {
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Formatting = Newtonsoft.Json.Formatting.Indented, // nicer :)
            // TypeNameHandling allows for polymorphic saving later (in case I'd need for example different types of drones...)
            TypeNameHandling = TypeNameHandling.Auto
        };

        /// <summary>
        /// Serializes any C# object into a JSON string using Newtonsoft.Json
        /// </summary>
        public static string Serialize<T>(T config)
        {
            return JsonConvert.SerializeObject(config, _settings);
        }

        /// <summary>
        /// Deserializes any JSON string into a C# object using Newtonsoft.Json
        /// </summary>
        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }

        /// <summary>
        /// Deserializes any JSON file at the given path into a C# object using Newtonsoft.Json
        /// </summary>
        public static T DeserializeFile<T>(string path)
        {
            var json = System.IO.File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }
    }
}