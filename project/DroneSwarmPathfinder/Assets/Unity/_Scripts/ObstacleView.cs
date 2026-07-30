using DroneSwarmPathfinder.Unity.Visuals;
using UnityEngine;

namespace DroneSwarmPathfinder.Unity.Environment
{
    /// <summary>
    /// Visual representation of an obstacle in the unity scene.
    /// </summary>
    public class ObstacleView : MonoBehaviour, ISelectableView
    {
        public int ID { get; private set; }

        public void Initialize(int id)
        {
            ID = id;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawCube(transform.position, transform.localScale);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}