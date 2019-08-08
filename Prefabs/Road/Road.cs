using Godot;
using System;

public class Road : Node2D
{
    public Tile tile;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (tile != null)
        {
            if (tile.speedMod > tile.baseSpeedMod)
            {
                tile.speedMod -= delta * tile.recoveryRate;
                if (tile.speedMod < tile.baseSpeedMod)
                    tile.speedMod = tile.baseSpeedMod;
                float mod = (tile.speedMod - tile.baseSpeedMod) / (tile.maxSpeedMod - tile.baseSpeedMod);
                GetChild<Sprite>(0).SetModulate(new Color(0, 0, 0, 0).LinearInterpolate(Color.ColorN("brown"), mod));
            }
        }
    }
}
