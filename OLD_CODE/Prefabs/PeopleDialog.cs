using Godot;
using OldCode;
using System;
using System.Collections.Generic;

namespace OldCode;
public partial class PeopleDialog : Control
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	public WindowPanel windowPanel;
	public VBoxContainer container;
	private Label title;
	private City city;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		windowPanel = GetNode<WindowPanel>("WindowPanel");
		title = windowPanel.title;
		container = GetNode<VBoxContainer>("WindowPanel/ScrollContainer/VBoxContainer");
		windowPanel.Connect("closeButtonPressed", new Callable(this, nameof(_on_CloseButton_pressed)));
		windowPanel.Connect("dragPositionSignal", new Callable(this, nameof(_on_DragPosition_Signal)));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton)
		{
			InputEventMouseButton e = @event as InputEventMouseButton;
			if (e.Pressed)
			{
				Godot.Collections.Array<Node> parentChildren = GetParent().GetChildren();
				if ( GetIndex() + 1 != parentChildren.Count)
				{
                    Console.WriteLine("pressed: {0}", e);
                }
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
		GlobalPosition += dragPosition;
	}

	public void ShowPeople(City city)
	{
		Show();
		title.Text = city.name;
		this.city = city;
		List<PersonData> people = city.GetPeople();
		people.Sort((a, b) => a.firstName.CompareTo(b.firstName));
		foreach( PersonData person in people)
		{
			CustomButton button = DialogManager.Instance.buttonPrefab.Instantiate<CustomButton>();

			button.SetPressedAction((object sender, EventArgs e) => { DialogManager.Instance.CreatePersonDialog(person); });
			button.Name = $"{person.firstName} {person.lastName}";
			button.Text = $"{person.firstName} {person.lastName}";
			container.AddChild(button);

		}
	}


}
