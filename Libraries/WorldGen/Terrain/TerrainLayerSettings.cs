using Godot;
using Godot.Collections;
using System;
using System.Linq;

namespace WorldGen
{
    [GlobalClass]
    public partial class TerrainLayerSettings : Resource
    {
        [Export]
        public FastNoiseLite ElevationNoise { get; set; } = new FastNoiseLite();

        [Export]
        public TileSet TileSet { get; set; } = GD.Load<TileSet>("res://Life/World/Assets/WorldTileSet.tres");

        [Export]
        public Dictionary<string, TerrainGen> TerrainGenMap { get; set; } = new Dictionary<string, TerrainGen> ()
        {
            { "water", new WaterTerrainGen() },
            { "grass", new GrassTerrainGen() },
            { "sand", new SandTerrainGen() },
        };

        private readonly System.Collections.Generic.Dictionary<string, Vector2I> _terrainTileSetLocationMap = [];

        public Vector2I? GetTerrainTileSetLocation(string terrainName)
        {
            if (_terrainTileSetLocationMap.TryGetValue(terrainName, out Vector2I value))
            {
                return value;
            }

            foreach (int k in Enumerable.Range(0, TileSet.GetTerrainSetsCount()))
            {
                foreach (int i in Enumerable.Range(0, TileSet.GetTerrainsCount(k)))
                {
                    if (terrainName == TileSet.GetTerrainName(k, i))
                    {
                        _terrainTileSetLocationMap[terrainName] = new Vector2I(k, i);
                        return _terrainTileSetLocationMap[terrainName];
                    }
                }
            }
            GD.Print($"WARNING: Could not find TileSet location indexes for '{terrainName}'");
            return null;
        }
    }
}
