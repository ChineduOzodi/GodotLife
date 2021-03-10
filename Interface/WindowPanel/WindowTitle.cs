using Godot;
using System;

public class WindowTitle : Panel
{
	// Declare member variables here. Examples:
	public Label title;
	private Button closeButton;
	[Signal]
	public delegate void closeButtonPressed();
	[Signal]
	public delegate void dragPositionSignal();
	public Vector2 dragPosition;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		title = GetNode<Label>("Label");
		closeButton = GetNode<Button>("CloseButton");
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	//  public override void _Process(float delta)
	//  {
	//      
	//  }

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton)
		{
			InputEventMouseButton e = @event as InputEventMouseButton;
			if(e.Pressed)
			{
				dragPosition = GetGlobalMousePosition() - RectGlobalPosition;
			} else
			{
				dragPosition = Vector2.Zero;
			}
		   
		}

		if (@event is InputEventMouseMotion && dragPosition != Vector2.Zero)
		{
			InputEventMouseMotion e = @event as InputEventMouseMotion;
			EmitSignal(nameof(dragPositionSignal), GetGlobalMousePosition() - RectGlobalPosition - dragPosition);
			dragPosition = GetGlobalMousePosition() - RectGlobalPosition;
		}
	}

	private void _on_CloseButton_pressed()
	{
		EmitSignal(nameof(closeButtonPressed));
	}
}
