using DroneSwarmPathfinder.Core.Environment;
using DroneSwarmPathfinder.Core.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DroneSwarmPathfinder.Core.Models
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

        [JsonConstructor]
        public TransformData(Vector3 position, Quaternion rotation, Vector3 size)
        {
            this = default;
            Position = position;
            Rotation = rotation;
            Size = size;
        }
        public TransformData(Vector3 position, Vector3 size)
        {
            this = default;
            Position = position;
            Rotation = Quaternion.Identity;
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
    public class Drone : IObstacleVolume, IDroneConfigItem
    {
        public int ID { get; init; }
        public string GroupName { get; set; }
        public TransformData Transform { get; set; }

        /// <summary>
        /// Takes the largest dimension from Transform and divides it by two
        /// </summary>
        [JsonIgnore] // We don't need to save calculated properties to JSON
        public float CollisionRadius
        {
            get
            {
                float maxDimension = Math.Max(Math.Max(Transform.Size.X, Transform.Size.Y), Transform.Size.Z);
                return maxDimension / 2f;
            }
        }

        [JsonConstructor]
        public Drone(int id, TransformData transform, string groupName = "default")
        {
            ID = id;
            Transform = transform;
            GroupName = groupName;
        }

        public bool Contains(Vector3 point)
        {
            float currentRadius = CollisionRadius;
            float radiusSquared = currentRadius * currentRadius;

            return Vector3.DistanceSquared(Transform.Position, point) <= radiusSquared;
        }
    }

    /// <summary>
    /// Class representing a position for a drone to move to (target position of a drone)
    /// </summary>
    public class DroneTargetPosition : IObstacleVolume, IDroneConfigItem
    {
        public int ID { get; init; }
        public string GroupName { get; set; }
        public TransformData Transform { get; set; }

        /// <summary>
        /// Takes the largest dimension from Transform and divides it by two
        /// </summary>
        [JsonIgnore] // We don't need to save calculated properties to JSON
        public float CollisionRadius
        {
            get
            {
                float maxDimension = Math.Max(Math.Max(Transform.Size.X, Transform.Size.Y), Transform.Size.Z);
                return maxDimension / 2f;
            }
        }

        [JsonConstructor]
        public DroneTargetPosition(int id, TransformData transform, string groupName)
        {
            ID = id;
            Transform = transform;
            GroupName = groupName;
        }

        public bool Contains(Vector3 point)
        {
            float currentRadius = CollisionRadius;
            float radiusSquared = currentRadius * currentRadius;

            return Vector3.DistanceSquared(Transform.Position, point) <= radiusSquared;
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

        [JsonConstructor]
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

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}