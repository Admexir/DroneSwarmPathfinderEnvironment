using System;
using System.Collections.Generic;
using System.Numerics;


namespace DroneSwampPathfiner.Core.Environment
{
    /// <summary>
    /// Interface used for limiting valid drone positions in space
    /// </summary>
    public interface ISpatialEnvironment
    {
        Vector3 ConstrainPosition(Vector3 position);
    }

    /// <summary>
    /// A basic 3D square discrete grid
    /// </summary>
    public class DiscreteGrid : ISpatialEnvironment
    {
        public float CellSize { get; init; }

        public DiscreteGrid(float cellSize = 1f)
        {
            CellSize = cellSize;
        }

        /// <summary>
        /// Snaps a position to the closest spot on the grid
        /// </summary>
        public Vector3 ConstrainPosition(Vector3 position)
        {
            if (CellSize <= 0.001f) return position;

            return new Vector3(
                (float)Math.Round(position.X / CellSize, MidpointRounding.AwayFromZero) * CellSize,
                (float)Math.Round(position.Y / CellSize, MidpointRounding.AwayFromZero) * CellSize,
                (float)Math.Round(position.Z / CellSize, MidpointRounding.AwayFromZero) * CellSize
            );
        }

        /// <summary>
        /// Converts world space coordinates to grid space coordinates (node indices)
        /// </summary>
        public (int X, int Y, int Z) GetNodeIndex(Vector3 position)
        {
            if (CellSize <= 0.001f) return (0, 0, 0); // Invalid/Empty grid check

            return (
                (int)Math.Round(position.X / CellSize, MidpointRounding.AwayFromZero),
                (int)Math.Round(position.Y / CellSize, MidpointRounding.AwayFromZero),
                (int)Math.Round(position.Z / CellSize, MidpointRounding.AwayFromZero)
            );
        }

        /// <summary>
        /// Converts grid-space coordinates (node indices) to world space coordinates
        /// </summary>
        public Vector3 GetWorldPosition(int x, int y, int z)
        {
            return new Vector3(x * CellSize, y * CellSize, z * CellSize);
        }
    }
}

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
        
    }
        

    /// <summary>
    /// Class representing one agent in the simulation
    /// </summary>
    public class Drone
    {
        public int ID { get; init; }
        public int GroupId { get; set; }
        public TransformData Transform { get; set; }

        public Drone(int id, TransformData transform, int groupId = 0)
        {
            ID = id;
            Transform = transform;
            GroupId = groupId;
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