using DroneSwampPathfiner.Unity.EditorTools;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DroneSwampPathfiner.Unity.Visuals
{
    /// <summary>
    /// Uses the GL library (-> graphics card) to render the transparent grid around selected drones
    /// </summary>
    public class GridVisualizer : MonoBehaviour
    {
        [Header("Grid Visuals")]
        [Tooltip("Color")]
        public Color gridColor = new Color(0.2f, 0.6f, 1f, 0.4f);

        [Tooltip("Distance around drone, from which the grid is shown")]
        public float fadeDistance = 5f;

        public Material gridMaterial;

        // Dict of distance -> opacity
        private Dictionary<Vector3Int, float> _nodeAlphas = new Dictionary<Vector3Int, float>();

        #region SUPPORT FOR URP
        // private void OnEnable() { RenderPipelineManager.endCameraRendering += OnEndCameraRendering; }
        // private void OnDisable() { RenderPipelineManager.endCameraRendering -= OnEndCameraRendering; }
        // private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera) { if (camera == Camera.main) DrawGrid(); }
        #endregion

        #region SUPPORT FOR DEFAULT RENDER PIPELINE
        private void OnRenderObject()
        {
            if (GraphicsSettings.defaultRenderPipeline == null) DrawGrid();
        }
        #endregion

        /// <summary>
        /// Draws a half-transparent grid around the currently selected drones
        /// </summary>
        private void DrawGrid()
        {
            // End if no drones are selected
            if (ConfigEditorManager.instance == null || ConfigEditorManager.instance.SelectedDrones.Count == 0) return;

            float gridSize = ConfigEditorManager.instance.gridSize;
            if (gridSize <= 0.01f) return;

            CalculateNodeAlphas(gridSize);

            Material matToUse = gridMaterial;
            if (matToUse == null)
            {
                Debug.LogError($"ERROR: No grid material assigned, assign it at the object holding GridVisualizer.cs!");
                return;
            }

            matToUse.SetPass(0);

            GL.PushMatrix();
            GL.Begin(GL.LINES);

            // Draw the grid in parts according to the distance from the drone (dict _nodeAlphas)
            foreach (var kvp in _nodeAlphas)
            {
                Vector3Int gridPos = kvp.Key;
                float currentAlpha = kvp.Value;
                Vector3 worldPos = new Vector3(gridPos.x * gridSize, gridPos.y * gridSize, gridPos.z * gridSize);

                DrawLineSegment(worldPos, gridPos, gridPos + Vector3Int.right, currentAlpha, gridSize);
                DrawLineSegment(worldPos, gridPos, gridPos + Vector3Int.up, currentAlpha, gridSize);
                DrawLineSegment(worldPos, gridPos, gridPos + new Vector3Int(0, 0, 1), currentAlpha, gridSize);
            }

            GL.End();
            GL.PopMatrix();
        }

        private void DrawLineSegment(Vector3 startWorldPos, Vector3Int startGridPos, Vector3Int endGridPos, float startAlpha, float gridSize)
        {
            float endAlpha = 0f;
            _nodeAlphas.TryGetValue(endGridPos, out endAlpha);

            // Skip nearly invisible lines
            if (startAlpha <= 0.02f && endAlpha <= 0.02f) return;

            Vector3 endWorldPos = new Vector3(endGridPos.x * gridSize, endGridPos.y * gridSize, endGridPos.z * gridSize);

            Color colorStart = gridColor; colorStart.a = startAlpha * gridColor.a;
            Color colorEnd = gridColor; colorEnd.a = endAlpha * gridColor.a;

            GL.Color(colorStart);
            GL.Vertex(startWorldPos);

            GL.Color(colorEnd);
            GL.Vertex(endWorldPos);
        }

        /// <summary>
        /// Calculates how opaque (how high alpha) a line should have at certain coordinates, depending on its distance from the selected drones
        /// </summary>
        /// <param name="gridSize"></param>
        private void CalculateNodeAlphas(float gridSize)
        {
            _nodeAlphas.Clear();
            int searchRadius = Mathf.CeilToInt(fadeDistance / gridSize);

            foreach (var selectedDrone in ConfigEditorManager.instance.SelectedDrones)
            {
                Vector3 centerPos = selectedDrone.transform.position;

                Vector3Int centerNode = new Vector3Int(
                    Mathf.RoundToInt(centerPos.x / gridSize),
                    Mathf.RoundToInt(centerPos.y / gridSize),
                    Mathf.RoundToInt(centerPos.z / gridSize)
                );

                // Go through the grid nodes around the drone
                for (int x = -searchRadius; x <= searchRadius; x++)
                {
                    for (int y = -searchRadius; y <= searchRadius; y++)
                    {
                        for (int z = -searchRadius; z <= searchRadius; z++)
                        {
                            Vector3Int nodeGridPos = centerNode + new Vector3Int(x, y, z);
                            Vector3 nodeWorldPos = new Vector3(nodeGridPos.x * gridSize, nodeGridPos.y * gridSize, nodeGridPos.z * gridSize);

                            float dist = Vector3.Distance(centerPos, nodeWorldPos);

                            if (dist <= fadeDistance)
                            {
                                // linear alpha falloff
                                float alpha = 1f - (dist / fadeDistance);
                                // +smoothstep smoothing for nicer edges 
                                alpha = Mathf.SmoothStep(0f, 1f, alpha);

                                // If two drones' grids collide, take higher alpha value
                                if (_nodeAlphas.TryGetValue(nodeGridPos, out float existingAlpha))
                                {
                                    if (alpha > existingAlpha) _nodeAlphas[nodeGridPos] = alpha;
                                }
                                else
                                {
                                    _nodeAlphas[nodeGridPos] = alpha;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}