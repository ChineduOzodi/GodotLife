using Godot;
using System;

public class Camera2D : Godot.Camera2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    private float movementStrength = 100f;
    private float zoomStrength = 0.2f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        float deltaX = 0;
        float deltaY = 0;

        if (Input.IsActionPressed("action_up"))
        {
            deltaY -= movementStrength * GetZoom().x;
        }
        if (Input.IsActionPressed("action_down"))
        {
            deltaY += movementStrength * GetZoom().x;
        }
        if (Input.IsActionPressed("action_left"))
        {
            deltaX -= movementStrength * GetZoom().x;
        }
        if (Input.IsActionPressed("action_right"))
        {
            deltaX += movementStrength * GetZoom().x;
        }
        if (Input.IsActionPressed("action_zoom_up"))
        {
            float zoom = zoomStrength * GetZoom().x * delta;
            SetZoom(new Vector2(GetZoom().x + zoom, GetZoom().x + zoom));
        }
        if (Input.IsActionPressed("action_zoom_down"))
        {
            float zoom = zoomStrength * GetZoom().x * delta;
            SetZoom(new Vector2(GetZoom().x - zoom, GetZoom().x - zoom));
        }
        SetOffset(new Vector2(GetOffset().x + (deltaX * delta), GetOffset().y + (deltaY * delta)));
    }
}
