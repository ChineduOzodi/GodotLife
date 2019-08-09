using Godot;
using System;
using System.Collections.Generic;

public class Tile
{
    public string name;
    public TileType biome;
    public float elev;
    public float elevAboveSeaLevel;
    public float maxSpeedMod;
    public float speedMod;
    public float baseSpeedMod;
    public float recoveryRate;
    public Vector2 position;
    public Dictionary<String, float> distanceToLoctaion = new Dictionary<string, float>();
    public double lastUpdated;
}
