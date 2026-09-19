using UnityVector3 = UnityEngine.Vector3;
using UnityQuaternion = UnityEngine.Quaternion;
using NumVector3 = System.Numerics.Vector3;
using NumQuaternion = System.Numerics.Quaternion;
using UnityColor = UnityEngine.Color;

namespace DroneSwarmPathfinder.Unity
{
    public static class CoreUnityConversionExtensions
    {
        // Convert numerics vectors to unity vectors
        public static UnityVector3 ToUnity(this NumVector3 v) => new UnityVector3(v.X, v.Y, v.Z);
        public static UnityQuaternion ToUnity(this NumQuaternion q) => new UnityQuaternion(q.X, q.Y, q.Z, q.W);
        public static UnityColor ToUnity(this System.Drawing.Color c) => new UnityColor(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);

        // Other way around
        public static NumVector3 ToNumerics(this UnityVector3 v) => new NumVector3(v.x, v.y, v.z);
        public static NumQuaternion ToNumerics(this UnityQuaternion q) => new NumQuaternion(q.x, q.y, q.z, q.w);
        public static System.Drawing.Color ToSystemDrawing(this UnityColor c) => System.Drawing.Color.FromArgb(UnityEngine.Mathf.RoundToInt(c.a * 255), UnityEngine.Mathf.RoundToInt(c.r * 255), UnityEngine.Mathf.RoundToInt(c.g * 255), UnityEngine.Mathf.RoundToInt(c.b * 255));
    }
}