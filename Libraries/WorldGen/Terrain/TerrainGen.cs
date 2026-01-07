using Godot;
using System;

namespace WorldGen
{
    public abstract partial class TerrainGen : Resource
    {
        public abstract bool ShouldDrawTerrain(Vector2 position, TerrainLayerSettings groundLayerSettings);
    }
}

