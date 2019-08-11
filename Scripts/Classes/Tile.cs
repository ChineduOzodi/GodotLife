using Godot;
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
    public float maxSpeedMod;
    public float speedMod;
    public float baseSpeedMod;
    public float recoveryRate;
    public Vector2 position;
    public Dictionary<String, float> distanceToLoctaion = new Dictionary<string, float>();
    public double lastUpdated;
}

public enum TileType
{
    land = 0,
    Water = 1,
    ice = 2,
    snow = 3
}
