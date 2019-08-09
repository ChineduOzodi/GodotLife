using Godot;
using System;

public class DisplayMenu : MenuButton
{
    World world;
    const string NORMAL = "Normal";
    const string HEIGHT = "Height";


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        world = GetParent().GetParent<World>();
        GetPopup().AddItem(NORMAL);
        GetPopup().AddItem(HEIGHT);

        GetPopup().Connect("id_pressed", this, "OnItemPressed");
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    public void OnItemPressed(int id)
    {
        //Console.WriteLine($"Item pressed: {id}");
        string itemName = GetPopup().GetItemText(id);
        switch(itemName)
        {
            case NORMAL:
                world.UpdateDisplayMode(DisplayMode.Normal);
                break;
            case HEIGHT:
                world.UpdateDisplayMode(DisplayMode.Height);
                break;
        }

    }


}
