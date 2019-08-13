using Godot;
using Life.Scripts.Classes;
using System;

public class Map : Node2D
{
    private World world;
    private float riverAlphaScale = 0.1f;
    private float riverWidthScale = .5f;
    private float waterBasinScale = 0.5f;
    private bool riverAlpha = true;
    MapResource mapResource;
    float resourceScale = 10;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        world = GetParent<World>();
    }

    public override void _Draw()
    {
        base._Draw();
        //Console.WriteLine("Updating map");

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
                    case DisplayMode.MoistureNoise:
                        DisplayMoistureNoise(tile);
                        break;
                    case DisplayMode.LatTemperature:
                        DisplayLatTemperature(tile);
                        break;
                    case DisplayMode.CellMoisture:
                        DisplayCellMoisture(tile);
                        break;
                    case DisplayMode.Moisture:
                        DisplayMoisture(tile);
                        break;
                    case DisplayMode.WalkingSpeed:
                        DisplayWalkingSpeed(tile);
                        break;
                    case DisplayMode.Resource:
                        DisplayResource(tile);
                        break;
                }
            }
        }

        //if (world.MapDisplayMode == DisplayMode.Normal)
        //{
        //    DrawRivers();
        //}

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
            case TileType.land:
                color = new Color(213f/255, 204f/255,127f/255).LinearInterpolate(new Color(57f/255, 118f/255, 40f/255), tile.moisture);
                color = color.LinearInterpolate(Color.ColorN("black"), (tile.elev) * .9f);

                //add rock effect
                if (tile.HasMapResource(MapResourceName.Rock))
                {
                    MapResourceData mapResourceData = tile.GetMapResourceData(MapResourceName.Rock);
                    color = color.LinearInterpolate(new Color(98f/255, 98f/255,98f/255), mapResourceData.amount / mapResourceData.mapResource.resourceMax * 0.75f);
                }
                color = color.LinearInterpolate(Color.ColorN("black"), (tile.elev) * .9f);

                if (tile.speedMod > tile.baseSpeedMod)
                {
                    tile.speedMod -= (float)(world.Time - tile.lastUpdated) * tile.recoveryRate;
                    if (tile.speedMod < tile.baseSpeedMod)
                        tile.speedMod = tile.baseSpeedMod;
                    float mod = (tile.speedMod - tile.baseSpeedMod) / (tile.maxSpeedMod - tile.baseSpeedMod);
                    color = color.LinearInterpolate(new Color(106f/255,92f/255,82f/255), mod);
                    tile.lastUpdated = world.Time;

                }

                break;
            case TileType.Water:
                float deepestWaterElev = -0.1f;
                Color deepestWaterColor = new Color(0.1f, 0.1f, 0.4f); ;
                if (tile.elev < deepestWaterElev)
                {
                    color = deepestWaterColor;
                }
                else
                {
                    color = new Color(0.2f, 0.2f, .6f).LinearInterpolate(deepestWaterColor, (tile.elev) / (deepestWaterElev));
                }
                break;
            case TileType.snow:
                color = new Color(1f, .9f, .9f);
                color = color.LinearInterpolate(Color.ColorN("black"), (1 - (tile.elev)) * 0.5f);

                //add path effect
                if (tile.speedMod > tile.baseSpeedMod)
                {
                    tile.speedMod -= (float)(world.Time - tile.lastUpdated) * tile.recoveryRate;
                    if (tile.speedMod < tile.baseSpeedMod)
                        tile.speedMod = tile.baseSpeedMod;
                    float mod = (tile.speedMod - tile.baseSpeedMod) / (tile.maxSpeedMod - tile.baseSpeedMod);
                    color = color.LinearInterpolate(new Color(106f / 255, 92f / 255, 82f / 255), mod);
                    tile.lastUpdated = world.Time;

                }
                //add rock effect
                if (tile.HasMapResource(MapResourceName.Rock))
                {
                    MapResourceData mapResourceData = tile.GetMapResourceData(MapResourceName.Rock);
                    color = color.LinearInterpolate(new Color(98f / 255, 98f / 255, 98f / 255), mapResourceData.amount / mapResourceData.mapResource.resourceMax * 0.75f);
                }
                break;
            case TileType.SeaIce:
                color = new Color(.77f, .8f, .85f);
                break;
            case TileType.Glacier:
                color = new Color(.77f, .8f, .85f);
                color = color.LinearInterpolate(Color.ColorN("black"), (1 - (tile.elev)) * 0.05f);

                //add path effect
                if (tile.speedMod > tile.baseSpeedMod)
                {
                    tile.speedMod -= (float)(world.Time - tile.lastUpdated) * tile.recoveryRate;
                    if (tile.speedMod < tile.baseSpeedMod)
                        tile.speedMod = tile.baseSpeedMod;
                    float mod = (tile.speedMod - tile.baseSpeedMod) / (tile.maxSpeedMod - tile.baseSpeedMod);
                    color = color.LinearInterpolate(new Color(106f / 255, 92f / 255, 82f / 255), mod);
                    tile.lastUpdated = world.Time;

                }

                //add rock effect
                if (tile.HasMapResource(MapResourceName.Rock))
                {
                    MapResourceData mapResourceData = tile.GetMapResourceData(MapResourceName.Rock);
                    color = color.LinearInterpolate(new Color(98f / 255, 98f / 255, 98f / 255), mapResourceData.amount / mapResourceData.mapResource.resourceMax * 0.75f);
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
            case TileType.Water:
                break;
            default:
                color = color.LinearInterpolate(new Color(1, 1f, 1), tile.elev);
                break;
        }

        Vector2 rectPosition = new Vector2(tile.position.x - world.TileSize / 2, tile.position.y - world.TileSize / 2);
        DrawRect(new Rect2(rectPosition, world.TileSize, world.TileSize),
            color);
    }

    private void DisplayResource(Tile tile)
    {
        Color color = Color.ColorN("black");

        if (mapResource != null)
        {
            if (tile.HasMapResource(mapResource.Name))
            {
                MapResourceData mapResourceData = tile.GetMapResourceData(mapResource.Name);
                color = color.LinearInterpolate(new Color(1, 1f, 1), mapResourceData.amount / mapResource.resourceMax);
            }
        }

        Vector2 rectPosition = new Vector2(tile.position.x - world.TileSize / 2, tile.position.y - world.TileSize / 2);
        DrawRect(new Rect2(rectPosition, world.TileSize, world.TileSize),
            color);

        
    }

    private void DisplayMoistureNoise(Tile tile)
    {
        Color color = Color.ColorN("black");
        color = color.LinearInterpolate(Color.ColorN("blue"), tile.moistureNoise);

        Vector2 rectPosition = new Vector2(tile.position.x - world.TileSize / 2, tile.position.y - world.TileSize / 2);
        DrawRect(new Rect2(rectPosition, world.TileSize, world.TileSize),
            color);
    }

    private void DisplayLatTemperature(Tile tile)
    {
        Color color = Color.ColorN("blue");
        color = color.LinearInterpolate(Color.ColorN("red"), tile.latTemperature);

        Vector2 rectPosition = new Vector2(tile.position.x - world.TileSize / 2, tile.position.y - world.TileSize / 2);
        DrawRect(new Rect2(rectPosition, world.TileSize, world.TileSize),
            color);
    }

    private void DisplayCellMoisture(Tile tile)
    {
        Color color = Color.ColorN("black");
        color = color.LinearInterpolate(Color.ColorN("blue"), tile.cellMoisture);

        Vector2 rectPosition = new Vector2(tile.position.x - world.TileSize / 2, tile.position.y - world.TileSize / 2);
        DrawRect(new Rect2(rectPosition, world.TileSize, world.TileSize),
            color);
    }
    private void DisplayMoisture(Tile tile)
    {
        Color color = Color.ColorN("black");
        color = color.LinearInterpolate(Color.ColorN("blue"), tile.moisture);

        Vector2 rectPosition = new Vector2(tile.position.x - world.TileSize / 2, tile.position.y - world.TileSize / 2);
        DrawRect(new Rect2(rectPosition, world.TileSize, world.TileSize),
            color);
    }
    private void DisplayWalkingSpeed(Tile tile)
    {
        Color color = Color.ColorN("red");
        color = color.LinearInterpolate(Color.ColorN("green"), tile.speedMod * tile.riverCrossingSpeed);

        Vector2 rectPosition = new Vector2(tile.position.x - world.TileSize / 2, tile.position.y - world.TileSize / 2);
        DrawRect(new Rect2(rectPosition, world.TileSize, world.TileSize),
            color);
    }

    private void DrawRivers()
    {
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                Tile tile = world.tiles[x][y];

                if (tile.hasWaterBasin)
                {
                    DrawCircle(tile.position, tile.waterBasinSize * waterBasinScale, new Color(0.2f, 0.2f, .6f));
                }
                else if (tile.hasRiver)
                {
                    RiverData river = world.rivers[$"{tile.position.ToString()}"];
                    if (riverAlpha)
                    {
                        DrawLine(river.position, river.toPosition, new Color(0.2f, 0.2f, .6f, river.size * riverAlphaScale), river.size * riverWidthScale);
                    }
                    else
                    {
                        DrawLine(river.position, river.toPosition, new Color(0.2f, 0.2f, .6f, 1), river.size * riverWidthScale);
                    }
                }
            }
        }
    }

    public void DisplayMapResource(MapResource mapResource)
    {
        this.mapResource = mapResource;
        Update();
    }
}

public enum DisplayMode
{
    Normal,
    Height,
    MoistureNoise,
    LatTemperature,
    CellMoisture,
    Moisture,
    WalkingSpeed,
    Resource
}
