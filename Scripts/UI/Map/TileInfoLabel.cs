using Godot;
using System;

public class TileInfoLabel : RichTextLabel
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		if (World.Instance != null && World.Instance.HoveredTile != null)
		{
			Tile tile = World.Instance.HoveredTile;
			string info = $"{tile.name}\n{tile.biome.ToString()}";
			if (tile.hasRiver)
			{
				info += $"\nHas River";
			}
			if (tile.hasWaterBasin)
			{
				info += "\nHas Waterbasin";
			}
			Text = info;
		} else
		{
			Text = "";
		}
	}
}
