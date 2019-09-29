using Godot;
using System;

public class Rivers : Node2D
{
     World world;
    private float riverAlphaScale = 0.1f;
    private float riverWidthScale = .5f;
    private float waterBasinScale = 0.5f;
    private bool riverAlpha = true;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        world = GetParent<World>();
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }

    public override void _Draw()
    {
        base._Draw();
        //Console.WriteLine("Updating map");

        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                Tile tile = world.tiles[x][y];
                
                if (tile.hasWaterBasin)
                {
                    DrawCircle(tile.position, tile.waterBasinSize * waterBasinScale, new Color(0.2f, 0.2f, .6f));
                } else if (tile.hasRiver)
                {
                    RiverData river = world.rivers[$"{tile.position.ToString()}"];
                    if (riverAlpha)
                    {
                        DrawLine(river.position, river.toPosition, new Color(0.2f, 0.2f, .6f,river.size * riverAlphaScale ), river.size * riverWidthScale);
                    } else
                    {
                        DrawLine(river.position, river.toPosition, new Color(0.2f, 0.2f, .6f,1 ), river.size * riverWidthScale);
                    }
                }
            }
        }

    }

    public void GenerateRivers()
    {
        Update();
    }

    
}


