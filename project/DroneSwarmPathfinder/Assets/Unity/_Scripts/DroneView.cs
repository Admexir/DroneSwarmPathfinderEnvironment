using DroneSwarmPathfinder.Unity.Managers;
using UnityEngine;

namespace DroneSwarmPathfinder.Unity.Visuals
{
    /// <summary>
    /// Representation of the dynamic visuals of a drone in a simulation
    /// </summary>
    public class DroneView : MonoBehaviour, ISelectableView
    {
        // Note: doesn't hold a reference to the source Models.Drone object, as that is a different "kind of representation",
        //      this is for the active visualisation, while Models.Drone is for the persistent config storage
        public int ID { get; private set; }
        public string DroneGroup { get; private set; }
        public Color CurrentColor { get; private set; }
        private MeshRenderer _renderer;
        private static MaterialPropertyBlock _propBlock;

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
        }

        // Called shortly after spawning (but not instantly... do not switch to Start() )
        public void Initialize(int id, string groupName, bool isTargetPosition = false)
        {
            ID = id;
            SetColorByGroup(groupName, isTargetPosition);
        }

        private void SetColorByGroup(string groupName, bool isTargetPosition = false)
        {
            DroneGroup = groupName;
            // PropertyBlock prevents unity from creating a unique copy of a material for every drone ... should improve performance for large swarms
            if (_propBlock == null)
            {
                _propBlock = new MaterialPropertyBlock();
            }

            // TODO: unhardcode v
            // hardcoded colors by index :)
            //Color[] colors = { Color.blue, Color.red, Color.green, Color.yellow, Color.cyan, Color.magenta, Color.white };
            //Color assignedColor = colors[Mathf.Abs(groupName) % colors.Length];
            _renderer.GetPropertyBlock(_propBlock);
            if (!DroneManager.instance.GetColorByGroup(groupName, out Color assignedColor)) assignedColor = Color.black;
            if (isTargetPosition)
            {
                assignedColor = new Color(assignedColor.r, assignedColor.g, assignedColor.b, 0.5f);
            }
            CurrentColor = assignedColor;

            _propBlock.SetColor("_Color", assignedColor);
            _renderer.SetPropertyBlock(_propBlock);
        }
    }
}