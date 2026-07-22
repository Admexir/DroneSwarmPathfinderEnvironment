using System;
//using UnityEngine;

namespace DroneSwampPathfined
{
    /// <summary>
    /// An ordered triplet of doubles
    /// </summary>
    public class Vector3
    {
        public double x; public double y; public double z;
        public Vector3(double x = 0, double y = 0, double z = 0) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 Zero => new();
        public static Vector3 Up => new(0,1);
        public static Vector3 Right => new(1);
        public static Vector3 Down => new(0, -1);
        public static Vector3 Left => new(-1);
        public static Vector3 Forward => new(0, 0, 1);
        public static Vector3 Back => new(0, 0, -1);
        public static Vector3 One => new(1, 1, 1);


        public double Magnitude => Math.Sqrt(x * x + y * y + z * z);
        public static Vector3 operator +(Vector3 left, Vector3 right) => new(left.x + right.x, left.y + right.y, left.z + right.z);
        public static Vector3 operator -(Vector3 left, Vector3 right) => new(left.x - right.x, left.y - right.y, left.z - right.z);
        public static Vector3 operator *(Vector3 left, double right) => new(left.x * right, left.y * right, left.z * right);
        public static Vector3 operator ^(Vector3 left, double right) => new(Math.Pow(left.x, right), Math.Pow(left.y, right), Math.Pow(left.z, right));
        public static Vector3 operator -(Vector3 left) => Vector3.Zero-left;
        public static bool operator ==(Vector3 a, Vector3 b)
        {
            // Check for null before accessing properties
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.x == b.x && a.y == b.y && a.z == b.z;
        }
        public static bool operator !=(Vector3 a, Vector3 b)
        {
            // Check for null before accessing properties
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return false;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return true;

            return a.x != b.x || a.y != b.y || a.z != b.z;
        }
        public override bool Equals(object obj)
        {
            if (obj is Vector3 other)
            {
                return this == other;
            }
            return false;
        }

        public static implicit operator Vector2(Vector3 right) => new(right.x, right.y);

        /// <summary>
        /// Helper function to rotate a 3D vector by given euler angles (in degrees)
        /// </summary>
        /// <param name="vector">The vector to rotate</param>
        /// <param name="eulerAngles">The rotation angles in degrees (X, Y, Z)</param>
        /// <returns>The rotated Vector3</returns>
        public static Vector3 Rotate(Vector3 vector, Vector3 eulerAngles)
        {
            double radX = eulerAngles.x * Math.PI / 180d;
            double radY = eulerAngles.y * Math.PI / 180d;
            double radZ = eulerAngles.z * Math.PI / 180d;

            double cx = Math.Cos(radX), sx = Math.Sin(radX);
            double cy = Math.Cos(radY), sy = Math.Sin(radY);
            double cz = Math.Cos(radZ), sz = Math.Sin(radZ);

            double x0 = vector.x;
            double y0 = vector.y;
            double z0 = vector.z;
            // x-axis
            double x1 = x0;
            double y1 = y0 * cx - z0 * sx;
            double z1 = y0 * sx + z0 * cx;
            // y-axis
            double x2 = x1 * cy + z1 * sy;
            double y2 = y1;
            double z2 = -x1 * sy + z1 * cy;
            // z-axis
            double x3 = x2 * cz - y2 * sz;
            double y3 = x2 * sz + y2 * cz;
            double z3 = z2;

            return new Vector3(x3, y3, z3);
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ", " + z + ")";
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z);
        }
    }


    /// <summary>
    /// An ordered pair of doubles
    /// </summary>
    public class Vector2
    {
        public double x { get; set; }
        public double y { get; set; }
        /// <summary>
        /// (0, 0)
        /// </summary>
        public static Vector2 zero { get => new Vector2(0, 0); }
        /// <summary>
        /// (1, 1)
        /// </summary>
        public static Vector2 one { get => new Vector2(1, 1); }
        /// <summary>
        /// (0, 1)
        /// </summary>
        public static Vector2 up { get => new Vector2(0, 1); }
        /// <summary>
        /// (1, 0)
        /// </summary>
        public static Vector2 right { get => new Vector2(1, 0); }
        /// <summary>
        /// (0, -1)
        /// </summary>
        public static Vector2 down { get => new Vector2(0, -1); }
        /// <summary>
        /// (-1, 0)
        /// </summary>
        public static Vector2 left { get => new Vector2(-1, 0); }
        /// <summary>
        /// Length of the vector as a double
        /// </summary>
        public double magnitude { get => Math.Sqrt(x * x + y * y); }
        public Vector2 normalized { get => magnitude == 0 ? Vector2.zero : new Vector2(x / magnitude, y / magnitude); }

        /// <summary>
        /// helper function to rotate a vector the the given rotation (positive to the right)
        /// </summary>
        /// <param name="vector">the vector to rotate</param>
        /// <param name="angle">the angle in degrees</param>
        /// <returns>the rotated vector</returns>
        public static Vector2 Rotate(Vector2 vector, double angle)
        {
            double radAngle = angle / 180 * Math.PI;
            double cos = Math.Cos(radAngle);
            double sin = Math.Sin(radAngle);
            return new Vector2(
                vector.x * cos - vector.y * sin,
                vector.x * sin + vector.y * cos
            );
        }
        public Vector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }
        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }
        public static Vector2 operator -(Vector2 a)
        {
            return new Vector2(-a.x, -a.y);
        }
        public static Vector2 operator *(Vector2 a, double b)
        {
            return new Vector2(a.x * b, a.y * b);
        }
        public static Vector2 operator /(Vector2 a, double b)
        {
            return new Vector2(a.x / b, a.y / b);
        }
        public static Vector2 operator ^(Vector2 a, double b)
        {
            return new Vector2((double)Math.Pow(a.x, b), (double)Math.Pow(a.y, b));
        }
        public static bool operator ==(Vector2 a, Vector2 b)
        {
            // Check for null before accessing properties
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(Vector2 a, Vector2 b)
        {
            // Check for null before accessing properties
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return false;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return true;

            return a.x != b.x || a.y != b.y;
        }
        public override bool Equals(object obj)
        {
            if (obj is Vector2 other)
            {
                return this == other;
            }
            return false;
        }

        public static implicit operator Vector3(Vector2 right) => new(right.x, right.y, 0);
        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }
    }

}
