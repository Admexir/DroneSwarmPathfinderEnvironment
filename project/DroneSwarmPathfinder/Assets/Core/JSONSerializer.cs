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
        /// <summary>
        /// List of all drones in the swarm
        /// </summary>
        public List<Drone> Drones { get; set; } = new();
        /// <summary>
        /// List of all general target drone positions ("any drone from group x")
        /// </summary>
        public List<DroneTargetPosition> TargetPositions { get; set; } = new();

        ///// <summary>
        ///// Dictionary of all drone groups
        ///// </summary>
        //[JsonIgnore]
        //public Dictionary<string, List<IDroneConfigItem>> Groups
        //{
        //    get
        //    {
        //        // Generate the groups dictionary from the drones list if it hasn't been generated before
        //        if(_groups == null)
        //        {
        //            _groups = new Dictionary<string, List<IDroneConfigItem>>();
        //            foreach (var drone in AllConfigItems)
        //            {
        //                if (_groups.TryGetValue(drone.GroupName, out var list)) { list.Add(drone); }
        //                else { _groups[drone.GroupName] = new List<IDroneConfigItem>() { drone }; }
        //            }
        //            if (!_groups.ContainsKey("default")) _groups["default"] = new(); // Edge case for when no drone uses default group so it doesn't get deleted
        //        }
        //        return _groups;
        //    }
        //}
        [JsonIgnore]
        private Dictionary<string, List<IDroneConfigItem>> _groups;

        /// <summary>
        /// List of all items contained in this file (currently only the drones)
        /// </summary>
        [JsonIgnore]
        public IEnumerable<IDroneConfigItem> AllConfigItems => Drones.Cast<IDroneConfigItem>().Concat(TargetPositions.Cast<IDroneConfigItem>());
    }

    /// <summary>
    /// Class representing the saveable state of the environment
    /// </summary>
    public class EnvironmentConfigJson
    {
        /// <summary>
        /// List of all the obstacles
        /// </summary>
        public List<IObstacleVolume> Obstacles { get; set; } = new();
        /// <summary>
        /// Spatial rules of the environment (grid size)
        /// </summary>
        public ISpatialEnvironment SpatialRules { get; set; }

        public EnvironmentConfigJson(WorldEnvironment env)
        {
            Obstacles = env.Obstacles.ToList();
            SpatialRules = env.SpatialRules;
        }

        [JsonIgnore]
        public IEnumerable<IEnvironmentConfigItem> AllConfigItems => Obstacles.Cast<IEnvironmentConfigItem>();

        [JsonConstructor] private EnvironmentConfigJson() { }
        // TODO: physics toggle and other environment specific variables go here
    }

    /// <summary>
    /// Class representing the saveable state of a simulation result
    /// </summary>
    public class ResultsJson
    {
        // In case this needs to hold more info, just add it as a variable and set the value in the FullResultObject conversion

        /// <summary>
        /// Conversion to SimulationResult
        /// </summary>
        [JsonIgnore] public SimulationResult FullResultObject => new SimulationResult() { Paths = this.Paths, SimulationContext = this.Context };
        /// <summary>
        /// Dictionary of all drones as keys and their paths as values
        /// </summary>
        public IReadOnlyDictionary<int, DronePath> Paths { get; init; }
        public SimulationContext Context { get; init; }
        public ResultsJson(SimulationResult result) { this.Paths = result.Paths; this.Context = result.SimulationContext; }
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