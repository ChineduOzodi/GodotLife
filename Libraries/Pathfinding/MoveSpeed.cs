using Godot;
using System;
using System.Collections.Generic;

namespace Pathfinding
{
    /// <summary>
    /// Holds the speed of a string position and its global position. Will have to add Neighbors after initialization to use for pathfinding.
    /// </summary>
    /// <param name="name">Name of position, typically x,y coords of a tile but can be anything depending on application.</param>
    /// <param name="baseSpeed">Base move speed.</param>
    /// <param name="globalPosition">Position in game world.</param>
    public class MoveSpeed(string name, float baseSpeed, Vector2 globalPosition)
    {

        public readonly string name = name;
        public float Speed { get => GetSpeed(); }

        public Vector2 GlobalPosition { get; set; } = globalPosition;

        private HashSet<MoveSpeed> _neighbors = [];
        public HashSet<MoveSpeed> Neighbors { get => GetNeighbors(_neighbors); set => _neighbors = value; }

        public readonly Dictionary<string, Func<MoveSpeed, float>> speedMultipliers = [];

        private float GetSpeed()
        {
            float speed = baseSpeed;

            foreach (var kvp in  speedMultipliers)
            {
                speed *= kvp.Value(this);
            }
            return speed;
        }

        private HashSet<MoveSpeed> GetNeighbors(HashSet<MoveSpeed> value)
        {
            if (value.Count == 0)
            {
                throw new Exception($"Movespeed '{name}' neighbors empty");
            }

            return value;
        }
    }
}
