using DroneSwarmPathfinder.Core.Serialization;
using System;
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
                if (tItems.Length == 0) { error = $"Invalid start config file: {StartConfigPath}"; return false; }
                if (sItems.Length != tItems.Length) { error = $"Configs have different item count: S = {sItems.Length}, T = {tItems.Length}"; return false; }
                for (int i = 0; i < sItems.Length; i++)
                {
                    if (sItems[i].ID != tItems[i].ID) { error = $"Configs don't have matching drone IDs, example: {sItems[i].ID}"; return false; }
                }
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