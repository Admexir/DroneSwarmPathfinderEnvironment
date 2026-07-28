using UnityEngine;
using DroneSwampPathfiner.Core.Environment;

namespace DroneSwampPathfiner.Unity.Environment
{
    /// <summary>
    /// Visual representation of an obstacle in the unity scene
    /// </summary>
    public class ObstacleView : MonoBehaviour
    {
        public int obstacleID;
        public BoxObstacle Model { get; private set; }

        public void Initialize(BoxObstacle coreModel)
        {
            obstacleID = coreModel.ID;
            Model = coreModel;
        }

        private void OnDrawGizmos()
        {
            if (Model == null) return;

            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawCube(transform.position, transform.localScale);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}