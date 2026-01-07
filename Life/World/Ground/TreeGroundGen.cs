using Godot;
using System;

namespace WorldGen
{
    [GlobalClass]
    public partial class TreeGroundGen : GroundGen
    {
        [Export(PropertyHint.Range, "-1,1,")]
        public float MinElevation { get; set; } = 0.1f;

        [Export(PropertyHint.Range, "-1,1,")]
        public float MaxElevation { get; set; } = 1.0f;

        protected override bool ShouldDrawObject(Vector2 position, string terrainName, TerrainLayerSettings groundLayerSettings)
        {
            FastNoiseLite elevationNoise = groundLayerSettings.ElevationNoise;

            if (elevationNoise == null)
            {
                return false;
            }

            float elevation = elevationNoise.GetNoise2D(position.X, position.Y);

            return elevation <= MaxElevation && elevation >= MinElevation;
        }
    }
}
