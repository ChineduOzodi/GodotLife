using Godot;
using System;

public class TileMap : Godot.TileMap
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
	
    

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //CreateNoise(500, 500);
    }
	
	public void CreateNoise(int width, int height, OpenSimplexNoise noise, float waterLevel) {
        int xOffset = -width / 2;
        int yOffset = -height / 2;
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
                float elev = (noise.GetNoise2d(x + xOffset, y + yOffset) + 1) * .5f;
                if (elev > waterLevel)
                {
                    SetCell(x + xOffset, y + yOffset, (int) TileType.Grassland);
                } else
                {
                    SetCell(x + xOffset, y + yOffset, (int) TileType.Water);
                }
				
			}
		}
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

public enum TileType
{
    Grassland = 0,
    Water = 1
}
