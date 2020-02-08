using Godot;
using System;
using System.Collections.Generic;

public class MouseClicks : Node
{
	public static MouseClicks Instance;
	public List<CityDialog> dialogs;
	CanvasLayer canvas;

	private PackedScene dialogPrefab;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
		dialogPrefab = ResourceLoader.Load<PackedScene>("res://Prefabs/cityDialog.tscn");
		dialogs = new List<CityDialog>();
		canvas = GetNode<CanvasLayer>("../CanvasLayer");
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	//  public override void _Process(float delta)
	//  {
	//      
	//  }

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton)
		{
			InputEventMouseButton e = @event as InputEventMouseButton;
			MouseClicked(e);
		}
	}

	public void MouseClicked(InputEventMouseButton e)
	{
		//find location of mouse
		if (e.ButtonIndex == (int)ButtonList.Left && e.IsPressed())
		{
			if (World.Instance != null && World.Instance.HoveredTile != null)
			{
				Tile tile = World.Instance.HoveredTile;
				if (tile.cityId != null)
				{
					City city = World.Instance.cities.Find(x => x.id == tile.cityId);

					CityDialog cityDialog = null;
					
					for(int i =0; i < dialogs.Count; i++)
					{
						CityDialog dialog = dialogs[i];
						if (!dialog.Visible)
						{
							cityDialog = dialog;
							break;
						}
					}
					if (cityDialog == null)
					{
						cityDialog = (CityDialog)dialogPrefab.Instance();
						canvas.AddChild(cityDialog);
						dialogs.Add(cityDialog);
					}
					cityDialog.ShowCity(city);
				}
			}
		}
	}
}