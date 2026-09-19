using UnityEngine;
using System.Collections.Generic;
using DroneSwarmPathfinder.Core.Models;
using DroneSwarmPathfinder.Unity.Managers;
using System.Linq;

namespace DroneSwarmPathfinder.Unity.Visuals
{
    /// <summary>
    /// Class responsible for rendering calculated drone path lines
    /// </summary>
    public class PathVisualizer : MonoBehaviour
    {
        [Header("Visual Config")]
        public GameObject linePrefab;
        public Transform linesHolder;
        public float lineWidth = 0.05f;
        public float lineAlpha = 0.5f;

        private List<GameObject> _activeLineObjects = new();

        public void DrawPaths(IReadOnlyDictionary<int, DronePath> paths)
        {
            ClearPaths();

            if (paths == null) return;

            foreach (var kvp in paths)
            {
                int droneId = kvp.Key;
                DronePath path = kvp.Value;

                // Exit if theres no movement
                if (path.Waypoints.Count < 2) continue;

                GameObject lineObj = GameObject.Instantiate(linePrefab, linesHolder);
                lineObj.name = lineObj.name.Substring(0, lineObj.name.Length-"(Clone)".Length) + droneId; // Rename path GameObject to Path_Drone_{id}

                LineRenderer lr = lineObj.GetComponent<LineRenderer>();

                lr.startWidth = lineWidth;
                lr.endWidth = lineWidth;
                lr.positionCount = path.Waypoints.Count;

                // Map waypoints to LineRenderer positions
                for (int i = 0; i < path.Waypoints.Count; i++)
                {
                    lr.SetPosition(i, path.Waypoints[i].Position.ToUnity());
                }

                // Change color to drone's color
                var droneView = DroneManager.instance.GetDroneView(droneId);
                if (droneView != null)
                {
                    Color c = droneView.CurrentColor;
                    c.a = lineAlpha;
                    lr.startColor = c;
                    lr.endColor = c;
                }

                _activeLineObjects.Add(lineObj);
            }
        }

        public void ClearPaths()
        {
            foreach (var obj in _activeLineObjects)
            {
                Destroy(obj);
            }
            _activeLineObjects.Clear();
        }
    }
}