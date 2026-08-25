using DroneSwarmPathfinder.Unity.Managers;
using DroneSwarmPathfinder.Unity.Visuals;
using System.Linq;
using UnityEngine;

namespace DroneSwarmPathfinder.Unity.EditorTools
{
    [RequireComponent(typeof(SelectionManager))]
    public class BoxSelectionVisualizer : MonoBehaviour
    {
        [Header("Visuals")]
        public Color selectionBoxColor = new Color(0.2f, 0.6f, 1f, 0.3f);
        public Color selectionBoxBorderColor = new Color(0.2f, 0.6f, 1f, 1f);

        private SelectionManager _selectionManager;
        private Camera _cam;

        public bool IsBoxSelecting { get; private set; }
        private Vector2 _boxStartPos;
        private Vector2 _boxEndPos;

        private static Texture2D _whiteTexture;
        private static Texture2D WhiteTexture
        {
            get
            {
                if (_whiteTexture == null)
                {
                    _whiteTexture = new Texture2D(1, 1);
                    _whiteTexture.SetPixel(0, 0, Color.white);
                    _whiteTexture.Apply();
                }
                return _whiteTexture;
            }
        }

        private void Awake()
        {
            _selectionManager = GetComponent<SelectionManager>();
            _cam = Camera.main;
        }

        public void StartBoxSelection(Vector2 startPos)
        {
            IsBoxSelecting = true;
            _boxStartPos = startPos;
            _boxEndPos = startPos;
        }

        public void UpdateBoxSelection(Vector2 currentPos)
        {
            if (IsBoxSelecting) _boxEndPos = currentPos;
        }

        public void EndBoxSelection()
        {
            if (!IsBoxSelecting) return;
            IsBoxSelecting = false;
            ApplyBoxSelection();
        }

        private void ApplyBoxSelection()
        {
            if (!Input.GetKey(KeyCode.LeftShift)) _selectionManager.ClearSelection();

            Vector2 min = Vector2.Min(_boxStartPos, _boxEndPos);
            Vector2 max = Vector2.Max(_boxStartPos, _boxEndPos);
            Rect selectionRect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);

            if (selectionRect.width < 10 && selectionRect.height < 10) return;

            // Box select drones
            var droneArr = DroneManager.instance.AllDroneModels.ToArray();
            foreach (var coreDrone in droneArr)
            {
                var droneView = DroneManager.instance.GetDroneView(coreDrone.ID);
                if (droneView == null) continue;

                Vector3 screenPos = _cam.WorldToScreenPoint(droneView.transform.position);

                if (screenPos.z > 0 && selectionRect.Contains(new Vector2(screenPos.x, screenPos.y)))
                {
                    _selectionManager.AddToSelection(droneView);
                }
            }

            // Box select obstacles
            var obsArr = EnvironmentManager.instance.AllObstacleModels.ToArray();
            foreach (var coreObs in obsArr)
            {
                var obsView = EnvironmentManager.instance.GetObstacleView(coreObs.ID);
                if (obsView == null) continue;

                Vector3 screenPos = _cam.WorldToScreenPoint(obsView.transform.position);

                if (screenPos.z > 0 && selectionRect.Contains(new Vector2(screenPos.x, screenPos.y)))
                {
                    _selectionManager.AddToSelection(obsView);
                }
            }
        }

        private void OnGUI()
        {
            if (IsBoxSelecting)
            {
                Rect rect = new Rect(_boxStartPos.x, Screen.height - _boxStartPos.y,
                                     _boxEndPos.x - _boxStartPos.x, -(_boxEndPos.y - _boxStartPos.y));

                DrawScreenRect(rect, selectionBoxColor);
                DrawScreenRectBorder(rect, 2, selectionBoxBorderColor);
            }
        }

        private static void DrawScreenRect(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, WhiteTexture);
            GUI.color = Color.white;
        }

        private static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
        {
            DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
            DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
            DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
            DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        }
    }
}