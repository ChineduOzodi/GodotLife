using Godot;
using System;

public class ActionText : Label
{
    Person person;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        person = GetParent<Person>();
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        SetText(person.PersonAction.ToString());
    }
}
