using UnityEngine;

namespace DroneSwarmPathfinder.Unity.Environment
{
    /// <summary>
    /// Visual representation of an obstacle in the unity scene.
    /// </summary>
    public class ObstacleView : MonoBehaviour
    {
        public int ObstacleID { get; private set; } // (only ID just like DroneView)

        public void Initialize(int id)
        {
            ObstacleID = id;
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