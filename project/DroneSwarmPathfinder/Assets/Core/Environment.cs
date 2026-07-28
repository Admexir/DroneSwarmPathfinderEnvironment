using DroneSwampPathfinder.Core.Models;
using System;
using System.Collections.Generic;
using System.Numerics;


namespace DroneSwampPathfinder.Core.Environment
{
    public class WorldEnvironment
    {
        public List<IObstacleVolume> Obstacles { get; } = new(); // TODO: maybe one generalized ISpatialVolume list and add an void Apply method?
        // Later can use something like List<IForceVolume> ForceFields { get; } = new();

        /// <summary>
        /// Gets if the point in space is empty
        /// </summary>
        public bool IsWalkable(Vector3 position)
        {
            foreach (var obstacle in Obstacles)
            {
                if (obstacle.Contains(position)) return false;
            }
            return true;
        }
    }


    /// <summary>
    /// Interface for any object taking up any space in the 3D simulation
    /// </summary>
    public interface ISpatialVolume
    {
        /// <summary>
        /// Determines whether the given point is inside this volume
        /// </summary>
        bool Contains(Vector3 point);
    }

    /// <summary>
    /// Interface for any non-passable-through obstacles in the 3D simulation
    /// </summary>
    public interface IObstacleVolume : ISpatialVolume
    {

    }

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

    /// <summary>
    /// Class representing an obstacle in the 3D scene
    /// </summary>
    public class BoxObstacle : IObstacleVolume
    {
        public int ID { get; }
        public TransformData Transform { get; set; }

        public BoxObstacle(int id, TransformData transform)
        {
            ID = id;
            Transform = transform;
        }

        /// <summary>
        /// normal box collisions
        /// (Returns whether the point is inside the obstacle)
        /// </summary>
        public bool Contains(Vector3 point)
        {
            Vector3 halfSize = Transform.Size / 2f;
            return Math.Abs(point.X - Transform.Position.X) <= halfSize.X &&
                   Math.Abs(point.Y - Transform.Position.Y) <= halfSize.Y &&
                   Math.Abs(point.Z - Transform.Position.Z) <= halfSize.Z;
        }
    }
}