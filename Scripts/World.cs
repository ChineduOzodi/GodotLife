using Godot;
using Life.Scripts.Classes;
using Life.Scripts.Classes.Tiles;
using Newtonsoft.Json;
using RandomNameGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

public partial class World : Node2D
{
	public const float HourDuration = 60; // in seconds
	public const float DayDuration = HourDuration * 24;
	public const float YearDuration = HourDuration * 24;
	public const float TileScale = 0.1f; //in km
	public const float TileArea = TileScale * TileScale; //in km^2
	public static World Instance;
	public int idGen = 1;
	public const int PEOPLE_COUNT = 200;

	public RandomName peopleNames;
	public CityNames cityNames;


	public FloorTile[][] floorTiles;
	public GroundTile[][] groundTiles;
	public List<City> cities = new List<City>();
	public Dictionary<String, PersonData> people = new Dictionary<string, PersonData>();
	public Dictionary<String, RiverData> rivers = new Dictionary<string, RiverData>();
	public List<MapResource> mapResources = new List<MapResource>();
	public Dictionary<String, Building> buildings = new Dictionary<string, Building>();
	public BuildingConstructionPlan[] buildingConstructionPlans;
	public ItemIdentification[] itemIdentifications;
	public Professions[] professions;


	private FloorTile hoveredTile = null;

	private GDate gDate = new GDate(2000, 0, 0, 0, 0, 0);
	private int xLimit;
	private int yLimit;
	private float elevChangeCost = 500;


	private DrawingLine drawingLine;
	private DisplayMenu displayMenu;
	private Map map;
	private WorldMap worldMap;
	private Rivers riverDisplay;
	private MapResourceOverlay mapResourceOverlay;
	private DisplayMode mapDisplayMode = DisplayMode.Normal;
	private RandomNumberGenerator random = new RandomNumberGenerator();
	private Person nearestPerson;
	float nearestPersonDistanceSquared;
	private float mapUpdateInterval = 5;
	private double mapLastUpdate;


	//Map Generation Variables
	private int tileSize = 16; // in pixels
	private int width = 300 * 2;
	private int height = 200 * 2;
	private float waterLevel = 0.5f;
	private int cityCount = 30;
	private int peopleCount = 20;
	private float riverSpawnProbability = .1f * TileScale;
	private float riverFormationMoistureScale = 3f; //greater number = less river formation for less moisture places;
	private float riverMovementMoistureScale = 1;
	private float riverMocementMoistureCarryScale = .9f * TileScale;
	private float elevationNoiseScale = 1f * TileScale;
	private float moistureNoiseScale = .75f * TileScale;
	private float goldOreNoiseScale = 20f * TileScale;
	private float goldOreNoiseMin = .6f;
	private float goldOreScale = 0.5f * TileScale;
	private float ironOreNoiseScale = 10f * TileScale;
	private float ironOreNoiseMin = 0.5f;
	private float ironOreScale = 1f * TileScale;
	private float rockElevationDiffScale = 1f * TileScale;
	private float rockMinElevationDiff = 0.005f;
	private float dearProbability = 0.5f;
	private float dearScale = 50 * TileScale;
	private float freshWaterFishProbability = 1f;
	private float freshWaterFishMinSize = 2f;
	private float freshWaterFishScale = 20f * TileScale;
	private float seaFishProbability = 1f;
	private float seaFishScale = 100f * TileScale;



	private FastNoiseLite moistureNoise = new FastNoiseLite();
	private FastNoiseLite elevationNoise = new FastNoiseLite();
	private FastNoiseLite ironOreNoise = new FastNoiseLite();
	private FastNoiseLite goldOreNoise = new FastNoiseLite();

	// Scenes for instancing
	private PackedScene cityPrefab;
	private PackedScene personPrefab;

	public int Width { get => width; }
	public int Height { get => height; }
	public int TileSize { get => tileSize; }
	public int YLimit { get => yLimit; }
	public int XLimit { get => xLimit; }
	public RandomNumberGenerator Random { get => random; }
	public double Time { get => gDate.time; }
	public Person NearestPerson { get => nearestPerson; }
	public float NearestPersonDistanceSquared { get => nearestPersonDistanceSquared; }
	public float ElevChangeCost { get => elevChangeCost; }
	public DisplayMode MapDisplayMode { get => mapDisplayMode; }
	public FloorTile HoveredTile { get => hoveredTile; set => hoveredTile = value; }
	public GDate GDate { get => gDate; }
	public Camera2D camera;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
		cityPrefab = ResourceLoader.Load<PackedScene>("res://Prefabs/City.tscn");
		personPrefab = ResourceLoader.Load<PackedScene>("res://Prefabs/Person.tscn");
		map = GetNode<Map>(new NodePath("Map"));
		worldMap = GetNode<WorldMap>(new NodePath("WorldMap"));
		riverDisplay = GetNode<Rivers>(new NodePath("Rivers"));
		displayMenu = GetNode<DisplayMenu>(new NodePath("CanvasLayer/DisplayMenu"));
		drawingLine = GetNode<DrawingLine>(new NodePath("DrawingLine"));
		mapResourceOverlay = GetNode<MapResourceOverlay>(new NodePath("MapResourceOverlay"));
		camera = GetNode<Camera2D>("RTS-Camera2D");

		LoadGameResources();
		CreateMapResources();
		GenerateWorld(width, height);
	}





	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		gDate.AddTime((double)delta);

		//if (mapLastUpdate + mapUpdateInterval < time && (mapDisplayMode == DisplayMode.Normal || mapDisplayMode == DisplayMode.WalkingSpeed))
		//{
		//    mapLastUpdate = time;
		//    map.GenerateMap();
		//}
		//else
		//{
		//    Console.WriteLine($"{mapLastUpdate + mapUpdateInterval} < {time}, delta: {(double)delta}");
		//}

		// show hovered over FloorTile
		Vector2 mousePosition = GetGlobalMousePosition();
		Coord mouseTileCoord = new Coord((int)((mousePosition.X + tileSize * 0.5f) / tileSize), (int)((mousePosition.Y + tileSize * 0.5f) / tileSize));
		if (mouseTileCoord.x >= 0 && mouseTileCoord.y >= 0 && mouseTileCoord.x < width && mouseTileCoord.x < height)
		{
			hoveredTile = floorTiles[mouseTileCoord.x][mouseTileCoord.y];
		}
		else
		{
			hoveredTile = null;
		}

		if (nearestPerson != null)
		{
			nearestPersonDistanceSquared = nearestPerson.GlobalPosition.DistanceSquaredTo(GetGlobalMousePosition());
		}
		//for (int i = 0; i < people.Count; i++)
		//{
		//    Person person = people[i];
		//    if (nearestPerson == null)
		//    {
		//        nearestPerson = person;
		//        nearestPersonDistanceSquared = nearestPerson.GetGlobalPosition().DistanceSquaredTo(GetGlobalMousePosition());
		//    }

		//    float distSquared = person.GetGlobalPosition().DistanceSquaredTo(GetGlobalMousePosition());

		//    if (distSquared < nearestPersonDistanceSquared)
		//    {
		//        nearestPerson = person;
		//        nearestPersonDistanceSquared = nearestPerson.GetGlobalPosition().DistanceSquaredTo(GetGlobalMousePosition());
		//        drawingLine.Update();

		//    }
		//}

		if (Input.IsActionPressed("R"))
		{
			GenerateWorld(width, height);
		}
	}

	public void LoadGameResources()
	{
		JsonSerializer serializer = new JsonSerializer();

		using (StreamReader reader = new StreamReader("resources/buildings.json"))
		using (JsonReader jreader = new JsonTextReader(reader))
		{
			buildingConstructionPlans = serializer.Deserialize<BuildingConstructionPlan[]>(jreader);
		}

		using (StreamReader reader = new StreamReader("resources/items.json"))
		using (JsonReader jreader = new JsonTextReader(reader))
		{
			itemIdentifications = serializer.Deserialize<ItemIdentification[]>(jreader);
		}

		using (StreamReader reader = new StreamReader("resources/professions.json"))
		using (JsonReader jreader = new JsonTextReader(reader))
		{
			professions = serializer.Deserialize<Professions[]>(jreader);
		}

		//Console.WriteLine($"Building constructions found: {buildingConstructionPlans.Length}");
	}

	public void GenerateWorld(int width, int height)
	{
		xLimit = width * tileSize;
		yLimit = height * tileSize;
		moistureNoise.Seed = (DateTime.Now.TimeOfDay.Milliseconds + "moisture".GetHashCode());
		goldOreNoise.Seed = (DateTime.Now.TimeOfDay.Milliseconds + "goldOre".GetHashCode());
		ironOreNoise.Seed = (DateTime.Now.TimeOfDay.Milliseconds + "ironOre".GetHashCode());
		elevationNoise.Seed = (DateTime.Now.TimeOfDay.Milliseconds + "elevation".GetHashCode());
		elevationNoise.FractalOctaves = 20;
		random.Seed = (ulong)DateTime.Now.ToBinary();
		int negativeMoistureCount = 0;
		floorTiles = new FloorTile[width][];
		groundTiles = new GroundTile[width][];

        Random r = new((int)(DateTime.Now.ToBinary() + Random.Randi()));
        peopleNames = new RandomName(r);
        cityNames = new CityNames(r);
        
		for (int x = 0; x < width; x++)
		{
			floorTiles[x] = new FloorTile[height];
			groundTiles[x] = new GroundTile[height];

			for (int y = 0; y < height; y++)
			{
                FloorTile tile = new(x,y)
                {
                    name = $"${x * tileSize},{y * tileSize}",
                    position = new Vector2(x * tileSize, y * tileSize)
                };

                float elevationPow = 2;
				float elevationNoiseNumber = elevationNoise.GetNoise2D(x * elevationNoiseScale, y * elevationNoiseScale);
				tile.elev = (elevationNoiseNumber + 1) * .5f;
				float elevAboveSeaLevel = tile.elev - waterLevel;
				//if (elevAboveSeaLevel < 0)
				//    Console.WriteLine($"elevAboveSeaLevel: {elevAboveSeaLevel}");
				float elevAboveSeaLevelScaled = (elevAboveSeaLevel >= 0) ? (elevAboveSeaLevel / (1 - waterLevel)) : elevAboveSeaLevel / waterLevel;
				//if (elevAboveSeaLevelScaled < 0)
				//    Console.WriteLine($"elevAboveSeaLevelScaled: {elevAboveSeaLevelScaled}");
				float alteredElevationAboveSeaLevelNumber = (elevAboveSeaLevelScaled >= 0) ? Mathf.Pow(elevAboveSeaLevelScaled, elevationPow) : -Mathf.Pow(-elevAboveSeaLevelScaled, elevationPow);
				tile.elev = alteredElevationAboveSeaLevelNumber;
				float moistureNoisePow = 0.75f;
				float moistureNoiseNumber = moistureNoise.GetNoise2D((x) * moistureNoiseScale, y * moistureNoiseScale);
				float alteredMoistureNoiseNumber = (moistureNoiseNumber < 0) ? -Mathf.Pow(-moistureNoiseNumber, moistureNoisePow) : Mathf.Pow(moistureNoiseNumber, moistureNoisePow);
				tile.moistureNoise = (alteredMoistureNoiseNumber + 1) * .5f;
				tile.latTemperature = -Mathf.Pow((y - height * .5f) / (height * .5f), 2) + 1;
				tile.cellMoisture = (Mathf.Cos((y - height * .5f) / (height * .5f) * Mathf.Pi * 3) + 1) * 0.5f;

				//calculate moisture
				float elevMoisture = (tile.elev >= 0) ? 1 - tile.elev : 1;

				float elevMoistureScale = 2;
				float cellMoistureScale = 1;
				float latTemperatureScale = 1;
				float moistureNoisModScale = 2;

				tile.moisture = (tile.cellMoisture * cellMoistureScale - (1 - tile.latTemperature) * latTemperatureScale
					+ tile.moistureNoise * moistureNoisModScale) / (cellMoistureScale + latTemperatureScale + moistureNoisModScale);
				if (tile.biome != FloorTileType.SeaIce && tile.biome != FloorTileType.Water)
				{
					tile.moisture *= Mathf.Pow(elevMoisture, elevMoistureScale);
				}

				if (tile.moisture < 0)
				{
					tile.moisture = 0;
					negativeMoistureCount++;

				}

				if (tile.elev > 0)
				{
					float snowLevel = 0.25f;
					float moistureScale = 1;
					float elevationScale = 3;

					float snowPossibility = (tile.moisture * moistureScale + tile.elev * elevationScale) / (moistureScale + elevationScale);
					if (snowPossibility >= snowLevel)
					{
						tile.biome = FloorTileType.snow;
						tile.speedMod = 0.20f;
						tile.baseSpeedMod = 0.2f;
						tile.recoveryRate = 0.002f;
						tile.maxSpeedMod = 1;
						//Console.WriteLine("tile biome set to snow");
					}
					else
					{
						tile.biome = FloorTileType.land;
						tile.speedMod = 0.60f;
						tile.baseSpeedMod = 0.6f;
						tile.recoveryRate = 0.001f;
						tile.maxSpeedMod = 1;
					}

					float iceLevel = 0.6f;
					float moistureIceScale = 1;
					float temperatureScale = 1;
					float elevationIceScale = .1f;

					float icePossibility = ((1 - tile.moisture) * moistureIceScale + (1 - tile.latTemperature) * temperatureScale + tile.elev * elevationIceScale) / (moistureIceScale + temperatureScale + elevationIceScale);

					if (icePossibility >= iceLevel)
					{
						tile.biome = FloorTileType.Glacier;
						tile.speedMod = 0.3f;
						tile.baseSpeedMod = 0.3f;
						tile.recoveryRate = 0.002f;
						tile.maxSpeedMod = 1;
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
						tile.biome = FloorTileType.SeaIce;
						tile.speedMod = 0.3f;
						tile.baseSpeedMod = 0.3f;
						tile.recoveryRate = 0.001f;
						tile.maxSpeedMod = 1;
					}
					else
					{
						tile.biome = FloorTileType.Water;
						tile.speedMod = 0.1f;
						tile.baseSpeedMod = 0.1f;
						tile.recoveryRate = 0.001f;
						tile.maxSpeedMod = 1;
					}
				}

				floorTiles[x][y] = tile;
				groundTiles[x][y] = new GroundTile(x, y);

				//calculate ores
				if (tile.biome == FloorTileType.land || tile.biome == FloorTileType.Glacier || tile.biome == FloorTileType.snow)
				{
					float goldOreNumber = goldOreNoise.GetNoise2D((x) * goldOreNoiseScale, y * goldOreNoiseScale);
					float goldOreProductionRate = (goldOreNumber - goldOreNoiseMin) * goldOreScale;

					if (goldOreProductionRate > 0)
					{
						// add gold ore resource
						MapResource resource = GetMapResource(MapResourceName.GoldOre);
						tile.resources.Add(MapResourceData.CreateMinable(resource, goldOreProductionRate));
						if (goldOreProductionRate > resource.resourceMax)
						{
							resource.resourceMax = goldOreProductionRate;
						}
					}

					float ironOreNumber = ironOreNoise.GetNoise2D((x) * ironOreNoiseScale, y * ironOreNoiseScale);
					float ironOreProductionRate = (ironOreNumber - ironOreNoiseMin) * ironOreScale;

					if (ironOreProductionRate > 0)
					{
						// add iron ore resource
						MapResource resource = GetMapResource(MapResourceName.IronOre);
						tile.resources.Add(MapResourceData.CreateMinable(resource, ironOreProductionRate));
						if (ironOreProductionRate > resource.resourceMax)
						{
							resource.resourceMax = ironOreProductionRate;
						}
					}
				}
			}
		}
		if (negativeMoistureCount > 0)
		{
			Console.WriteLine("WARNING: negative moisture count: " + negativeMoistureCount);
		}
		GenerateRivers();
		GenerateGroundObjects();
		GenerateResources();
		//GenerateCities(cityCount);
		GeneratePeople();
		map.GenerateMap();
		worldMap.GenerateMap();
		mapResources.ForEach(x => Console.WriteLine($"{x.Name}: {x.resourceMax}"));
	}

	private void GeneratePeople()
	{
		for (int i = 0; i < PEOPLE_COUNT; i++)
		{
			Vector2? position = GetRandomPositionOnMap([FloorTileType.land]);
			if (position.HasValue)
			{
				GeneratePerson(position.Value);
			}
		}
	}

	private Vector2? GetRandomPositionOnMap(List<FloorTileType> floorTileTypes, bool onRiverTile = false)
	{
		const int RANDOM_CHECK_LIMIT = 1000;

		int count = 0;
		while (count < RANDOM_CHECK_LIMIT)
		{
			count++;

			float randomX = random.Randf() * width * tileSize;
			float randomY = random.Randf() * height * tileSize;

			Vector2 position = new(randomX, randomY);

			FloorTile tile = GetTile(position);

			if (floorTileTypes.Contains(tile.biome) && tile.hasRiver == onRiverTile)
			{
				return position;
			}

		}
		return null;
	}

	private void GenerateCities(int num)
	{
		DeleteCities();

		cities = new List<City>();
		int tries = 0;
		while (tries < 1000 && num > 0)
		{
			int randomX = random.RandiRange(0, width - 1);
			int randomY = random.RandiRange(0, height - 1);
			//Console.WriteLine($"randomX:{randomX}, randomY: {randomY}, width:{tiles.Length}, height:{tiles[0].Length}");
			if (floorTiles[randomX][randomY].biome != FloorTileType.Water && floorTiles[randomX][randomY].biome != FloorTileType.SeaIce)
			{
				City city = cityPrefab.Instantiate<City>();
				AddChild(city);
				cities.Add(city);
				city.Name = "city" + num;
				city.name = city.Name;
				Vector2 pos = new Vector2((randomX) * tileSize, (randomY) * tileSize);
				city.Position = (pos);
				city.GenerateCity(this, floorTiles[randomX][randomY], 5);
				tries = 0;
				num--;
				//Console.WriteLine($"Created {city.name}");
			}
			else
			{
				tries++;
			}
		}
	}

	private void GeneratePerson(Vector2 position)
	{
		PersonData personData = GeneratePersonData(position);
		Person person = personPrefab.Instantiate<Person>();
		person.Position = (position);
		person.personData = personData;

		if (person == null)
		{
			Debug.WriteLine("person is null");
		}
		else
		{
			AddChild(person);
		}
	}

	private PersonData GeneratePersonData(Vector2 position)
	{
        PersonData person = new PersonData();
        person.id = Random.Randi().ToString();
        people[person.id] = person;
		person.position = position;

        if (Random.Randf() < 0.5f)
        {
            person.gender = Gender.male;
            person.firstName = peopleNames.GenerateFirstName(Sex.Male);
        }
        else
        {
            person.gender = Gender.female;
            person.firstName = peopleNames.GenerateFirstName(Sex.Female);
        }
        person.lastName = peopleNames.GenerateLastName();
        int age = Random.RandiRange(18, 50);
        person.birthDate = Time - GDate.Year * age;
		return person;
    }

	public void GenerateRivers()
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				//Console.WriteLine($"x: {x} y: {y}");
				FloorTile tile = floorTiles[x][y];
				if (random.Randf() <= Mathf.Pow(tile.moisture, riverFormationMoistureScale) && random.Randf() <= riverSpawnProbability && (tile.biome == FloorTileType.land || tile.biome == FloorTileType.snow) && !tile.hasRiver && !tile.hasWaterBasin)
				{
					this.GenerateRiver(tile);
				}
			}
		}

		riverDisplay.GenerateRivers();
	}

	private void GenerateRiver(FloorTile tile)
	{
		Vector2 previousPosition = Vector2.Zero;

		int count = 0;
		//Console.WriteLine($"{tile.position.ToString()} starting river");
		while (true)
		{
			count++;
			//check if tile already has river
			if (tile.hasWaterBasin)
			{
				//Console.WriteLine($"{tile.position.ToString()} already has basin");
				tile.waterBasinSize += rivers[$"{previousPosition}"].size * riverMocementMoistureCarryScale;
				//set new tile attributes

				break;
			}
			else if (tile.hasRiver)
			{
				//Console.WriteLine($"{tile.position.ToString()} already has river data");

				int num = 0;
				float riverMoistureCarry = rivers[$"{previousPosition}"].size * riverMocementMoistureCarryScale;
				RiverData riverData = rivers[$"{tile.position}"];
				riverData.fromPositions.Add(previousPosition);
				while (true)
				{
					num++;
					if (tile.hasWaterBasin)
					{
						tile.waterBasinSize += riverMoistureCarry;
						//set new tile attributes
						break;
					}
					else if (tile.biome == FloorTileType.land || tile.biome == FloorTileType.snow)
					{
						riverData = rivers[$"{tile.position}"];
						riverData.size += riverMoistureCarry;
						riverMoistureCarry *= riverMocementMoistureCarryScale;

						//set new tile attributes

						//Console.WriteLine($"FloorTile speedMod: {tile.speedMod}");
						tile = GetTile(riverData.toPosition);
					}
					else
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
			else
			{
				//try to create new river data
				if (tile.biome == FloorTileType.land || tile.biome == FloorTileType.snow)
				{
					//spawn stream
					//Console.WriteLine($"{tile.position.ToString()} creating possible river data");

					FloorTile toTile = null;
					float distanceDiff = 0;
					foreach (FloorTile neighbor in GetNeighbors(tile, floorTiles, noDiagonals: true))
					{
                        float neighborDiff = neighbor.elev - tile.elev;
                        if (neighborDiff < distanceDiff)
                        {
                            distanceDiff = neighborDiff;
                            toTile = neighbor;
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
						riverData.fromPositions.ForEach(fromPosition => riverData.size += rivers[$"{fromPosition.ToString()}"].size * riverMocementMoistureCarryScale);
						//Console.WriteLine($"river size: {riverData.size}");
						riverData.size += tile.moisture * riverMovementMoistureScale;

						previousPosition = tile.position;
						riverData.toPosition = toTile.position;
						//TODO: calculate speed

						//set new tile attributes

						rivers[tile.position.ToString()] = riverData;
						tile.hasRiver = true;
						tile = toTile;
					}
					else
					{
						//Console.WriteLine($"{tile.position.ToString()} creating new river basin");
						tile.hasWaterBasin = true;
						tile.waterBasinSize += 1;

						//set new tile attributes
						break;
					}

				}
				else
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

	private void DeleteCities()
	{
		for (int i = cities.Count - 1; i >= 0; i--)
		{
			cities[i].Free();
		}
	}

	public FloorTile GetTile(Vector2 position)
	{
		int x = Mathf.FloorToInt(position.X / tileSize);
		int y = Mathf.FloorToInt(position.Y / tileSize);

		return floorTiles[x][y];
	}

	public MapResource GetMapResource(string mapResource)
	{
		return mapResources.Find(x => x.Name == mapResource);
	}

	public void UpdateDisplayMode(DisplayMode displayMode)
	{
		mapDisplayMode = displayMode;
		map.GenerateMap();
	}

	private void CreateMapResources()
	{
		MapResource mapResource = new MapResource(MapResourceName.Dear, new List<string>(), ResourceType.Rooted, 0.001f, 0);
		displayMenu.AddToDisplayMenu(mapResource.Name);
		mapResources.Add(mapResource);

		mapResource = new MapResource(MapResourceName.FreshWaterFish, new List<string>(), ResourceType.Rooted, 0.005f, 0);
		displayMenu.AddToDisplayMenu(mapResource.Name);
		mapResources.Add(mapResource);

		mapResource = new MapResource(MapResourceName.SeaFish, new List<string>(), ResourceType.Rooted, 0.01f, 0);
		displayMenu.AddToDisplayMenu(mapResource.Name);
		mapResources.Add(mapResource);

		mapResource = new MapResource(MapResourceName.Tree, new List<string>(), ResourceType.Rooted, 0.0001f, 0);
		displayMenu.AddToDisplayMenu(mapResource.Name);
		mapResources.Add(mapResource);

		mapResource = new MapResource(MapResourceName.FruitTree, new List<string>(), ResourceType.Rooted, 0.0001f, 0);
		displayMenu.AddToDisplayMenu(mapResource.Name);
		mapResources.Add(mapResource);

		mapResource = new MapResource(MapResourceName.BerryBush, new List<string>(), ResourceType.Rooted, 0.0001f, 0);
		displayMenu.AddToDisplayMenu(mapResource.Name);
		mapResources.Add(mapResource);

		mapResource = new MapResource(MapResourceName.Rock, new List<string>(), ResourceType.Minable, 0, 0.95f);
		displayMenu.AddToDisplayMenu(mapResource.Name);
		mapResources.Add(mapResource);

		mapResource = new MapResource(MapResourceName.GoldOre, new List<string>(), ResourceType.Minable, 0, 0.9f);
		displayMenu.AddToDisplayMenu(mapResource.Name);
		mapResources.Add(mapResource);

		mapResource = new MapResource(MapResourceName.IronOre, new List<string>(), ResourceType.Minable, 0, 0.92f);
		displayMenu.AddToDisplayMenu(mapResource.Name);
		mapResources.Add(mapResource);
	}

	public void DisplayMapResource(MapResource mapResource)
	{
		mapDisplayMode = DisplayMode.Resource;
		map.DisplayMapResource(mapResource);
	}

	private void GenerateGroundObjects()
	{
		var groundObjectsSpawners = GroundObjectFactory.GetGroundObjectSpawnings();
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				FloorTile floorTile = floorTiles[x][y];
				GroundTile groundTile = groundTiles[x][y];

				float averageElevationDiff = GetAverageElevationDiff(floorTile);

				string spawnObjName = "";
				float maxSpawnObjProbability = float.MinValue;

				foreach (GroundObjectSpawning spawner in groundObjectsSpawners)
				{
					if (
						(spawner.FloorTypesAreInclusive && !spawner.FloorTileTypes.Contains(floorTile.biome)) ||
						(!spawner.FloorTypesAreInclusive && spawner.FloorTileTypes.Contains(floorTile.biome)))
					{
						continue;
					}

					if (!spawner.SpawnOnRiver && floorTile.hasRiver)
					{
						continue;
					}

					float spawnObjProbability = 1;
					if (spawner.MoistureSpawnRate.HasValue)
					{
						SpawnRate spawnRate = spawner.MoistureSpawnRate.Value;
						spawnObjProbability *= (Math.Min(floorTile.moisture, spawnRate.Max) - spawnRate.Min) * spawnRate.Rate;
					}
					if (spawner.ElevDiffSpawnRate.HasValue)
					{
						SpawnRate spawnRate = spawner.ElevDiffSpawnRate.Value;
						spawnObjProbability *= (Math.Min(averageElevationDiff, spawnRate.Max) - spawnRate.Min) * spawnRate.Rate;
					}

					if (spawnObjProbability > maxSpawnObjProbability)
					{
						maxSpawnObjProbability = spawnObjProbability;
						spawnObjName = spawner.Name;
					}
				}

				if (maxSpawnObjProbability < random.Randf())
				{
					continue;
				}

				switch (spawnObjName)
				{
					case "Tree":
						GenerateTree(groundTile);
						break;
					case "Rock":
						GenerateRock(groundTile);
						break;
					default:
						throw new Exception($"Unkown spawnObjName {spawnObjName}.");
				}
			}
		}
	}

	private void GenerateResources()
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				//Console.WriteLine($"x: {x} y: {y}");
				FloorTile tile = floorTiles[x][y];
				GenerateAnimals(tile);
			}
		}
	}

	private void GenerateAnimals(FloorTile tile)
	{
		if (tile.biome == FloorTileType.Water)
		{
			if (seaFishProbability >= random.Randf())
			{
				float maxAmount = Mathf.Floor(-tile.elev * seaFishScale);
				MapResource resource = GetMapResource(MapResourceName.SeaFish);
				tile.resources.Add(MapResourceData.CreateLiving(resource, maxAmount));
				if (maxAmount > resource.resourceMax)
				{
					resource.resourceMax = maxAmount;
				}
			}
		}
		else if (tile.biome == FloorTileType.land || tile.biome == FloorTileType.snow)
		{
			if (dearProbability * tile.moisture >= random.Randf())
			{
				float maxAmount = Mathf.Floor(tile.moisture * dearScale);
				MapResource resource = GetMapResource(MapResourceName.Dear);
				tile.resources.Add(MapResourceData.CreateLiving(resource, maxAmount));
				if (maxAmount > resource.resourceMax)
				{
					resource.resourceMax = maxAmount;
				}
			}

			if (tile.hasWaterBasin && tile.waterBasinSize >= freshWaterFishMinSize)
			{
				float maxAmount = Mathf.Floor((tile.waterBasinSize - freshWaterFishMinSize) * freshWaterFishScale);
				if (maxAmount > 0)
				{
					MapResource resource = GetMapResource(MapResourceName.FreshWaterFish);
					tile.resources.Add(MapResourceData.CreateLiving(resource, maxAmount));
					if (maxAmount > resource.resourceMax)
					{
						resource.resourceMax = maxAmount;
					}
				}
			}
			else if (tile.hasRiver && freshWaterFishProbability >= random.Randf())
			{
				RiverData riverData = rivers[tile.position.ToString()];

				float maxAmount = Mathf.Floor((riverData.size - freshWaterFishMinSize) * freshWaterFishScale);
				if (maxAmount > 0)
				{
					MapResource resource = GetMapResource(MapResourceName.FreshWaterFish);
					tile.resources.Add(MapResourceData.CreateLiving(resource, maxAmount));
					if (maxAmount > resource.resourceMax)
					{
						resource.resourceMax = maxAmount;
					}
				}
			}
		}
	}

	private static void GenerateRock(GroundTile tile)
	{
		tile.AddRock();
	}

	private float GetAverageElevationDiff(FloorTile tile)
	{
		float averageElevDiff = 0;
		List<FloorTile> neighbors = GetNeighbors(tile, floorTiles);



        foreach (FloorTile neighbor in neighbors)
		{
            averageElevDiff += Mathf.Abs(neighbor.elev - tile.elev);
        }

		averageElevDiff /= neighbors.Count;
		return averageElevDiff;
	}

    private List<T> GetNeighbors<T>(T tile, T[][] tiles, bool noDiagonals = false) where T : Tile
    {
        List<T> neighbors = [];

        for (int nx = -1; nx <= 1; nx += 1)
        {
            for (int ny = -1; ny <= 1; ny += 1)
            {
                if (nx == 0 && ny == 0)
                    continue;

				if (noDiagonals && ((nx == -1 && ny == 1) || nx == ny || (nx == 1 && ny == -1)))
				{
					continue;
				}

                int checkX = tile.coord.X + nx;
                int checkY = tile.coord.Y + ny;

                if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
                {
                    neighbors.Add(tiles[checkX][checkY]);
                }
            }
        }

        return neighbors;
    }

    private static void GenerateTree(GroundTile tile)
	{
		tile.AddTree();
	}
}

public partial class RiverData
{
	public string name;
	public Vector2 position;
	public Vector2 toPosition;
	public float size;
	public float speed;
	public List<Vector2> fromPositions = new List<Vector2>();
}


