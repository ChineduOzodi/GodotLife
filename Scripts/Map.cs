using Godot;
using System;

public class Map : Node2D
{
    private World world;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        world = GetParent<World>();
    }

    public override void _Draw()
    {
        base._Draw();
        Console.WriteLine("Updating map");

        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                Tile tile = world.tiles[x][y];
                switch (world.MapDisplayMode)
                {
                    case DisplayMode.Normal:
                        DisplayNormal(tile);
                        break;
                    case DisplayMode.Height:
                        DisplayHeight(tile);
                        break;
                }
            }
        }

    }

    public void GenerateMap()
    {
        Update();
    }

    private void DisplayNormal(Tile tile)
    {
        Color color;

        switch (tile.biome)
        {
            case TileType.Grassland:
                color = new Color(0.2f, 0.5f, 0.2f, 1).LinearInterpolate(new Color(0, 0.1f, 0), (tile.elevAboveSeaLevel) / (1 - world.WaterLevel));

                if (tile.speedMod > tile.baseSpeedMod)
                {
                    tile.speedMod -= (float)(world.Time - tile.lastUpdated) * tile.recoveryRate;
                    if (tile.speedMod < tile.baseSpeedMod)
                        tile.speedMod = tile.baseSpeedMod;
                    float mod = (tile.speedMod - tile.baseSpeedMod) / (tile.maxSpeedMod - tile.baseSpeedMod);
                    color = color.LinearInterpolate(Color.ColorN("brown", 0.5f), mod);
                    tile.lastUpdated = world.Time;

                }

                break;
            case TileType.Water:
                float deepestWaterElev = 0.3f;
                Color deepestWaterColor = new Color(0.1f, 0.1f, 0.4f); ;
                if (tile.elev < deepestWaterElev)
                {
                    color = deepestWaterColor;
                }
                else
                {
                    color = new Color(0.2f, 0.2f, .6f).LinearInterpolate(deepestWaterColor, (-tile.elevAboveSeaLevel) / (world.WaterLevel - deepestWaterElev));
                }
                break;
            default:
                color = Color.ColorN("pink");
                break;
        }

        Vector2 rectPosition = new Vector2(tile.position.x - world.TileSize / 2, tile.position.y - world.TileSize / 2);
        DrawRect(new Rect2(rectPosition, world.TileSize, world.TileSize),
            color);
    }

    private void DisplayHeight(Tile tile)
    {
        Color color = Color.ColorN("black");

        switch (tile.biome)
        {
            case TileType.Grassland:
                color = color.LinearInterpolate(new Color(1, 1f, 1), (tile.elevAboveSeaLevel) / (1 - world.WaterLevel));
                break;
            case TileType.Water:
                break;
            default:
                color = Color.ColorN("pink");
                break;
        }

        Vector2 rectPosition = new Vector2(tile.position.x - world.TileSize / 2, tile.position.y - world.TileSize / 2);
        DrawRect(new Rect2(rectPosition, world.TileSize, world.TileSize),
            color);
    }
}

public enum DisplayMode
{
    Normal,
    Height
}
