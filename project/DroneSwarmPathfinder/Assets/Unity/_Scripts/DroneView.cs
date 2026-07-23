using UnityEngine;

namespace DroneSwampPathfiner.Unity.Visuals
{
    public class DroneView : MonoBehaviour
    {
        public int DroneID { get; private set; }

        private MeshRenderer _renderer;
        private static MaterialPropertyBlock _propBlock;

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
        }

        // Called shortly after spawning (but not instantly... do not switch to Start() )
        public void Initialize(int id, int groupId)
        {
            DroneID = id;
            SetColorByGroup(groupId);
        }

        private void SetColorByGroup(int groupId)
        {
            // PropertyBlock prevents unity from creating a unique copy of a material for every drone ... should improve performance for large swarms
            if (_propBlock == null)
            {
                _propBlock = new MaterialPropertyBlock();
            }

            // TODO: unhardcode v
            // hardcoded colors by index :)
            Color[] colors = { Color.blue, Color.red, Color.green, Color.yellow, Color.cyan, Color.magenta, Color.white };
            Color assignedColor = colors[Mathf.Abs(groupId) % colors.Length];

            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor("_Color", assignedColor);
            _renderer.SetPropertyBlock(_propBlock);
        }
    }
}