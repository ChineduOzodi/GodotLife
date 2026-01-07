using Godot;
using System;
using WorldGen;

namespace WorldGen
{
    [GlobalClass]
    public partial class WaterTerrainGen : TerrainGen
    {

        [Export(PropertyHint.Range, "-1,1,")]
        public float MaxElevation { get; set; } = 0f;
        public override bool ShouldDrawTerrain(Vector2 position, TerrainLayerSettings terrainLayerSettings)
        {
            FastNoiseLite elevationNoise = terrainLayerSettings.ElevationNoise;

            if (elevationNoise == null )
            {
                return false;
            }

            float elevation = elevationNoise.GetNoise2D(position.X, position.Y);

            return elevation <= MaxElevation;
        }
    }
}

