using Godot;
using Life.Scripts.Classes;
using System;

public class DisplayMenu : MenuButton
{
    World world;
    const string Normal = "Normal";
    const string Height = "Height";
    const string MoistureNoise = "Moisture Noise";
    const string LatTemperature = "Lat Temperature";
    const string CellMoisture = "Cell Moisture";
    const string Moisture = "Moisture";
    const string WalkingSpeed = "Walking Speed";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        world = GetParent().GetParent<World>();
        GetPopup().AddItem(Normal);
        GetPopup().AddItem(Height);
        GetPopup().AddItem(WalkingSpeed);
        GetPopup().AddItem(MoistureNoise);
        GetPopup().AddItem(LatTemperature);
        GetPopup().AddItem(CellMoisture);
        GetPopup().AddItem(Moisture);

        GetPopup().Connect("id_pressed", this, "OnItemPressed");
    }

    public void AddToDisplayMenu(string itemName)
    {
        Console.WriteLine($"Adding to display menu: {itemName}");
        GetPopup().AddItem(itemName);
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
            case Normal:
                world.UpdateDisplayMode(DisplayMode.Normal);
                break;
            case Height:
                world.UpdateDisplayMode(DisplayMode.Height);
                break;
            case MoistureNoise:
                world.UpdateDisplayMode(DisplayMode.MoistureNoise);
                break;
            case LatTemperature:
                world.UpdateDisplayMode(DisplayMode.LatTemperature);
                break;
            case CellMoisture:
                world.UpdateDisplayMode(DisplayMode.CellMoisture);
                break;
            case Moisture:
                world.UpdateDisplayMode(DisplayMode.Moisture);
                break;
            case WalkingSpeed:
                world.UpdateDisplayMode(DisplayMode.WalkingSpeed);
                break;
            default:
                //should be a map resource
                MapResource mapResource = world.GetMapResource(itemName);
                world.DisplayMapResource(mapResource);
                break;
        }

    }


}
