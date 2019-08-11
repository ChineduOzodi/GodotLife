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
    public Dictionary<String, RiverData> rivers = new Dictionary<string, RiverData>();

    private double time = 0;
    
    private int xLimit;
    private int yLimit;
    private float elevChangeCost = 500;
    
    
    private DrawingLine drawingLine;
    private Map map;
    private Rivers riverDisplay;
    private DisplayMode mapDisplayMode = DisplayMode.Normal;
    private int xOffset;
    private int yOffset;
    private RandomNumberGenerator random = new RandomNumberGenerator();
    private Person nearestPerson;
    float nearestPersonDistanceSquared;
    private float mapUpdateInterval = 5;
    private double mapLastUpdate;


    //Map Generation Variables
    private int tileSize = 32;
    private int width = 300;
    private int height = 300;
    private float waterLevel = 0.5f;
    private int cityCount = 30;
    private int peopleCount = 20;
    private float riverSpawnProbability = .1f;
    private float riverFormationMoistureScale = 3f; //greater number = less river formation for less moisture places;
    private float riverMovementMoistureScale = 1;
    private float riverMocementMoistureCarryScale = .9f;
    private float riverCrossingDifficultyScale = 11f;
    private float riverCrossingDifficultyMax = 0.01f;
    private float elevationNoiseScale = .5f;
    private float moistureNoiseScale = .75f;

    private OpenSimplexNoise moistureNoise = new OpenSimplexNoise();
    private OpenSimplexNoise elevationNoise = new OpenSimplexNoise();

    // Scenes for instancing
    private PackedScene cityPrefab;
    private PackedScene personPrefab;

    public int XOffset { get => xOffset; }
    public int YOffset { get => yOffset; }
    public int Width { get => width; }
    public int Height { get => height; }
    public int TileSize { get => tileSize; }
    public int YLimit { get => yLimit; }
    public int XLimit { get => xLimit; }
    public RandomNumberGenerator Random { get => random; }
    public double Time { get => time; }
    public Person NearestPerson { get => nearestPerson; }
    public float NearestPersonDistanceSquared { get => nearestPersonDistanceSquared; }
    public float ElevChangeCost { get => elevChangeCost; }
    public DisplayMode MapDisplayMode { get => mapDisplayMode; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        cityPrefab = ResourceLoader.Load<PackedScene>("res://Prefabs/City.tscn");
        personPrefab = ResourceLoader.Load<PackedScene>("res://Prefabs/Person.tscn");
        map = GetNode<Map>(new NodePath("Map"));
        riverDisplay = GetNode<Rivers>(new NodePath("Rivers"));
        drawingLine = GetNode<DrawingLine>(new NodePath("DrawingLine"));
        GenerateWorld(width, height);
    }

    

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        time += (double) delta;

        if (mapLastUpdate + mapUpdateInterval < time && (mapDisplayMode == DisplayMode.Normal || mapDisplayMode == DisplayMode.WalkingSpeed))
        {
            mapLastUpdate = time;
            map.GenerateMap();
        }
        //else
        //{
        //    Console.WriteLine($"{mapLastUpdate + mapUpdateInterval} < {time}, delta: {(double)delta}");
        //}

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
        moistureNoise.SetSeed(DateTime.Now.TimeOfDay.Milliseconds + "moisture".GetHashCode());
        elevationNoise.SetSeed(DateTime.Now.TimeOfDay.Milliseconds + "elevation".GetHashCode());
        elevationNoise.SetOctaves(20);
        random.SetSeed(DateTime.Now.TimeOfDay.Milliseconds);
        tiles = new Tile[width][];
        for (int x = 0; x < width; x++)
        {
            tiles[x] = new Tile[height];
            for (int y = 0; y < height; y++)
            {
                Tile tile = new Tile();
                float elevationPow = 2;

                tile.name = $"${(x + xOffset) * tileSize},{(y + yOffset) * tileSize}";
                float elevationNoiseNumber = elevationNoise.GetNoise2d((x + xOffset) * elevationNoiseScale, (y + yOffset) * elevationNoiseScale);
                tile.elev = ( elevationNoiseNumber + 1) * .5f;
                float elevAboveSeaLevel = tile.elev - waterLevel;
                //if (elevAboveSeaLevel < 0)
                //    Console.WriteLine($"elevAboveSeaLevel: {elevAboveSeaLevel}");
                float elevAboveSeaLevelScaled = (elevAboveSeaLevel >= 0) ? (elevAboveSeaLevel / (1 - waterLevel)) : elevAboveSeaLevel / waterLevel;
                //if (elevAboveSeaLevelScaled < 0)
                //    Console.WriteLine($"elevAboveSeaLevelScaled: {elevAboveSeaLevelScaled}");
                float alteredElevationAboveSeaLevelNumber = ( elevAboveSeaLevelScaled >= 0) ? Mathf.Pow(elevAboveSeaLevelScaled, elevationPow) :  -Mathf.Pow(-elevAboveSeaLevelScaled, elevationPow);
                tile.elev = alteredElevationAboveSeaLevelNumber;
                float moistureNoisePow = 0.75f;
                float moistureNoiseNumber = moistureNoise.GetNoise2d((x + xOffset) * moistureNoiseScale, (y + yOffset) * moistureNoiseScale);
                float alteredMoistureNoiseNumber = (moistureNoiseNumber < 0) ? -Mathf.Pow(-moistureNoiseNumber, moistureNoisePow) : Mathf.Pow(moistureNoiseNumber, moistureNoisePow);
                tile.moistureNoise = (alteredMoistureNoiseNumber + 1) * .5f;
                tile.latTemperature = -Mathf.Pow(((float) y + yOffset) / (-yOffset), 2) + 1;
                tile.cellMoisture = (Mathf.Cos((((float)y + yOffset) / (-yOffset)) * Mathf.Pi * 3) + 1) * 0.5f;

                //calculate moisture
                float elevMoisture = (tile.elev >= 0) ? 1 - tile.elev : 1;

                float elevMoistureScale = 1;
                float cellMoistureScale = 1;
                float latTemperatureScale = 1;
                float moistureNoisModScale = 2;

                tile.moisture = (elevMoisture * elevMoistureScale + tile.cellMoisture * cellMoistureScale - ( 1 - tile.latTemperature) * latTemperatureScale
                    + tile.moistureNoise * moistureNoisModScale) / (elevMoistureScale + cellMoistureScale + latTemperatureScale + moistureNoisModScale);
                tile.position = new Vector2((x + xOffset) * tileSize, (y + yOffset) * tileSize);

                if (tile.moisture < 0)
                {
                    tile.moisture = 0;
                    Console.WriteLine("negative moisture");
                }

                if (tile.elev > 0)
                {
                    float snowLevel = 0.4f;
                    float moistureScale = 1;
                    float elevationScale = 1;

                    float snowPossibility = (tile.moisture * moistureScale + tile.elev * elevationScale) / (moistureScale + elevationScale);
                    if (snowPossibility >= snowLevel)
                    {
                        tile.biome = TileType.snow;
                        tile.speedMod = 0.20f;
                        tile.baseSpeedMod = 0.2f;
                        tile.recoveryRate = 0.001f;
                        tile.maxSpeedMod = 1;
                        //Console.WriteLine("tile biome set to snow");
                    } else
                    {
                        tile.biome = TileType.land;
                        tile.speedMod = 0.60f;
                        tile.baseSpeedMod = 0.6f;
                        tile.recoveryRate = 0.001f;
                        tile.maxSpeedMod = 1;
                    }

                    float iceLevel = 0.6f;
                    float moistureIceScale = 1;
                    float temperatureScale = 1;
                    float elevationIceScale = .1f;

                    float icePossibility = ( (1 - tile.moisture) * moistureIceScale + (1 - tile.latTemperature) * temperatureScale + tile.elev * elevationIceScale) / (moistureIceScale + temperatureScale + elevationIceScale);

                    if (icePossibility >= iceLevel)
                    {
                        tile.biome = TileType.ice;
                        tile.speedMod = 0.3f;
                        tile.baseSpeedMod = 0.3f;
                        tile.recoveryRate = 0.001f;
                        tile.maxSpeedMod = 1;
                        //Console.WriteLine("tile biome set to ice");
                    }
                }
                else
                {
                    float iceLevel = 0.8f;
                    float moistureScale = 1;
                    float temperatureScale = 1;

                    float icePossibility = ((1 - tile.moisture) * moistureScale + (1 - tile.latTemperature) * temperatureScale) / (moistureScale + temperatureScale);

                    if (icePossibility >= iceLevel)
                    {
                        tile.biome = TileType.ice;
                        tile.speedMod = 0.3f;
                        tile.baseSpeedMod = 0.3f;
                        tile.recoveryRate = 0.001f;
                        tile.maxSpeedMod = 1;
                    } else
                    {
                        tile.biome = TileType.Water;
                        tile.speedMod = 0.1f;
                        tile.baseSpeedMod = 0.1f;
                        tile.recoveryRate = 0.001f;
                        tile.maxSpeedMod = 1;
                    }
                }

                tiles[x][y] = tile;
            }
        }
        map.GenerateMap();
        GenerateRivers();
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
            if (tiles[randomX][randomY].biome == TileType.land)
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
                //Console.WriteLine($"Created {city.name}");
                for (int i = 0; i < peopleCount; i++)
                {
                    GeneratePerson(pos);
                }
            } else
            {
                tries++;
                //Console.WriteLine($"tries: {tries}");
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

    public void GenerateRivers()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //Console.WriteLine($"x: {x} y: {y}");
                Tile tile = tiles[x][y];
                Vector2 previousPosition = Vector2.Zero;
                int count = 0;
                if (random.Randf() <= Mathf.Pow(tile.moisture,riverFormationMoistureScale) && random.Randf() <= riverSpawnProbability && (tile.biome == TileType.land || tile.biome == TileType.snow) && !tile.hasFreshWater)
                {
                    //Console.WriteLine($"{tile.position.ToString()} starting river");
                    while (true)
                    {
                        count++;
                        //check if tile already has river
                        if (tile.hasFreshWater)
                        {
                            if (tile.hasWaterBasin)
                            {
                                //Console.WriteLine($"{tile.position.ToString()} already has basin");
                                tile.waterBasinSize += rivers[$"{previousPosition.ToString()}"].size * riverMocementMoistureCarryScale;
                                //set new tile attributes
                                float riverCrossingDifficulty = (1 * riverCrossingDifficultyScale - tile.waterBasinSize) / riverCrossingDifficultyScale;
                                if (riverCrossingDifficulty < riverCrossingDifficultyMax)
                                {
                                    Console.WriteLine($"WARNING: basin crossing difficulty crossing threshold: {riverCrossingDifficulty} < {riverCrossingDifficultyMax}");
                                }
                                tile.riverCrossingSpeed = (riverCrossingDifficulty <= riverCrossingDifficultyMax) ? riverCrossingDifficultyMax : riverCrossingDifficulty;
                                break;
                            } else
                            {
                                //find data increment number
                                //Console.WriteLine($"{tile.position.ToString()} already has river data");
                                

                                int num = 0;
                                float riverMoistureCarry = rivers[$"{previousPosition.ToString()}"].size * riverMocementMoistureCarryScale;
                                RiverData riverData = rivers[$"{tile.position.ToString()}"];
                                riverData.fromPositions.Add(previousPosition);
                                while (true)
                                {
                                    num++;
                                    if (tile.hasWaterBasin)
                                    {
                                        tile.waterBasinSize += riverMoistureCarry;
                                        //set new tile attributes
                                        float riverCrossingDifficulty = (1 * riverCrossingDifficultyScale - tile.waterBasinSize) / riverCrossingDifficultyScale;
                                        if (riverCrossingDifficulty < riverCrossingDifficultyMax)
                                        {
                                            Console.WriteLine($"WARNING: basin crossing difficulty crossing threshold: {riverCrossingDifficulty} < {riverCrossingDifficultyMax}");
                                        }
                                        tile.riverCrossingSpeed = (riverCrossingDifficulty <= riverCrossingDifficultyMax) ? riverCrossingDifficultyMax : riverCrossingDifficulty;
                                        break;
                                    } else if (tile.biome == TileType.land || tile.biome == TileType.snow)
                                    {
                                        riverData = rivers[$"{tile.position.ToString()}"];
                                        riverData.size += riverMoistureCarry;
                                        riverMoistureCarry *= riverMocementMoistureCarryScale;

                                        //set new tile attributes
                                        float riverCrossingDifficulty = (1 * riverCrossingDifficultyScale - riverData.size) / riverCrossingDifficultyScale;
                                        if (riverCrossingDifficulty < riverCrossingDifficultyMax)
                                        {
                                            Console.WriteLine($"WARNING: river crossing difficulty crossing threshold: {riverCrossingDifficulty} < {riverCrossingDifficultyMax}");

                                        }
                                        tile.riverCrossingSpeed = (riverCrossingDifficulty <= riverCrossingDifficultyMax) ? riverCrossingDifficultyMax : riverCrossingDifficulty;

                                        if (riverData.toPosition.x != tile.position.x && riverData.toPosition.y != tile.position.y)
                                        {
                                            float newRiverCrossingSpeed = tile.riverCrossingSpeed  + (1 - tile.riverCrossingSpeed) * 0.5f;
                                            Tile nTile1 = GetTile(new Vector2(riverData.toPosition.x, tile.position.y));
                                            Tile nTile2 = GetTile(new Vector2(tile.position.x, riverData.toPosition.y));
                                            nTile1.riverCrossingSpeed *= newRiverCrossingSpeed;
                                            nTile2.riverCrossingSpeed *= newRiverCrossingSpeed;
                                        }
                                        //Console.WriteLine($"Tile speedMod: {tile.speedMod}");
                                        tile = GetTile(riverData.toPosition);
                                    } else
                                    {
                                        break;
                                    }
                                    

                                    if (num > 1000)
                                    {
                                        Console.WriteLine("ERROR: Entered into a forever loop");
                                        break;
                                    }
                                }
                                break;
                            }
                            
                        }
                        else
                        {
                            //try to create new river data
                            if (tile.biome == TileType.land || tile.biome == TileType.snow)
                            {
                                //spawn stream
                                //Console.WriteLine($"{tile.position.ToString()} creating possible river data");
                                tile.hasFreshWater = true;
                                
                                Tile toTile = null;
                                float distanceDiff = 0;

                                for (int nx = -tileSize; nx <= tileSize; nx += TileSize)
                                {
                                    for (int ny = -tileSize; ny <= tileSize; ny += tileSize)
                                    {
                                        if (nx == 0 && ny == 0)
                                            continue;

                                        float checkX = tile.position.x + nx;
                                        float checkY = tile.position.y + ny;

                                        if (checkX >= -xLimit && checkX < xLimit && checkY >= -yLimit && checkY < yLimit)
                                        {
                                            Tile neighborTile =GetTile(new Vector2(checkX,checkY));
                                            float neighborDiff = neighborTile.elev - tile.elev;
                                            if (neighborDiff < distanceDiff)
                                            {
                                                distanceDiff = neighborDiff;
                                                toTile = neighborTile;
                                            }
                                        }

                                        
                                    }
                                }

                                if (toTile != null)
                                {
                                    //set stats
                                    //Console.WriteLine($"{tile.position.ToString()} creating new river data");
                                    RiverData riverData = new RiverData();
                                    riverData.position = tile.position;
                                    if (count != 1)
                                    {
                                        riverData.fromPositions.Add(previousPosition);
                                    }
                                    riverData.size = 0;
                                    riverData.fromPositions.ForEach( fromPosition => riverData.size += rivers[$"{fromPosition.ToString()}"].size * riverMocementMoistureCarryScale);
                                    //Console.WriteLine($"river size: {riverData.size}");
                                    riverData.size += tile.moisture * riverMovementMoistureScale;
                                    
                                    previousPosition = tile.position;
                                    riverData.toPosition = toTile.position;
                                    //TODO: calculate speed

                                    //set new tile attributes
                                    float riverCrossingDifficulty = (1 * riverCrossingDifficultyScale - riverData.size) / riverCrossingDifficultyScale;
                                    if (riverCrossingDifficulty < riverCrossingDifficultyMax)
                                    {
                                        Console.WriteLine($"WARNING: river crossing difficulty crossing threshold: {riverCrossingDifficulty} < {riverCrossingDifficultyMax}");
                                    }

                                    tile.riverCrossingSpeed = (riverCrossingDifficulty <= riverCrossingDifficultyMax) ? riverCrossingDifficultyMax : riverCrossingDifficulty;
                                    if (riverData.toPosition.x != tile.position.x && riverData.toPosition.y != tile.position.y)
                                    {
                                        float newRiverCrossingSpeed = tile.riverCrossingSpeed + (1 - tile.riverCrossingSpeed) * 0.5f;
                                        Tile nTile1 = GetTile(new Vector2(riverData.toPosition.x, tile.position.y));
                                        Tile nTile2 = GetTile(new Vector2(tile.position.x, riverData.toPosition.y));
                                        nTile1.riverCrossingSpeed *= newRiverCrossingSpeed;
                                        nTile2.riverCrossingSpeed *= newRiverCrossingSpeed;
                                    }

                                    rivers[$"{tile.position.ToString()}"] = riverData;
                                    tile = toTile;
                                } else
                                {
                                    //Console.WriteLine($"{tile.position.ToString()} creating new river basin");
                                    tile.hasWaterBasin = true;
                                    tile.waterBasinSize += 1;

                                    //set new tile attributes
                                    float riverCrossingDifficulty = (1 * riverCrossingDifficultyScale - tile.waterBasinSize) / riverCrossingDifficultyScale;
                                    if (riverCrossingDifficulty < riverCrossingDifficultyMax)
                                    {
                                        Console.WriteLine($"WARNING: basin crossing difficulty crossing threshold: {riverCrossingDifficulty} < {riverCrossingDifficultyMax}");
                                    }

                                    tile.riverCrossingSpeed = (riverCrossingDifficulty <= riverCrossingDifficultyMax) ? riverCrossingDifficultyMax : riverCrossingDifficulty;
                                    break;
                                }

                            } else
                            {
                                //Console.WriteLine($"{tile.position.ToString()} not land or snow");
                                break;
                            }
                        }

                        if (count > 1000)
                        {
                            Console.WriteLine($"Breaking from river generation, either really long river or forever loop");
                            count = 0;
                            break;
                        }
                    }
                }
            }
        }
        riverDisplay.GenerateRivers();
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

    public void UpdateDisplayMode( DisplayMode displayMode)
    {
        mapDisplayMode = displayMode;
        map.GenerateMap();
    }
}

public class RiverData
{
    public string name;
    public Vector2 position;
    public Vector2 toPosition;
    public float size;
    public float speed;
    public List<Vector2> fromPositions = new List<Vector2>();
}


