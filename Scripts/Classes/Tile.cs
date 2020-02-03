using Godot;
using Life.Scripts.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Tile
{
	public const int subTileWidthHeight = 19;

	public string name;

	public TileType biome;
	public float elev;
	public float moistureNoise;
	public float latTemperature;
	public float cellMoisture;
	public float moisture;
	public Coord coord;

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

	public Property[][] properties;
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

	public void GenerateProperties()
	{
		this.properties = new Property[Tile.subTileWidthHeight][];

		int treesPerProperty = 0;
		if (resources.Exists(x => x.resource == MapResourceName.Tree))
		{
			Debug.WriteLine("Total trees count: " + resources.Find(x => x.resource == MapResourceName.Tree).amount + "/" + (Tile.subTileWidthHeight * Tile.subTileWidthHeight));
			treesPerProperty = (int)resources.Find(x => x.resource == MapResourceName.Tree).amount / (Tile.subTileWidthHeight * Tile.subTileWidthHeight);
			resources.Find(x => x.resource == MapResourceName.Tree).amount = treesPerProperty * Tile.subTileWidthHeight * Tile.subTileWidthHeight;
			Debug.WriteLine("trees per property " + treesPerProperty);

		}

		for (int x = 0; x < Tile.subTileWidthHeight; x++)
		{
			this.properties[x] = new Property[Tile.subTileWidthHeight];
			for (int y = 0; y < Tile.subTileWidthHeight; y++)
			{
				Property property = new Property();
				property.x = x;
				property.y = y;
				property.treeCount = treesPerProperty;
				this.properties[x][y] = property;

			}
		}
	   
	}

	public void RemoveTrees(int count, int x, int y)
	{
		//for (int i = 0; i < resources.Count; i++)
		//{
		//    MapResourceData mapResourceData = resources[i];
		//    Debug.WriteLine(mapResourceData.resource);
		//}
		if (resources.Exists(r => r.resource == MapResourceName.Tree))
		{
			//Debug.WriteLine("Found Tree resource");
			properties[x][y].treeCount -= count;
			resources.Find(r => r.resource == MapResourceName.Tree).amount -= count;
		} else
		{
			//Debug.WriteLine("could not find tree resource");
		}
		
	}

	public void AddBuilding(Building building)
	{
		if (this.properties == null)
		{
			GenerateProperties();
		}

		for (int x = 0; x < Tile.subTileWidthHeight; x++)
		{
			for (int y = 0; y < Tile.subTileWidthHeight; y++)
			{
				Property property = this.properties[x][y];
				if (!property.hasStructure)
				{
					property.building = building;
					RemoveTrees(property.treeCount, x, y);
					return;
				}
			}
		}

		throw new Exception("Did not find space to add building");
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
