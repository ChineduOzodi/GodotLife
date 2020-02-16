using Godot;
using System;

public class CustomButton : Button
{
	public event EventHandler onButtonPressed;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	//  public override void _Process(float delta)
	//  {
	//      
	//  }


	public void SetPressedAction (EventHandler action)
	{
		onButtonPressed += action;
	}
	public override void _Pressed()
	{
		onButtonPressed?.Invoke(this, EventArgs.Empty);
	}
}
