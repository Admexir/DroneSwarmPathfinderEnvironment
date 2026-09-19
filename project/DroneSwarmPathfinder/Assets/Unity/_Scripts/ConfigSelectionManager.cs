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
                    startConfig = new DroneConfigJson() { Drones = DroneManager.instance.AllDroneModelsOnly.ToList(), TargetPositions = DroneManager.instance.AllDroneTargetPositionsOnly.ToList() }; //TODO: make a helper function to centralize the "use current scene" serialization
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
                // Empty starting config
                if (sItems.Length == 0) { error = $"Invalid start config file: {StartConfigPath}"; return false; }
                // Empty target config
                if (tItems.Length == 0) { error = $"Invalid target config file: {TargetConfigPath}"; return false; }
                // Start config containing target (non-specific) positions
                if (startConfig.TargetPositions.Count > 0) { error = $"Start config can't contain target positions!"; return false; }
                // Different amount of drones in configs
                if (sItems.Length != tItems.Length) { error = $"Configs have different item count: S = {sItems.Length}, T = {tItems.Length}"; return false; }
                // Target has a specific drone that's not in start
                var missingSpecificDrones = targetConfig.Drones.Select(x => x.ID).Except(startConfig.Drones.Select(x => x.ID));
                if (missingSpecificDrones.Any()) { error = $"Start config doesn't have a drone coresponding to specific ID target: {missingSpecificDrones.First()}"; return false; }
                // Configs have different groups or groups of different sizes
                //if(!startConfig.Groups.GroupsAreSame(targetConfig.Groups)) { error = $"Configs have different groups: {string.Join(", ", startConfig.Groups.Keys.Except(targetConfig.Groups.Keys).Union(targetConfig.Groups.Keys.Except(startConfig.Groups.Keys)).ToArray())}"; return false; }
                // It's impossible to fill target positions with start config
                var requiredIDs = tItems.Select(x => x.ID).ToHashSet();
                var freeDrones = sItems.Where(x => !requiredIDs.Contains(x.ID)).ToList();
                var availableDronesByGroup = freeDrones.GroupBy(x => x.GroupName).ToDictionary(g => g.Key, g => g.Count());
                var groupedTarget = targetConfig.TargetPositions.GroupBy(x => x.GroupName);
                foreach (var targetGroup in groupedTarget)
                {
                    availableDronesByGroup.TryGetValue(targetGroup.Key, out int availableCount);
                    if (availableCount < targetGroup.Count())
                    {
                        error = $"Start config can't fill all target positions (need {targetGroup.Count()}, but only have {availableCount} free drones in group: {targetGroup.Key})";
                        return false;
                    }
                }
                // Has too many drones of some group (can't happen if above is satisfied)
                //if (groupedStart.Except(groupedTarget).Any()) { error = $"Start config has too many free drones (not enough drones with group: {groupedTarget.Except(groupedStart).First().Key})"; return false; }


                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                if(error == "") { error = "Unknown error checking file compatibility"; }
                return false;
            }
        }
    }
}
//public static class GroupsDictionaryComparerExtensions
//{
//    /// <summary>
//    /// Returns true exactly if dictionaries have the same keys and their associated values have the same sizes, used for comparing compatibility of drone groups between configs
//    /// </summary>
//    public static bool GroupsAreSame(this Dictionary<string, List<IDroneConfigItem>> a, Dictionary<string, List<IDroneConfigItem>> b)
//    {
//        foreach(var kvp in a)
//        {
//            if(!b.TryGetValue(kvp.Key, out var val2)) { Debug.Log($"b doesnt have key {kvp.Key}"); return false; }
//            if(kvp.Value.Count != val2.Count) { Debug.Log($"a and b have different count of {kvp.Key}: a has {kvp.Value.Count}, b has {val2.Count}"); return false; }
//        }
//        return true;
//    }
//}