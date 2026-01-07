using Godot;
using System;

namespace WorldGen
{
    [GlobalClass]
    public partial class WorldSettings : Resource
    {
        [Export]
        public int Width { get; set; } = 250;

        [Export]
        public int Height { get; set; } = 250;

        [Export]
        public TerrainLayerSettings TerrainLayerSettings { get; set; } = new TerrainLayerSettings();

        [Export]
        public GroundLayerSettings GroundLayerSettings { get; set; } = new GroundLayerSettings();

        public int HalfWidth
        {
            get => Width / 2;
        }
        public int HalfHeight {
            get => Height / 2;
        }


        public Vector2I WorldGridToPosition(int x, int y)
        {
            return new Vector2I(x-HalfWidth, y-HalfHeight);
        }
    }
}

