using Godot;
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

    private long time = 0;
    private int tileSize = 64;
    private int width = 300;
    private int height = 300;
    private int xLimit;
    private int yLimit;
    TileMap tileMap;
    private OpenSimplexNoise noise = new OpenSimplexNoise();
    private float waterLevel = 0.6f;
    private int xOffset;
    private int yOffset;
    private RandomNumberGenerator random = new RandomNumberGenerator();
    private int cityCount = 10;
    private int peopleCount = 100;

    

    private PackedScene cityPrefab;
    private PackedScene personPrefab;

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

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        cityPrefab = ResourceLoader.Load<PackedScene>("res://Prefabs/City.tscn");
        personPrefab = ResourceLoader.Load<PackedScene>("res://Prefabs/Person.tscn");
        tileMap = GetNode<TileMap>(new NodePath("TileMap"));
        GenerateWorld(width, height);
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        time += (long) delta;
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
                tiles[x][y] = new Tile();
                tiles[x][y].name = $"${x + xOffset},{y + yOffset}";
                tiles[x][y].height = (noise.GetNoise2d(x + xOffset, y + yOffset) + 1) * .5f;

                if (tiles[x][y].height > waterLevel)
                {
                   tiles[x][y].biome =  TileType.Grassland;
                   tiles[x][y].speedMod = 0.60f;
                }
                else
                {
                    tiles[x][y].biome = TileType.Water;
                    tiles[x][y].speedMod = 0.10f;
                }
                
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
}


