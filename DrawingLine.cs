using Godot;
using System;

public class DrawingLine : Node2D
{
    private World world;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        world = GetParent<World>();
    }

    public override void _Draw()
    {
        base._Draw();

        if (world.NearestPerson != null && world.NearestPerson.path != null)
        {
            Vector2[] linePath = new Vector2[world.NearestPerson.path.Length * 2 - 2];
            Color[] colors = new Color[world.NearestPerson.path.Length * 2 - 2];
            for (int i = 0; i < world.NearestPerson.path.Length - 1; i++)
            {
                linePath[i * 2] = world.NearestPerson.path[i].worldPosition;
                linePath[i * 2 + 1] = world.NearestPerson.path[i + 1].worldPosition;

                if (world.NearestPerson.PathIndex > i)
                {
                    colors[i * 2] = new Color(1,1,1,0.1f);
                    colors[i * 2 + 1] = new Color(1,1,1,0.1f);
                } else if (world.NearestPerson.PathIndex < i){
                    colors[i * 2] = new Color(1, 1, 1, 0.7f);
                    colors[i * 2 + 1] = new Color(1, 1, 1, 0.3f);
                } else
                {
                    colors[i * 2] = new Color(1, 1, 0, 0.7f);
                    colors[i * 2 + 1] = new Color(1, 1, 0, 0.7f);
                }
                
            }
            DrawMultilineColors(linePath, colors);
            //DrawMultiline(linePath, Color.ColorN("white"));
            //Console.WriteLine("Drawing path");
        }

    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //public override void _Process(float delta)
    //{

    //}
}
