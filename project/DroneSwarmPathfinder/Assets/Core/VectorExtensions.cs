using UnityVector3 = UnityEngine.Vector3;
using UnityQuaternion = UnityEngine.Quaternion;
using NumVector3 = System.Numerics.Vector3;
using NumQuaternion = System.Numerics.Quaternion;

namespace DroneSwampPathfiner.Unity
{
    public static class VectorExtensions
    {
        // Convert numerics vectors to unity vectors
        public static UnityVector3 ToUnity(this NumVector3 v) => new UnityVector3(v.X, v.Y, v.Z);
        public static UnityQuaternion ToUnity(this NumQuaternion q) => new UnityQuaternion(q.X, q.Y, q.Z, q.W);

        // Other way around
        public static NumVector3 ToNumerics(this UnityVector3 v) => new NumVector3(v.x, v.y, v.z);
        public static NumQuaternion ToNumerics(this UnityQuaternion q) => new NumQuaternion(q.x, q.y, q.z, q.w);
    }
}