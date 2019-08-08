using Godot;
using Life.Scripts.Pathfinding;
using System;
using System.Collections.Generic;

public class World : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    public Tile[][] tiles;
    public List<City> cities = new List<City>();
    public List<Person> people = new List<Person>();
    public Dictionary<string, Road> roads = new Dictionary<string, Road>();

    private long time = 0;
    private int tileSize = 64;
    private int width = 300;
    private int height = 300;
    private int xLimit;
    private int yLimit;
    TileMap tileMap;
    private OpenSimplexNoise noise = new OpenSimplexNoise();
    DrawingLine drawingLine;
    private float waterLevel = 0.6f;
    private int xOffset;
    private int yOffset;
    private RandomNumberGenerator random = new RandomNumberGenerator();
    private int cityCount = 10;
    private int peopleCount = 100;
    private bool pathDrawn = false;
    private Person nearestPerson;
    float nearestPersonDistanceSquared;

    private PackedScene cityPrefab;
    private PackedScene personPrefab;
    private PackedScene roadPrefab;

    public int XOffset { get => xOffset; }
    public int YOffset { get => yOffset; }
    public float WaterLevel { get => waterLevel; }
    public int Width { get => width; }
    public int Height { get => height; }
    public int TileSize { get => tileSize; }
    public int YLimit { get => yLimit; }
    public int XLimit { get => xLimit; }
    public RandomNumberGenerator Random { get => random; }
    public long Time { get => time; }
    public Person NearestPerson { get => nearestPerson; }
    public float NearestPersonDistanceSquared { get => nearestPersonDistanceSquared; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        cityPrefab = ResourceLoader.Load<PackedScene>("res://Prefabs/City.tscn");
        personPrefab = ResourceLoader.Load<PackedScene>("res://Prefabs/Person.tscn");
        roadPrefab = ResourceLoader.Load<PackedScene>("res://Prefabs/Road/Road.tscn");
        tileMap = GetNode<TileMap>(new NodePath("TileMap"));
        drawingLine = GetNode<DrawingLine>(new NodePath("DrawingLine"));
        GenerateWorld(width, height);
    }

    

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        time += (long) delta;

        if (nearestPerson != null)
        {
            nearestPersonDistanceSquared = nearestPerson.GetGlobalPosition().DistanceSquaredTo(GetGlobalMousePosition());
        }
        for (int i = 0; i < people.Count; i++)
        {
            Person person = people[i];
            if (nearestPerson == null)
            {
                nearestPerson = person;
                nearestPersonDistanceSquared = nearestPerson.GetGlobalPosition().DistanceSquaredTo(GetGlobalMousePosition());
            }

            float distSquared = person.GetGlobalPosition().DistanceSquaredTo(GetGlobalMousePosition());

            if (distSquared < nearestPersonDistanceSquared)
            {
                nearestPerson = person;
                nearestPersonDistanceSquared = nearestPerson.GetGlobalPosition().DistanceSquaredTo(GetGlobalMousePosition());
                drawingLine.Update();
                
            }
        }
       
        if (Input.IsActionPressed("R"))
        {
            GenerateWorld(width, height);
        }
    }

    public void GenerateWorld(int width, int height)
    {
        xOffset = -width / 2;
        yOffset = -height / 2;
        xLimit = (width + xOffset) * tileSize;
        yLimit = (height + yOffset) * tileSize;
        noise.SetSeed(DateTime.Now.TimeOfDay.Milliseconds);
        random.SetSeed(DateTime.Now.TimeOfDay.Milliseconds);
        tileMap.CreateNoise(width, height, noise, waterLevel);
        tiles = new Tile[width][];
        for (int x = 0; x < width; x++)
        {
            tiles[x] = new Tile[height];
            for (int y = 0; y < height; y++)
            {
                Tile tile = new Tile();
                tile.name = $"${(x + xOffset) * tileSize},{(y + yOffset) * tileSize}";
                tile.height = (noise.GetNoise2d(x + xOffset, y + yOffset) + 1) * .5f;
                tile.position = new Vector2((x + xOffset) * tileSize, (y + yOffset) * tileSize);

                if (tile.height > waterLevel)
                {
                   tile.biome =  TileType.Grassland;
                   tile.speedMod = 0.60f;
                    tile.baseSpeedMod = 0.6f;
                    tile.recoveryRate = 0.001f;
                    tile.maxSpeedMod = 5;
                }
                else
                {
                    tile.biome = TileType.Water;
                    tile.speedMod = 0.10f;
                    tile.baseSpeedMod = 0.1f;
                    tile.recoveryRate = 0.001f;
                    tile.maxSpeedMod = 1;
                }

                tiles[x][y] = tile;
            }
        }
        GenerateCities(cityCount);
    }

    private void GenerateCities(int num)
    {
        DeleteCities();
        cities = new List<City>();
        int tries = 0;
        while(tries < 1000 && num > 0)
        {
            int randomX = random.RandiRange(0, width - 1);
            int randomY = random.RandiRange(0, height - 1);
            //Console.WriteLine($"randomX:{randomX}, randomY: {randomY}, width:{tiles.Length}, height:{tiles[0].Length}");
            if (tiles[randomX][randomY].biome == TileType.Grassland)
            {
                City city = (City) cityPrefab.Instance();
                AddChild(city);
                cities.Add(city);
                city.Name = "city" + num;
                city.name = city.Name;
                Vector2 pos = new Vector2((randomX + xOffset) * tileSize, (randomY + yOffset) * tileSize);
                city.SetPosition(pos);
                tries = 0;
                num--;
                Console.WriteLine($"Created {city.name}");
                for (int i = 0; i < peopleCount; i++)
                {
                    GeneratePerson(pos);
                }
            } else
            {
                tries++;
                Console.WriteLine($"tries: {tries}");
            }
        }
    }

    private void GeneratePerson(Vector2 position)
    {
        Person person = (Person)personPrefab.Instance();
        person.SetPosition(position);
        AddChild(person);
        people.Add(person);
    }
    private void DeleteCities()
    {
        for (int i = cities.Count - 1; i >= 0; i--)
        {
            cities[i].Free();
        }
    }

    public Tile GetTile(Vector2 position)
    {
        Coord pos = new Coord(Mathf.RoundToInt(position.x / tileSize) * tileSize, Mathf.RoundToInt(position.y / tileSize) * tileSize);
        int x = pos.x / tileSize - xOffset;
        int y = pos.y / tileSize - xOffset;

        return tiles[x][y];
    }

    public void GenerateRoad(Vector2 position, Tile tile)
    {
        Road road = (Road)roadPrefab.Instance();
        AddChild(road);
        road.tile = tile;
        road.SetPosition(position);
        roads[$"{position.ToString()}"] = road;
    }
}


