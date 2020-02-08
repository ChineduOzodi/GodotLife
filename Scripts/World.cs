using Godot;
using Life.Scripts.Classes;
using Life.Scripts.Pathfinding;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using RandomNameGen;

public class World : Node2D
{
	public const float HourDuration = 60; // in seconds
	public const float DayDuration = HourDuration * 24;
	public const float YearDuration = HourDuration * 24;
	public const float TileScale = 1; //in km
	public const float TileArea = TileScale * TileScale; //in km^2
	public static World Instance;
	public int idGen = 1;

	public RandomName peopleNames;
	public CityNames cityNames;
	

	public Tile[][] tiles;
	public List<City> cities = new List<City>();
	public Dictionary<String,PersonData> people = new Dictionary<string, PersonData>();
	public Dictionary<String, RiverData> rivers = new Dictionary<string, RiverData>();
	public List<MapResource> mapResources = new List<MapResource>();
	public Dictionary<String, Building> buildings = new Dictionary<string, Building>();

	private Tile hoveredTile = null;

	private GDate gDate = new GDate(2000, 0, 0, 0, 0, 0);
	private int xLimit;
	private int yLimit;
	private float elevChangeCost = 500;
	
	
	private DrawingLine drawingLine;
	private DisplayMenu displayMenu;
	private Map map;
	private Rivers riverDisplay;
	private MapResourceOverlay mapResourceOverlay;
	private DisplayMode mapDisplayMode = DisplayMode.Normal;
	private int xOffset;
	private int yOffset;
	private RandomNumberGenerator random = new RandomNumberGenerator();
	private Person nearestPerson;
	float nearestPersonDistanceSquared;
	private float mapUpdateInterval = 5;
	private double mapLastUpdate;


	//Map Generation Variables
	private int tileSize = 100;
	private int width = 300;
	private int height = 200;
	private float waterLevel = 0.5f;
	private int cityCount = 30;
	private int peopleCount = 20;
	private float riverSpawnProbability = .5f;
	private float riverFormationMoistureScale = 3f; //greater number = less river formation for less moisture places;
	private float riverMovementMoistureScale = 1;
	private float riverMocementMoistureCarryScale = .9f;
	private float riverCrossingDifficultyScale = 11f;
	private float riverCrossingDifficultyMax = 0.01f;
	private float elevationNoiseScale = 1f;
	private float moistureNoiseScale = .75f;
	private float goldOreNoiseScale = 20f;
	private float goldOreNoiseMin = .6f;
	private float goldOreScale = 0.5f;
	private float ironOreNoiseScale = 10f;
	private float ironOreNoiseMin = 0.5f;
	private float ironOreScale = 1f;
	private float rockElevationDiffScale = 1f;
	private float rockMinElevationDiff = 0.005f;
	private float treeMinMoisture = 0.2f;
	private float treeMoistureScale = 1000;
	private float dearProbability = 0.5f;
	private float dearScale = 50;
	private float freshWaterFishProbability = 1f;
	private float freshWaterFishMinSize = 2f;
	private float freshWaterFishScale = 20f;
	private float seaFishProbability = 1f;
	private float seaFishScale = 100f;

	

	private OpenSimplexNoise moistureNoise = new OpenSimplexNoise();
	private OpenSimplexNoise elevationNoise = new OpenSimplexNoise();
	private OpenSimplexNoise ironOreNoise = new OpenSimplexNoise();
	private OpenSimplexNoise goldOreNoise = new OpenSimplexNoise();

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
	public double Time { get => gDate.time; }
	public Person NearestPerson { get => nearestPerson; }
	public float NearestPersonDistanceSquared { get => nearestPersonDistanceSquared; }
	public float ElevChangeCost { get => elevChangeCost; }
	public DisplayMode MapDisplayMode { get => mapDisplayMode; }
	public Tile HoveredTile { get => hoveredTile; set => hoveredTile = value; }
	public GDate GDate { get => gDate; }
	public Camera2D camera;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
		cityPrefab = ResourceLoader.Load<PackedScene>("res://Prefabs/City.tscn");
		personPrefab = ResourceLoader.Load<PackedScene>("res://Prefabs/Person.tscn");
		map = GetNode<Map>(new NodePath("Map"));
		riverDisplay = GetNode<Rivers>(new NodePath("Rivers"));
		displayMenu = GetNode<DisplayMenu>(new NodePath("CanvasLayer/DisplayMenu"));
		drawingLine = GetNode<DrawingLine>(new NodePath("DrawingLine"));
		mapResourceOverlay = GetNode<MapResourceOverlay>(new NodePath("MapResourceOverlay"));
		camera = GetNode<Camera2D>("RTS-Camera2D");
		CreateMapResources();
		GenerateWorld(width, height);
	}

	



	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		gDate.AddTime((double) delta);

		//if (mapLastUpdate + mapUpdateInterval < time && (mapDisplayMode == DisplayMode.Normal || mapDisplayMode == DisplayMode.WalkingSpeed))
		//{
		//    mapLastUpdate = time;
		//    map.GenerateMap();
		//}
		//else
		//{
		//    Console.WriteLine($"{mapLastUpdate + mapUpdateInterval} < {time}, delta: {(double)delta}");
		//}

		// show hovered over Tile
		Vector2 mousePosition = GetGlobalMousePosition();
		Coord mouseTileCoord = new Coord( (int) ((mousePosition.x + tileSize * 0.5f) / tileSize - xOffset ), (int) ((mousePosition.y + tileSize * 0.5f) / tileSize - yOffset));
		if (mouseTileCoord.x >= 0 && mouseTileCoord.y >= 0 && mouseTileCoord.x < width && mouseTileCoord.y < height)
		{
			hoveredTile = tiles[mouseTileCoord.x][mouseTileCoord.y];
		} else
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

	public void GenerateWorld(int width, int height)
	{
		xOffset = -width / 2;
		yOffset = -height / 2;
		xLimit = (width + xOffset) * tileSize;
		yLimit = (height + yOffset) * tileSize;
		moistureNoise.Seed = (DateTime.Now.TimeOfDay.Milliseconds + "moisture".GetHashCode());
		goldOreNoise.Seed =(DateTime.Now.TimeOfDay.Milliseconds + "goldOre".GetHashCode());
		ironOreNoise.Seed =(DateTime.Now.TimeOfDay.Milliseconds + "ironOre".GetHashCode());
		elevationNoise.Seed =(DateTime.Now.TimeOfDay.Milliseconds + "elevation".GetHashCode());
		elevationNoise.Octaves = 20;
		random.Seed = (ulong) DateTime.Now.ToBinary();
		int negativeMoistureCount = 0;
		tiles = new Tile[width][];
		for (int x = 0; x < width; x++)
		{
			tiles[x] = new Tile[height];
			for (int y = 0; y < height; y++)
			{
				Tile tile = new Tile();
				tile.coord = new Coord(x, y);
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

				float elevMoistureScale = 2;
				float cellMoistureScale = 1;
				float latTemperatureScale = 1;
				float moistureNoisModScale = 2;

				tile.moisture = (tile.cellMoisture * cellMoistureScale - ( 1 - tile.latTemperature) * latTemperatureScale
					+ tile.moistureNoise * moistureNoisModScale) / (cellMoistureScale + latTemperatureScale + moistureNoisModScale);
				if (tile.biome!= TileType.SeaIce && tile.biome != TileType.Water)
				{
					tile.moisture *= Mathf.Pow(elevMoisture,elevMoistureScale);
				}
				tile.position = new Vector2((x + xOffset) * tileSize, (y + yOffset) * tileSize);

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
						tile.biome = TileType.snow;
						tile.speedMod = 0.20f;
						tile.baseSpeedMod = 0.2f;
						tile.recoveryRate = 0.002f;
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
						tile.biome = TileType.Glacier;
						tile.speedMod = 0.3f;
						tile.baseSpeedMod = 0.3f;
						tile.recoveryRate = 0.002f;
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
						tile.biome = TileType.SeaIce;
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

				//calculate ores
				if (tile.biome == TileType.land || tile.biome == TileType.Glacier || tile.biome == TileType.snow)
				{
					float goldOreNumber = goldOreNoise.GetNoise2d((x + xOffset) * goldOreNoiseScale, (y + yOffset) * goldOreNoiseScale);
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

					float ironOreNumber = ironOreNoise.GetNoise2d((x + xOffset) * ironOreNoiseScale, (y + yOffset) * ironOreNoiseScale);
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
		GenerateResources();
		GenerateCities(cityCount);
		map.GenerateMap();
		mapResources.ForEach(x => Console.WriteLine($"{x.Name}: {x.resourceMax}"));
	}

	

	private void GenerateCities(int num)
	{
		DeleteCities();
		Random r = new Random((int) (DateTime.Now.ToBinary() + Random.Randi()));
		peopleNames = new RandomName(r);
		cityNames = new CityNames(r);

		cities = new List<City>();
		int tries = 0;
		while(tries < 1000 && num > 0)
		{
			int randomX = random.RandiRange(0, width - 1);
			int randomY = random.RandiRange(0, height - 1);
			//Console.WriteLine($"randomX:{randomX}, randomY: {randomY}, width:{tiles.Length}, height:{tiles[0].Length}");
			if (tiles[randomX][randomY].biome != TileType.Water && tiles[randomX][randomY].biome != TileType.SeaIce)
			{
				City city = (City) cityPrefab.Instance();
				AddChild(city);
				cities.Add(city);
				city.Name = "city" + num;
				city.name = city.Name;
				Vector2 pos = new Vector2((randomX + xOffset) * tileSize, (randomY + yOffset) * tileSize);
				city.Position = (pos);
				city.GenerateCity(this, tiles[randomX][randomY], 5);
				tries = 0;
				num--;
				//Console.WriteLine($"Created {city.name}");
				for (int i = 0; i < peopleCount; i++)
				{
					//GeneratePerson(pos);
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
		person.Position = (position);
		if (person == null)
		{
			Debug.WriteLine("person is null");
		} else
		{
			AddChild(person);
			//people.Add(person);
		}
	}

	public void GenerateRivers()
	{
		int negativeNumberCorrectionCount = 0;
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				//Console.WriteLine($"x: {x} y: {y}");
				Tile tile = tiles[x][y];
				Vector2 previousPosition = Vector2.Zero;
				int count = 0;
				if (random.Randf() <= Mathf.Pow(tile.moisture,riverFormationMoistureScale) && random.Randf() <= riverSpawnProbability && (tile.biome == TileType.land || tile.biome == TileType.snow) && !tile.hasRiver && !tile.hasWaterBasin)
				{
					//Console.WriteLine($"{tile.position.ToString()} starting river");
					while (true)
					{
						count++;
						//check if tile already has river
						if (tile.hasWaterBasin)
						{
							//Console.WriteLine($"{tile.position.ToString()} already has basin");
							tile.waterBasinSize += rivers[$"{previousPosition.ToString()}"].size * riverMocementMoistureCarryScale;
							//set new tile attributes
							float riverCrossingDifficulty = (1 * riverCrossingDifficultyScale - tile.waterBasinSize) / riverCrossingDifficultyScale;
							if (riverCrossingDifficulty < riverCrossingDifficultyMax)
							{
								negativeNumberCorrectionCount++;
							}
							tile.riverCrossingSpeed = (riverCrossingDifficulty <= riverCrossingDifficultyMax) ? riverCrossingDifficultyMax : riverCrossingDifficulty;
							break;
						}
						else if(tile.hasRiver)
						{
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
										negativeNumberCorrectionCount++;
									}
									tile.riverCrossingSpeed = (riverCrossingDifficulty <= riverCrossingDifficultyMax) ? riverCrossingDifficultyMax : riverCrossingDifficulty;
									break;
								}
								else if (tile.biome == TileType.land || tile.biome == TileType.snow)
								{
									riverData = rivers[$"{tile.position.ToString()}"];
									riverData.size += riverMoistureCarry;
									riverMoistureCarry *= riverMocementMoistureCarryScale;

									//set new tile attributes
									float riverCrossingDifficulty = (1 * riverCrossingDifficultyScale - riverData.size) / riverCrossingDifficultyScale;
									if (riverCrossingDifficulty < riverCrossingDifficultyMax)
									{
										negativeNumberCorrectionCount++;
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
									//Console.WriteLine($"Tile speedMod: {tile.speedMod}");
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
							if (tile.biome == TileType.land || tile.biome == TileType.snow)
							{
								//spawn stream
								//Console.WriteLine($"{tile.position.ToString()} creating possible river data");
								
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
										negativeNumberCorrectionCount++;
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

									rivers[tile.position.ToString()] = riverData;
									tile.hasRiver = true;
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
										negativeNumberCorrectionCount++;
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
		if (negativeNumberCorrectionCount > 0)
		{
			Console.WriteLine($"INFO: crossing difficulty above crossing threshold count: {negativeNumberCorrectionCount}");
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
		int y = pos.y / tileSize - yOffset;

		return tiles[x][y];
	}

	public MapResource GetMapResource( string mapResource)
	{
		return mapResources.Find(x => x.Name == mapResource);
	}

	public void UpdateDisplayMode( DisplayMode displayMode)
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

	private void GenerateResources()
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				//Console.WriteLine($"x: {x} y: {y}");
				Tile tile = tiles[x][y];
				GenerateRock(tile);
				GenerateTrees(tile);
				GenerateAnimals(tile);
			}
		}
	}

	private void GenerateAnimals(Tile tile)
	{
		if (tile.biome == TileType.Water)
		{
			if (seaFishProbability  >= random.Randf())
			{
				float maxAmount = Mathf.Floor(-tile.elev * seaFishScale);
				MapResource resource = GetMapResource(MapResourceName.SeaFish);
				tile.resources.Add(MapResourceData.CreateLiving(resource, maxAmount));
				if (maxAmount > resource.resourceMax)
				{
					resource.resourceMax = maxAmount;
				}
			}
		} else if (tile.biome == TileType.land || tile.biome == TileType.snow)
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
			} else if (tile.hasRiver && freshWaterFishProbability >=  random.Randf())
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

	private void GenerateRock(Tile tile)
	{
		//set rock resource
		if (tile.biome != TileType.Water && tile.biome != TileType.SeaIce)
		{
			int neighbors = 0;
			float averageElevDiff = 0;

			for (int nx = -tileSize; nx <= tileSize; nx += tileSize)
			{
				for (int ny = -tileSize; ny <= tileSize; ny += tileSize)
				{
					if (nx == 0 && ny == 0)
						continue;

					float checkX = tile.position.x + nx;
					float checkY = tile.position.y + ny;

					if (checkX >= -xLimit && checkX < xLimit && checkY >= -yLimit && checkY < yLimit)
					{
						Tile neighborTile = GetTile(new Vector2(checkX, checkY));
						averageElevDiff += Mathf.Abs(neighborTile.elev - tile.elev);
						neighbors++;
					}
				}
			}

			averageElevDiff /= neighbors;

			float rockProdutionRate = averageElevDiff * rockElevationDiffScale - rockMinElevationDiff;
			if (rockProdutionRate > 0)
			{
				// add rock resource
				//Console.WriteLine("generated rock");
				MapResource resource = GetMapResource(MapResourceName.Rock);
				tile.resources.Add(MapResourceData.CreateMinable(resource, rockProdutionRate));
				if (rockProdutionRate > resource.resourceMax)
				{
					resource.resourceMax = rockProdutionRate;
				}
			}
		}
		
	}

	private void GenerateTrees(Tile tile)
	{
		//set tree resource
		if (tile.biome != TileType.Water && tile.biome != TileType.SeaIce)
		{
			
			float maxAmount = Mathf.Floor((tile.moisture - treeMinMoisture) * treeMoistureScale)  * Tile.subTileWidthHeight * Tile.subTileWidthHeight;
			if (maxAmount > 0)
			{
				// add tree resources
				MapResource resource = GetMapResource(MapResourceName.Tree);
				tile.resources.Add(MapResourceData.CreateRooted(resource,maxAmount));
				if (maxAmount > resource.resourceMax)
				{
					resource.resourceMax = maxAmount;
				}
			}

			////create fruit tree
			//maxAmount *= 0.2f;
			//if (maxAmount > 0)
			//{
			//    //create fruit tree
			//    MapResource resource = GetMapResource(MapResourceName.FruitTree);

			//    tile.resources.Add(MapResourceData.CreateRooted(resource, Mathf.Floor(maxAmount * 0.2f) * Tile.subTileWidthHeight * Tile.subTileWidthHeight));
			//    if (maxAmount > resource.resourceMax)
			//    {
			//        resource.resourceMax = maxAmount;
			//    }
			//}

			////create berry bushes
			//maxAmount *= 2f;
			//if (maxAmount > 0)
			//{
			//    MapResource resource = GetMapResource(MapResourceName.BerryBush);

			//    tile.resources.Add(MapResourceData.CreateRooted(resource, Mathf.Floor(maxAmount * 0.2f) * Tile.subTileWidthHeight * Tile.subTileWidthHeight));
			//    if (maxAmount > resource.resourceMax)
			//    {
			//        resource.resourceMax = maxAmount;
			//    }
			//}
		}

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


