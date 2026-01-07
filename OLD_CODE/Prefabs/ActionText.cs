using Godot;
using System;

namespace OldCode;
public partial class ActionText : Label
{
	Person person;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		person = GetParent<Person>();
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		this.Text = person.PersonAction.ToString();
	}
}
