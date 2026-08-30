using DroneSwarmPathfinder.Core.Models;
using DroneSwarmPathfinder.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DroneSwarmPathfinder.Unity.Managers
{
    /// <summary>
    /// Class used as a holder for the simulation configuration parameters
    /// </summary>
    public class ConfigSelectionManager : MonoBehaviour
    {
        public static ConfigSelectionManager instance;
        private void Awake() => instance = this;

        public bool UseCurrentSceneForStart { get; private set; } = true;
        public string StartConfigPath { get; private set; }
        public string TargetConfigPath { get; private set; }
        public string EnvironmentConfigPath { get; private set; }
        public event Action OnScenarioStateChanged; // For UI changes

        public void SetUseCurrentScene(bool useCurrent)
        {
            UseCurrentSceneForStart = useCurrent;
            OnScenarioStateChanged?.Invoke();
        }

        public void SetStartConfigPath(string path)
        {
            StartConfigPath = path;
            OnScenarioStateChanged?.Invoke();
        }

        public void SetEnvironmentConfigPath(string path)
        {
            EnvironmentConfigPath = path;
            OnScenarioStateChanged?.Invoke();
        }

        public void SetTargetConfigPath(string path)
        {
            TargetConfigPath = path;
            OnScenarioStateChanged?.Invoke();
        }

        /// <summary>
        /// Verifies whether two selected drone configurations are compatible - have the same number of drones, same groups,...
        /// If a config is not selected, returns true
        /// </summary>
        public bool CheckConfigCompatibility(out string error)
        {
            error = "";
            if ((StartConfigPath == null && !UseCurrentSceneForStart) || TargetConfigPath == null) return true; // exit if one of the configs is not yet selected
            try
            {
                DroneConfigJson startConfig;
                if (UseCurrentSceneForStart)
                {
                    startConfig = new DroneConfigJson() { Drones = DroneManager.instance.AllDroneModels.ToList() }; //TODO: make a helper function to centralize the "use current scene" serialization
                    // also it'd be nice to despaghettify this :)
                }
                else
                {
                    error = $"Invalid start config file: {StartConfigPath}";
                    startConfig = JSONSerializer.DeserializeFile<DroneConfigJson>(StartConfigPath);

                }
                error = $"Invalid target config file: {StartConfigPath}";
                var targetConfig = JSONSerializer.DeserializeFile<DroneConfigJson>(TargetConfigPath);
                error = "";

                var sItems = startConfig.AllConfigItems.OrderBy(x => x.ID).ToArray();
                var tItems = targetConfig.AllConfigItems.OrderBy(x => x.ID).ToArray();
                if (sItems.Length == 0) { error = $"Invalid start config file: {StartConfigPath}"; return false; }
                if (tItems.Length == 0) { error = $"Invalid target config file: {TargetConfigPath}"; return false; }
                if (sItems.Length != tItems.Length) { error = $"Configs have different item count: S = {sItems.Length}, T = {tItems.Length}"; return false; }
                for (int i = 0; i < sItems.Length; i++)
                {
                    if (sItems[i].ID != tItems[i].ID) { error = $"Configs don't have matching drone IDs, example: {sItems[i].ID}"; return false; }
                }
                if(!startConfig.Groups.GroupsAreSame(targetConfig.Groups)) { error = $"Configs have different groups: {string.Join(", ", startConfig.Groups.Keys.Except(targetConfig.Groups.Keys).Union(targetConfig.Groups.Keys.Except(startConfig.Groups.Keys)).ToArray())}"; return false; }
                return true;
            }
            catch
            {
                if(error == "") { error = "Unknown error checking file compatibility"; }
                return false;
            }
        }
    }
}
public static class GroupsDictionaryComparerExtensions
{
    /// <summary>
    /// Returns true exactly if dictionaries have the same keys and their associated values have the same sizes, used for comparing compatibility of drone groups between configs
    /// </summary>
    public static bool GroupsAreSame(this Dictionary<string, List<Drone>> a, Dictionary<string, List<Drone>> b)
    {
        foreach(var kvp in a)
        {
            if(!b.TryGetValue(kvp.Key, out var val2)) { Debug.Log($"b doesnt have key {kvp.Key}"); return false; }
            if(kvp.Value.Count != val2.Count) { Debug.Log($"a and b have different count of {kvp.Value}: {kvp.Value.Count} x {val2.Count}"); return false; }
        }
        return true;
    }
}