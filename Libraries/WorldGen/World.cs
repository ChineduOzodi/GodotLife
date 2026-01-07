using Godot;
using Godot.Collections;
using System;

namespace WorldGen
{
    public partial class World : Node2D
    {
        [Export]
        public WorldSettings Settings { get; set; }

        [Export]
        public TileMapLayer TerrainLayer { get; set; } = new TileMapLayer();

        [Export]
        public TileMapLayer GroundLayer { get; set; } = new TileMapLayer();

        public System.Collections.Generic.Dictionary<Vector2I, GroundObject> GroundLayerObjects { get; set; } = [];

        public override void _Ready()
        {
            if (TerrainLayer.GetParent() != this) 
            { 
                AddChild(TerrainLayer);
            }
            AddChild(GroundLayer);
            DrawMap();

            new WorldPathfinding().SetupWorldPathfinding(this);
        }

        public void DrawMap()
        {
            DrawTerrainLayer();
            DrawGroundLayer();
        }

        private void DrawTerrainLayer()
        {
            TerrainLayer.TileSet = Settings.TerrainLayerSettings.TileSet;

            Dictionary<string, Array<Vector2I>> cellsMap = [];

            void GetCellMapping(Vector2I coords)
            {
                foreach ((string terrainName, TerrainGen terrainGen) in Settings.TerrainLayerSettings.TerrainGenMap)
                {
                    if (terrainGen.ShouldDrawTerrain(coords, Settings.TerrainLayerSettings))
                    {
                        if (cellsMap.TryGetValue(terrainName, out Array<Vector2I> value))
                        {
                            value.Add(coords);
                        }
                        else
                        {
                            cellsMap[terrainName] = [coords];
                        }
                    }
                }
            }

            IterateThroughTilesAndCall(GetCellMapping);

            foreach ((string terrainName, Array<Vector2I> cells) in cellsMap)
            {
                Vector2I? terrainLocation = Settings.TerrainLayerSettings.GetTerrainTileSetLocation(terrainName);

                if (terrainLocation is null)
                {
                    return;
                }
                Vector2I t = (Vector2I)terrainLocation;

                TerrainLayer.SetCellsTerrainConnect(cells, t.X, t.Y);
            }
        }

        private void DrawGroundLayer()
        {
            GroundLayer.TileSet = Settings.GroundLayerSettings.TileSet;

            void DrawPattern(Vector2I coords)
            {
                foreach((string objectName, GroundObjectSettings settings) in Settings.GroundLayerSettings.GroundObjectGen)
                {
                    if (settings.Generator.ShouldSpawnObject(coords, GetTerrainName(coords), Settings.TerrainLayerSettings))
                    {
                        if (settings.PackedScene is not null)
                        {
                            InstantiateObject(coords, settings.PackedScene);
                        } else
                        {
                            SetPattern(coords, settings);
                        }
                    }
                }
            }

            IterateThroughTilesAndCall(DrawPattern);
        }

        private void SetPattern(Vector2I coords, GroundObjectSettings settings)
        {
            TileMapPattern pattern = Settings.GroundLayerSettings.TileSet.GetPattern(settings.PatternIndex);
            GroundLayer.SetPattern(coords, pattern);
        }

        private void InstantiateObject(Vector2I coords, PackedScene packedScene)
        {
            Vector2 position = GroundLayer.MapToLocal(coords);
            GroundObject instance = packedScene.Instantiate<GroundObject>();
            GroundLayer.AddChild(instance);
            instance.Position = position;
            instance.OnWorldSpawn(this);
            GroundLayerObjects[coords] = instance;
        }

        public void IterateThroughTilesAndCall(Action<Vector2I> action)
        {
            for (int x = -Settings.HalfWidth; x <= Settings.HalfWidth; x++)
            {
                for (int y = -Settings.HalfHeight; y <= Settings.HalfHeight; y++)
                {
                    Vector2I coords = new(x, y);
                    action(coords);
                }
            }
        }

        private string GetTerrainName(Vector2I coords) {
            TileData tileData = TerrainLayer.GetCellTileData(coords);
            return TerrainLayer.TileSet.GetTerrainName(tileData.TerrainSet, tileData.Terrain);
        }
    }
}

