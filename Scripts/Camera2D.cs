using Godot;
using System;

public class Camera2D : Godot.Camera2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	private float movementStrength = 100f;
	private float zoomStrength = 0.2f;

	// Camera control settings:
	// key - by keyboard
	//drag - by clicking mouse button, right mouse button by default;
	// edge - by moving mouse to the window edge
	// wheel - zoom in/out by mouse wheel
	public bool key = true;
	public bool drag = true;
	public bool edge = false;
	public bool wheel = true;

	public int zoom_out_limit = 100;

	// Camera speed in px/s.
	public int camera_speed = 450;

	// Initial zoom value taken from Editor.
	Vector2 camera_zoom;

	// Value meaning how near to the window edge (in px) the mouse must be,
	// to move a view.
	public int camera_margin = 50;

	// It changes a camera zoom value in units... (?, but it works... it probably
	// multiplies camera size by 1+camera_zoom_speed)
	Vector2 camera_zoom_speed = new Vector2(0.05f, 0.05f);
	const float zoom_speed_mod = 0.05f;
	const int worldXLimit = 100 * 150;
	const int worldYLimit = 100 * 100;

	// Vector of camera's movement / second.
	Vector2 camera_movement = new Vector2();

	// Previous mouse position used to count delta of the mouse movement.
	Vector2 _prev_mouse_pos = new Vector2();

	// INPUTS

	// Right mouse button was or is pressed.
	bool __rmbk = false;
	// Move camera by keys: left, top, right, bottom.
	bool[] __keys = new bool[] { false, false, false, false };

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		camera_zoom = Zoom;
		_prev_mouse_pos = GetLocalMousePosition();
		DragMarginHEnabled = false;
		DragMarginVEnabled = false;
		SmoothingEnabled = true;
		SmoothingSpeed = 4;

	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		if(key)
		{
			if (__keys[0])
			{
				camera_movement.x -= camera_speed * delta;
			}
			if (__keys[1])
			{
				camera_movement.y -= camera_speed * delta;
			}
			if (__keys[2])
			{
				camera_movement.x += camera_speed * delta;
			}
			if (__keys[3])
			{
				camera_movement.y += camera_speed * delta;
			}
		}

		// Move camera by mouse, when it's on the margin (defined by camera_margin).

		if (edge)
		{
			Rect2 rec = GetViewport().GetVisibleRect();
			Vector2 v = GetLocalMousePosition() + rec.Size * 0.5f;

			if (rec.Size.x - v.x <= camera_margin)
			{
				camera_movement.x += camera_speed * delta;
			}
			if (v.x <= camera_margin)
			{
				camera_movement.x -= camera_speed * delta;
			}
			if (rec.Size.y - v.y <= camera_margin)
			{
				camera_movement.y += camera_speed * delta;
			}
			if (v.y <= camera_margin)
			{
				camera_movement.y -= camera_speed * delta;
			}
		}

		// When RMB is pressed, move camera by difference of mouse position
		if (drag && __rmbk)
		{
			camera_movement = _prev_mouse_pos - GetLocalMousePosition();
		}

		// Update position of the camera.
		Position += camera_movement * Zoom;
		if (Position.y < -worldYLimit)
		{
			Position = new Vector2(Position.x, -worldYLimit);
		}
		if (Position.y > worldYLimit)
		{
			Position = new Vector2(Position.x, worldYLimit);
		}
		if (Position.x < -worldXLimit)
		{
			Position = new Vector2(-worldXLimit, Position.y);
		}
		if (Position.y > worldXLimit)
		{
			Position = new Vector2(worldXLimit, Position.y);
		}

		// Set camera movement to zero, update old mouse position.
		camera_movement = Vector2.Zero;
		_prev_mouse_pos = GetLocalMousePosition();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton)
		{
			InputEventMouseButton e = @event as InputEventMouseButton;
			if (drag && e.ButtonIndex == (int) ButtonList.Right)
			{
				__rmbk = e.IsPressed();
			}

			if (wheel)
			{
				if (e.ButtonIndex == (int) ButtonList.WheelUp && camera_zoom.x + camera_zoom_speed.x > 0 && camera_zoom.y + camera_zoom_speed.y > 0)
				{
					camera_zoom -= camera_zoom_speed + Zoom * zoom_speed_mod;
					Zoom = camera_zoom;
				}

				if (e.ButtonIndex == (int)ButtonList.WheelDown && camera_zoom.x + camera_zoom_speed.x < zoom_out_limit && camera_zoom.y + camera_zoom_speed.y < zoom_out_limit)
				{
					camera_zoom += camera_zoom_speed + Zoom * zoom_speed_mod;
					Zoom = camera_zoom;
				}
			}
		}
		if (Input.IsActionJustPressed("action_left"))
		{
			__keys[0] = true;
		}
		if (Input.IsActionJustPressed("action_up"))
		{
			__keys[1] = true;
		}
		if (Input.IsActionJustPressed("action_right"))
		{
			__keys[2] = true;
		}
		if (Input.IsActionJustPressed("action_down"))
		{
			__keys[3] = true;
		}
		if (Input.IsActionJustReleased("action_left"))
		{
			__keys[0] = false;
		}
		if (Input.IsActionJustReleased("action_up"))
		{
			__keys[1] = false;
		}
		if (Input.IsActionJustReleased("action_right"))
		{
			__keys[2] = false;
		}
		if (Input.IsActionJustReleased("action_down"))
		{
			__keys[3] = false;
		}
	}
}
