using Godot;
using Life.Scripts.Classes;
using System;
using System.Collections.Generic;

public class City : Node2D
{
    //city generation
    private World world;
    private const float familyPerCity = 50;
    private const float minAdultAge = 18;
    private const float maxAdultAge = 60;
    private const float marriedPercent = .5f;
    private const float marriedAgeDifferenceMax = 10;
    private const float childrenAgeSubtractionMin = 16;
    private const float grandparentPercent = .5f;
    private const float grandparentAgeAdditionMin = 16;
    private const float grandparentAgeMax = 100;
    private const float percentChanceOwnBusiness = 0.1f;

    public string name;
    public List<Coord> tileCoords = new List<Coord>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    public void GenerateCity(World world, Tile tile)
    {
        this.world = world;
        tileCoords.Add(tile.coord);
        tile.GenerateProperties();

        for (int x = 0; x < Tile.subTileWidthHeight; x++)
        {
            for (int y = 0; y < Tile.subTileWidthHeight; y++)
            {
                Property property = tile.properties[x][y];
                if (x == y || Tile.subTileWidthHeight - x - 1 == y || Tile.subTileWidthHeight - y - 1 == x || x == Tile.subTileWidthHeight / 2 || y == Tile.subTileWidthHeight / 2)
                {
                    property.hasRoad = true;
                }
                tile.RemoveTrees(property.treeCount, x, y);

            }
        }
        Update();
    }

    public override void _Draw()
    {
        base._Draw();
        Console.WriteLine("Updating City Map");
        float squareWidthHeight = (float) world.TileSize / Tile.subTileWidthHeight;
        Tile tile = world.tiles[tileCoords[0].x][tileCoords[0].y];
        //Color roadColor = new Color(213f / 255, 204f / 255, 127f / 255);
        Color roadColor = Color.ColorN("brown");
        Console.WriteLine($"squareWidthHeight: {squareWidthHeight}");

        for (int x = 0; x < Tile.subTileWidthHeight; x++)
        {
            for (int y = 0; y < Tile.subTileWidthHeight; y++)
            {
                Property property = tile.properties[x][y];
                if (property.hasRoad)
                {
                    float propertyX = property.x * squareWidthHeight - Tile.subTileWidthHeight / 2f * squareWidthHeight;
                    float propertyY = property.y * squareWidthHeight - Tile.subTileWidthHeight / 2f * squareWidthHeight;
                    Console.WriteLine($"road property: {propertyX}, {propertyY}");
                    Vector2 rectPosition = new Vector2(propertyX, propertyY);
                    DrawRect(new Rect2(rectPosition, squareWidthHeight, squareWidthHeight),
                        roadColor);
                    //Console.WriteLine($"drew road");
                }
            }
        }
    }
}

public struct Coord
{
    public int x;
    public int y;

    public Coord(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}
