using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DroneSwarmPathfinder.Unity.Services
{
    /// <summary>
    /// Desktop implementation of the file browser
    /// </summary>
    public class DesktopFileBrowserService : IFileBrowserService
    {
        public string RequestLoadPath(string title, string extension)
        {
#if UNITY_EDITOR
            // (works only in unity editor!!!!!)
            return EditorUtility.OpenFilePanel(title, Application.dataPath, extension);
#else
            // TODO: Create implementation that works on a build
            return string.Empty;
#endif
        }

        public string RequestSavePath(string title, string defaultName, string extension)
        {
#if UNITY_EDITOR
            return EditorUtility.SaveFilePanel(title, Application.dataPath, defaultName, extension);
#else
            // TODO: Create implementation that works on a build
            return string.Empty;
#endif
        }
    }
}