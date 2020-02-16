using Godot;
using Life.Scripts.Classes;
using RandomNameGen;
using System;
using System.Collections.Generic;

public class City : Node2D
{
	//city generation
	private World world;
	private const int familyPerCity = 50;
	private const int minAdultAge = 18;
	private const int maxAdultAge = 60;
	private const float marriedPercent = .5f;
	private const float marriedAgeDifferenceMax = 10;
	private const float childrenAgeSubtractionMin = 16;
	private const float grandparentPercent = .5f;
	private const float grandparentAgeAdditionMin = 16;
	private const float grandparentAgeMax = 100;
	private const float percentChanceOwnBusiness = 0.1f;
	private const float bussinessOwnerPercent = 0.2f;

	private Label cityNameMedium;
	private Label cityNameLarge;

	public string name;
	public string id;
	public List<String> buildings = new List<string>();
	public List<Coord> tileCoords = new List<Coord>();
	public List<String> peopleIds = new List<string>();
	public int Population { get => peopleIds.Count; }
	


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		cityNameMedium = GetNode("cityNameMedium") as Label;
		cityNameLarge = GetNode("cityNameLarge") as Label;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		if (World.Instance != null)
		{
			if (World.Instance.camera.Zoom.x > 5)
			{
				cityNameLarge.Show();
				cityNameMedium.Hide();
			} else if (World.Instance.camera.Zoom.x > 1)
			{
				cityNameLarge.Hide();
				cityNameMedium.Show();
			} else
			{
				cityNameLarge.Hide();
				cityNameMedium.Hide();
			}
		}
	}

	public void GenerateCity(World world, Tile tile, int numFamilies)
	{
		this.world = world;
		tileCoords.Add(tile.coord);
		tile.GenerateProperties();

		name = world.cityNames.Generate("US");
		id = world.Random.Randi().ToString();
		tile.cityId = id;
		cityNameMedium.Text = name;
		cityNameLarge.Text = name;

		//get river directions
		if (World.Instance.rivers.ContainsKey(tile.position.ToString()))
		{
			RiverData river = World.Instance.rivers[tile.position.ToString()];
			Vector2 toMovement = river.toPosition - river.position;
			List<Vector2> fromMovements = new List<Vector2>();
			for (int i = 0; i < river.fromPositions.Count; i++)
			{
				fromMovements.Add(river.fromPositions[i] - river.position);
			}

			for (int x = 0; x < Tile.subTileWidthHeight; x++)
			{
				for (int y = 0; y < Tile.subTileWidthHeight; y++)
				{
					Property property = tile.properties[x][y];
					if (x == Tile.subTileWidthHeight / 2 && y == Tile.subTileWidthHeight / 2)
					{
						property.hasRiver = true;
					} else if (toMovement.x > 0 && toMovement.y == 0 && x >= Tile.subTileWidthHeight / 2 && y == Tile.subTileWidthHeight / 2 )
					{
						property.hasRiver = true;
					}
					else if (toMovement.x < 0 && toMovement.y == 0 && x <= Tile.subTileWidthHeight / 2 && y == Tile.subTileWidthHeight / 2)
					{
						property.hasRiver = true;
					}
					else if (toMovement.x == 0 && toMovement.y > 0 && x == Tile.subTileWidthHeight / 2 && y >= Tile.subTileWidthHeight / 2)
					{
						property.hasRiver = true;
					}
					else if (toMovement.x == 0 && toMovement.y < 0 && x == Tile.subTileWidthHeight / 2 && y <= Tile.subTileWidthHeight / 2)
					{
						property.hasRiver = true;
					}
					else if (toMovement.x > 0 && toMovement.y > 0 && x >= Tile.subTileWidthHeight / 2 && y >= Tile.subTileWidthHeight / 2 && Tile.subTileWidthHeight - y - 1 == x)
					{
						property.hasRiver = true;
					}
					else if (toMovement.x < 0 && toMovement.y < 0 && x <= Tile.subTileWidthHeight / 2 && y <= Tile.subTileWidthHeight / 2 && Tile.subTileWidthHeight - y - 1 == x)
					{
						property.hasRiver = true;
					}
					else if (toMovement.x > 0 && toMovement.y < 0 && x >= Tile.subTileWidthHeight / 2 && y <= Tile.subTileWidthHeight / 2 && Tile.subTileWidthHeight - x - 1 == y)
					{
						property.hasRiver = true;
					}
					else if (toMovement.x < 0 && toMovement.y > 0 && x <= Tile.subTileWidthHeight / 2 && y >= Tile.subTileWidthHeight / 2 && Tile.subTileWidthHeight - x - 1 == y)
					{
						property.hasRiver = true;
					}

					for (int r = 0; r < fromMovements.Count; r++)
					{
						Vector2 fromMovement = fromMovements[r];

						if (x == Tile.subTileWidthHeight / 2 && y == Tile.subTileWidthHeight / 2)
						{
							property.hasRiver = true;
						}
						else if (fromMovement.x > 0 && fromMovement.y == 0 && x >= Tile.subTileWidthHeight / 2 && y == Tile.subTileWidthHeight / 2)
						{
							property.hasRiver = true;
						}
						else if (fromMovement.x < 0 && fromMovement.y == 0 && x <= Tile.subTileWidthHeight / 2 && y == Tile.subTileWidthHeight / 2)
						{
							property.hasRiver = true;
						}
						else if (fromMovement.x == 0 && fromMovement.y > 0 && x == Tile.subTileWidthHeight / 2 && y >= Tile.subTileWidthHeight / 2)
						{
							property.hasRiver = true;
						}
						else if (fromMovement.x == 0 && fromMovement.y < 0 && x == Tile.subTileWidthHeight / 2 && y <= Tile.subTileWidthHeight / 2)
						{
							property.hasRiver = true;
						}
						else if (fromMovement.x > 0 && fromMovement.y > 0 && x >= Tile.subTileWidthHeight / 2 && y >= Tile.subTileWidthHeight / 2 && x == y)
						{
							property.hasRiver = true;
						}
						else if (fromMovement.x < 0 && fromMovement.y < 0 && x <= Tile.subTileWidthHeight / 2 && y <= Tile.subTileWidthHeight / 2 && x == y)
						{
							property.hasRiver = true;
						}
						else if (fromMovement.x > 0 && fromMovement.y < 0 && x >= Tile.subTileWidthHeight / 2 && y <= Tile.subTileWidthHeight / 2 && Tile.subTileWidthHeight - x - 1 == y)
						{
							property.hasRiver = true;
						}
						else if (fromMovement.x < 0 && fromMovement.y > 0 && x <= Tile.subTileWidthHeight / 2 && y >= Tile.subTileWidthHeight / 2 && Tile.subTileWidthHeight - x - 1 == y)
						{
							property.hasRiver = true;
						}

					}

					if (property.hasRiver)
					{
						tile.RemoveTrees(property.treeCount, x, y);
					}

				}
			}

		}

		for (int x = 0; x < Tile.subTileWidthHeight; x++)
		{
			for (int y = 0; y < Tile.subTileWidthHeight; y++)
			{
				Property property = tile.properties[x][y];
				if (!property.hasRiver && (x == y || Tile.subTileWidthHeight - x - 1 == y || Tile.subTileWidthHeight - y - 1 == x || x == Tile.subTileWidthHeight / 2 || y == Tile.subTileWidthHeight / 2))
				{
					property.hasRoad = true;
				} else if (x == Tile.subTileWidthHeight / 2 || y == Tile.subTileWidthHeight / 2)
				{
					property.hasRoad = true;
				}
				tile.RemoveTrees(property.treeCount, x, y);

			}
		}

		for (int i = 0; i < familyPerCity; i++)
		{
			GeneratePeople(world, tile);
		}
		Update();
	}

	private void GeneratePeople(World world, Tile tile)
	{
		PersonData person1 = new PersonData();
		person1.id = world.Random.Randi().ToString();
		world.people[person1.id] = person1;
		peopleIds.Add(person1.id);

		if (world.Random.Randf() < 0.5f)
		{
			person1.gender = Gender.male;
			person1.firstName = world.peopleNames.GenerateFirstName(Sex.Male);
		} else
		{
			person1.gender = Gender.female;
			person1.firstName = world.peopleNames.GenerateFirstName(Sex.Female);
		}
		person1.lastName = world.peopleNames.GenerateLastName();
		int age = world.Random.RandiRange(minAdultAge, maxAdultAge);
		person1.birthDate = world.Time - GDate.Year * age;

		Building building = new Building();
		building.name = $"{person1.lastName} Household";
		building.capacity = 5;
		building.type = Building.Type.Residential;
		building.buildingId = "building" + world.Random.Randi().ToString();
		tile.AddBuilding(building);

		person1.houseId = building.buildingId;

		if (world.Random.Randf() < bussinessOwnerPercent)
		{
			CommercialBuilding commercialBuilding = CommercialBuilding.CreateWoodShop(person1, tile);
			//Console.WriteLine($"Available Space: {commercialBuilding.AvaliableWork()}");
			commercialBuilding.AddWorker(person1);
		} else
		{
			//TODO: find work
		}


		if (World.Instance.Random.Randf() > marriedPercent)
		{
			PersonData person2 = new PersonData();
			person2.id = world.Random.Randi().ToString();
			world.people[person2.id] = person2;
			peopleIds.Add(person2.id);

			if (person1.gender == Gender.female)
			{
				person2.gender = Gender.male;
				person2.firstName = world.peopleNames.GenerateFirstName(Sex.Male);
			} else
			{
				person2.gender = Gender.female;
				person2.firstName = world.peopleNames.GenerateFirstName(Sex.Female);
			}
			person1.spouseId = person2.id;
			person2.spouseId = person1.spouseId;
			person2.houseId = building.buildingId;
			int age2 = world.Random.RandiRange(age - 10, age + 10);
			age2 = Mathf.Clamp(age2, minAdultAge, maxAdultAge);
			person1.birthDate = world.Time - GDate.Year * age2;
			person2.lastName = person1.lastName;
			Console.WriteLine(person2.ToString());

			if (person1.businessOwnerIds.Count > 0 && person1.GetComercialBuilding(0).AvaliableWork() > 0)
			{
				CommercialBuilding commercialBuilding = person1.GetComercialBuilding(0);
				//Console.WriteLine($"Available Space: {commercialBuilding.AvaliableWork()}");
				commercialBuilding.AddWorker(person2);
			}
		}

		Console.WriteLine(person1.ToString());
	}

	public override void _Draw()
	{
		base._Draw();
		//Console.WriteLine("Updating City Map");
		float squareWidthHeight = (float) world.TileSize / Tile.subTileWidthHeight;
		Tile tile = world.tiles[tileCoords[0].x][tileCoords[0].y];
		//Color roadColor = new Color(213f / 255, 204f / 255, 127f / 255);
		Color roadColor = Color.ColorN("brown");
		Color waterColor = Color.ColorN("blue");
		Color industryColor = Color.ColorN("yellow");
		Color commercialColor = Color.ColorN("purple");
		Color residentialColor = Color.ColorN("green");
		Color errorColor = Color.ColorN("pink");
		//Console.WriteLine($"squareWidthHeight: {squareWidthHeight}");

		for (int x = 0; x < Tile.subTileWidthHeight; x++)
		{
			for (int y = 0; y < Tile.subTileWidthHeight; y++)
			{
				Property property = tile.properties[x][y];
				float propertyX = property.x * squareWidthHeight - Tile.subTileWidthHeight / 2f * squareWidthHeight;
				float propertyY = property.y * squareWidthHeight - Tile.subTileWidthHeight / 2f * squareWidthHeight;
				 Vector2 rectPosition = new Vector2(propertyX, propertyY);
				if (property.hasRoad)
				{
					
					//Console.WriteLine($"road property: {propertyX}, {propertyY}");
				   
					DrawRect(new Rect2(rectPosition, squareWidthHeight, squareWidthHeight), roadColor);
					//Console.WriteLine($"drew road");
				} else if (property.hasRiver || property.hasBasin)
				{
					DrawRect(new Rect2(rectPosition, squareWidthHeight, squareWidthHeight), waterColor);
				} else if (property.hasStructure)
				{
					switch (property.Building.type)
					{
						case Building.Type.Residential:
							DrawRect(new Rect2(rectPosition, squareWidthHeight, squareWidthHeight), residentialColor);
							break;
						case Building.Type.Commercial:
							DrawRect(new Rect2(rectPosition, squareWidthHeight, squareWidthHeight), commercialColor);
							break;
						case Building.Type.Industrial:
							DrawRect(new Rect2(rectPosition, squareWidthHeight, squareWidthHeight), industryColor);
							break;
						default:
							DrawRect(new Rect2(rectPosition, squareWidthHeight, squareWidthHeight), errorColor);
							break;
					}
				}
			}
		}
	}

	public List<PersonData> GetPeople()
	{
		List<PersonData> people = new List<PersonData>();
		foreach (String personId in peopleIds)
		{
			people.Add(World.Instance.people[personId]);
		}

		return people;
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
