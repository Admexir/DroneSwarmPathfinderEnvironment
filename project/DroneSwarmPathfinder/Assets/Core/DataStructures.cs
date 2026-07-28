using DroneSwampPathfiner.Core.Environment;
using System;
using System.Collections.Generic;
using System.Numerics;


namespace DroneSwampPathfiner.Core.Models
{
    /// <summary>
    /// Struct representing the position, rotation and size of a drone (agent)
    /// </summary>
    public struct TransformData
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Size { get; set; }

        public TransformData(Vector3 position)
        {
            this = default;
            Position = position;
            Rotation = Quaternion.Identity;
            Size = Vector3.One;
        }
        public TransformData(Vector3 position, Quaternion rotation)
        {
            this = default;
            Position = position;
            Rotation = rotation;
            Size = Vector3.One;
        }
        public TransformData(Vector3 position, Quaternion rotation, Vector3 size)
        {
            this = default;
            Position = position;
            Rotation = rotation;
            Size = size;
        }
    }
    public static class TransformExtensions
    {
        public static TransformData WithPosition(this TransformData t, Vector3 newPosition) => new TransformData(newPosition, t.Rotation, t.Size);
        public static TransformData WithRotation(this TransformData t, Quaternion newRotation) => new TransformData(t.Position, newRotation, t.Size);
        public static TransformData WithSize(this TransformData t, Vector3 newSize) => new TransformData(t.Position, t.Rotation, newSize);

    }
        

    /// <summary>
    /// Class representing one agent in the simulation
    /// </summary>
    public class Drone : IObstacleVolume
    {
        public int ID { get; init; }
        public int GroupId { get; set; }
        public TransformData Transform { get; set; }
        /// <summary>
        /// Takes the largest dimension from Transform and divides it by two
        /// </summary>
        public float CollisionRadius
        {
            get
            {
                float maxDimension = Math.Max(Math.Max(Transform.Size.X, Transform.Size.Y), Transform.Size.Z);
                return maxDimension / 2f;
            }
        }

        public Drone(int id, TransformData transform, int groupId = 0)
        {
            ID = id;
            Transform = transform;
            GroupId = groupId;
        }

        public bool Contains(Vector3 point)
        {
            float currentRadius = CollisionRadius;
            float radiusSquared = currentRadius * currentRadius;

            return Vector3.DistanceSquared(Transform.Position, point) <= radiusSquared; // (squared math to prevent calculating a lot of square roots)
        }
    }

    /// <summary>
    /// Readonly struct representing one point in time on a drones path
    /// </summary>
    public readonly struct Waypoint
    {
        public int StepIndex { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }

        public Waypoint(int stepIndex, Vector3 position, Quaternion rotation)
        {
            StepIndex = stepIndex;
            Position = position;
            Rotation = rotation;
        }
    }

    /// <summary>
    /// Class representing the result of a pathfinding algorithm of ONE drone (agent)
    /// </summary>
    public class DronePath
    {
        public int DroneId { get; init; }
        public List<Waypoint> Waypoints { get; init; } = new();
    }

}


// This polyfill fixes the CS0518 error when using 'init' in Unity
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}