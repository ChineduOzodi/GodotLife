using Godot;
using System;
using WorldGen;

namespace WorldGen
{
    [GlobalClass]
    public partial class GrassTerrainGen : TerrainGen
    {

        [Export(PropertyHint.Range, "-1,1,")]
        public float MinElevation { get; set; } = 0.1f;

        [Export(PropertyHint.Range, "-1,1,")]
        public float MaxElevation { get; set; } = 1f;
        public override bool ShouldDrawTerrain(Vector2 position, TerrainLayerSettings groundLayerSettings)
        {
            FastNoiseLite elevationNoise = groundLayerSettings.ElevationNoise;

            if (elevationNoise == null )
            {
                return false;
            }

            float elevation = elevationNoise.GetNoise2D(position.X, position.Y);

            return elevation <= MaxElevation && elevation >= MinElevation;
        }
    }
}

