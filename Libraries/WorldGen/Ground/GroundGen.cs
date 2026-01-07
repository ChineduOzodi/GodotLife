using Godot;
using System;
using GameGlobal;

namespace WorldGen
{
    [GlobalClass]
    public abstract partial class GroundGen : Resource
    {
        [Export(PropertyHint.Range, "0,1,")]
        float SpawnChance { get; set; } = 0.1f;

        public bool ShouldSpawnObject(Vector2 position, string terrainName, TerrainLayerSettings groundLayerSettings)
        {
            if (!ShouldSpawn())
            {
                return false;
            }

            return ShouldDrawObject(position, terrainName, groundLayerSettings);
        }

        protected abstract bool ShouldDrawObject(Vector2 position, string terrainName, TerrainLayerSettings groundLayerSettings);

        private bool ShouldSpawn()
        {
            return Global.RNG.Randf() <= SpawnChance;
        }
    }
}

