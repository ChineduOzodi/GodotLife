using Godot;
using System;

namespace WorldGen
{
    public abstract partial class GroundObject : Node2D
    {
        public abstract void OnWorldSpawn(World world);
    }
}
