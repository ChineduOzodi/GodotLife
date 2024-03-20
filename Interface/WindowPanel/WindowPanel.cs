using Godot;
using System;

public partial class WindowPanel : Panel
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	public Label title;
	private WindowTitle windowTitle;
	[Signal]
	public delegate void closeButtonPressedEventHandler();
	[Signal]
	public delegate void dragPositionSignalEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		windowTitle = GetNode<WindowTitle>("WindowTitle");
		windowTitle.Connect("closeButtonPressed", new Callable(this, nameof(_on_CloseButton_pressed)));
		windowTitle.Connect("dragPositionSignal", new Callable(this, nameof(_on_DragPosition_Signal)));
		title = windowTitle.title;
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_CloseButton_pressed()
	{
		EmitSignal(nameof(closeButtonPressedEventHandler));
	}

	private void _on_DragPosition_Signal(Vector2 dragPosition)
	{
		EmitSignal(nameof(dragPositionSignalEventHandler),dragPosition);
	}
}
