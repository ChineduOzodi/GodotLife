using Godot;
using Life.Scripts.Classes;
using System;
using System.Collections.Generic;

public class PersonDialog : Control
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	public WindowPanel windowPanel;
	public VBoxContainer container;
	private Label title;
    private PersonData person;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		windowPanel = GetNode<WindowPanel>("WindowPanel");
		title = windowPanel.title;
		container = GetNode<VBoxContainer>("WindowPanel/ScrollContainer/VBoxContainer");
		windowPanel.Connect("closeButtonPressed", this, nameof(_on_CloseButton_pressed));
		windowPanel.Connect("dragPositionSignal", this, nameof(_on_DragPosition_Signal));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton)
		{
			InputEventMouseButton e = @event as InputEventMouseButton;
			if (e.Pressed)
			{
				Godot.Collections.Array parentChildren = GetParent().GetChildren();
				if ( GetIndex() + 1 != parentChildren.Count)
				{
					Raise(); 
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
		RectGlobalPosition += dragPosition;
	}

	public void ShowPerson(PersonData person)
	{
		Show();
		title.Text = person.name;
        this.person = person;

        CustomButton button = (CustomButton) DialogManager.Instance.buttonPrefab.Instance();

        button.Disabled = true;
        button.SetPressedAction((object sender, EventArgs e) => { });
        button.Name = "person age";
        button.Text = $"Age: {person.GetAge(World.Instance.Time)}";
        container.AddChild(button);


        button = (CustomButton)DialogManager.Instance.buttonPrefab.Instance();
        button.Disabled = true;
        button.SetPressedAction((object sender, EventArgs e) => { });
        button.Name = "gender";
        button.Text = $"Gender: {person.gender.ToString()}";
        container.AddChild(button);


        if (person.hasSpouse)
        {
            button = (CustomButton)DialogManager.Instance.buttonPrefab.Instance();
            PersonData spouse = World.Instance.people[person.spouseId];
            button.SetPressedAction((object sender, EventArgs e) => { DialogManager.Instance.CreatePersonDialog(spouse); });
            button.Name = "spouse";
            button.Text = $"Spouse: {spouse.name}";
            container.AddChild(button);
        }

        if (person.houseId != null)
        {
            button = (CustomButton)DialogManager.Instance.buttonPrefab.Instance();
            button.Disabled = true;
            button.SetPressedAction((object sender, EventArgs e) => { });
            button.Name = "home";
            button.Text = $"Home: {World.Instance.buildings[person.houseId].name}";
            container.AddChild(button);
        }

        if (person.workPlaceId != null)
        {
            button = (CustomButton)DialogManager.Instance.buttonPrefab.Instance();
            button.Disabled = true;
            button.SetPressedAction((object sender, EventArgs e) => { });
            button.Name = "work";
            button.Text = $"Work: {World.Instance.buildings[person.workPlaceId].name}";
            container.AddChild(button);
        }

    }


}
