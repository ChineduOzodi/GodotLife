using Godot;
using Life.Scripts.Classes;
using System;

public class MapResourceOverlay : Node2D
{
	World world;
	MapResource mapResource;
	float resourceScale = 10;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		world = GetParent<World>();
	}

	public override void _Draw()
	{
		base._Draw();
		//Console.WriteLine("Updating map");

		if (mapResource != null)
		{
			for (int x = 0; x < world.Width; x++)
			{
				for (int y = 0; y < world.Height; y++)
				{
					Tile tile = world.tiles[x][y];

					if (tile.HasMapResource(mapResource.Name))
					{
						MapResourceData mapResourceData = tile.GetMapResourceData(mapResource.Name);
						DrawCircle(tile.position, mapResourceData.amount / mapResource.resourceMax * resourceScale, Color.ColorN("green",0.75f));
					}
				}
			}
		}

		

	}

	public void DisplayMapResource(MapResource mapResource)
	{
		this.mapResource = mapResource;
		Update();
	}

	public void ClearMapResourceDisplay()
	{
		mapResource = null;
		Update();
	}
}
