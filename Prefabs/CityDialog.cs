using Godot;
using System;

public partial class CityDialog : Control
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
		windowPanel.Connect("closeButtonPressed", new Callable(this, nameof(_on_CloseButton_pressed)));
		windowPanel.Connect("dragPositionSignal", new Callable(this, nameof(_on_DragPosition_Signal)));
		itemList.Connect("item_selected", new Callable(this, nameof(onItemSelected)));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (this.city != null)
		{
			itemList.SetItemText(0, $"Population: {city.Population}");
			itemList.SetItemText(1, $"Buildings: {city.buildings.Count}");
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton)
		{
			InputEventMouseButton e = @event as InputEventMouseButton;
			if (e.Pressed)
			{
				Console.WriteLine("pressed: {0}", e);
			}

		}
	}

	public void onItemSelected ( int index)
	{
		itemList.Deselect(index);
		if (index == 0 && DialogManager.Instance != null)
		{
			DialogManager.Instance.CreatePeopleDialog(city);
		}

	}

	private void _on_CloseButton_pressed()
	{
		Console.WriteLine("CloseButton Pressed");
		Hide();
	}

	private void _on_DragPosition_Signal(Vector2 dragPosition)
	{
		GlobalPosition += dragPosition;
	}

	public void ShowCity(City city)
	{
		Show();
		title.Text = city.name;
		this.city = city;
		itemList.SetItemText(0, $"Population: {city.Population}");
		itemList.SetItemText(1, $"Buildings: {city.buildings.Count}");
	}


}
