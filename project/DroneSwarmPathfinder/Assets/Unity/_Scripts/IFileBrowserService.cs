using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DroneSwarmPathfinder.Unity.Services
{
    /// <summary>
    /// Interface used for abstracting file browser dialogs
    /// </summary>
    public interface IFileBrowserService
    {
        /// <summary>
        /// Opens an OS dialog to select a file to load
        /// </summary>
        /// <returns>The absolute file path, or an empty string if the user canceled</returns>
        string RequestLoadPath(string title, string extension);

        /// <summary>
        /// Opens an OS dialog to choose where to save a file
        /// </summary>
        /// <returns>The absolute file path, or an empty string if the user canceled</returns>
        string RequestSavePath(string title, string defaultName, string extension);
    }
}