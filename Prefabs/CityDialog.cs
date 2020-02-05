using Godot;
using System;

public class CityDialog : Control
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	public WindowPanel windowPanel;
	public ItemList itemList;
	private Label title;
	private City city;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		windowPanel = GetNode<WindowPanel>("WindowPanel");
		title = windowPanel.title;
		itemList = GetNode<ItemList>("WindowPanel/ItemList");
		windowPanel.Connect("closeButtonPressed", this, nameof(_on_CloseButton_pressed));
		windowPanel.Connect("dragPositionSignal", this,nameof(_on_DragPosition_Signal));
		itemList.Items = new Godot.Collections.Array();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		if (this.city != null)
		{
			itemList.SetItemText(0, $"Population: {city.Population}");
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton)
		{
			InputEventMouseButton e = @event as InputEventMouseButton;
			if (e.Pressed)
			{
				Raise();
			}

		}
	}

	private void _on_CloseButton_pressed()
	{
		Console.WriteLine("CloseButton Pressed");
		Hide();
	}

	private void _on_DragPosition_Signal(Vector2 dragPosition)
	{
		RectGlobalPosition += dragPosition;
	}

	public void ShowCity(City city)
	{
		Show();
		itemList.Items = new Godot.Collections.Array();
		title.Text = city.name;
		this.city = city;
		itemList.AddItem($"Population: {city.Population}");
	}


}
