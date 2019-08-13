using Godot;
using Life.Scripts.Classes;
using System;
using System.Collections.Generic;

public class Tile
{
    public string name;

    public TileType biome;
    public float elev;
    public float moistureNoise;
    public float latTemperature;
    public float cellMoisture;
    public float moisture;

    // movement
    public float maxSpeedMod;
    public float speedMod;
    public float riverCrossingSpeed = 1;
    public float baseSpeedMod;
    public float recoveryRate;

    // water/rivers
    public bool hasRiver;
    public bool hasWaterBasin;
    public float waterBasinSize = 0;
    public Vector2 position;
    public Dictionary<String, float> distanceToLoctaion = new Dictionary<string, float>();
    public double lastUpdated;

    // resources
    public List<MapResourceData> resources = new List<MapResourceData>();
    public bool HasMapResource(string resource)
    {
        return resources.Exists(x => x.resource == resource);
    }
    public MapResourceData GetMapResourceData(string resource)
    {
        return resources.Find(x => x.resource == resource);
    }
}

public enum TileType
{
    land = 0,
    Water = 1,
    SeaIce = 2,
    Glacier = 3,
    snow = 4
}
