using System;
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

        public void SetTargetConfigPath(string path)
        {
            TargetConfigPath = path;
            OnScenarioStateChanged?.Invoke();
        }
    }
}