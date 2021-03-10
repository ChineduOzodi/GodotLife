using Godot;
using Life.Scripts.Classes;
using System;
using System.Collections.Generic;

public class DialogManager : Node
{
    public static DialogManager Instance;
    CanvasLayer canvas;

    private PackedScene cityDialogPrefab;
    private PackedScene peopleDialogPrefab;
    private PackedScene personDialogPrefab;
    public PackedScene buttonPrefab;

    public List<CityDialog> cityDialogs = new List<CityDialog>();
    public List<PeopleDialog> peopleDialogs = new List<PeopleDialog>();
    public List<PersonDialog> personDialogs = new List<PersonDialog>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;
        canvas = GetNode<CanvasLayer>("../CanvasLayer");

        cityDialogPrefab = ResourceLoader.Load<PackedScene>("res://Prefabs/CityDialog.tscn");
        peopleDialogPrefab = ResourceLoader.Load<PackedScene>("res://Prefabs/PeopleDialog.tscn");
        personDialogPrefab = ResourceLoader.Load<PackedScene>("res://Prefabs/PersonDialog.tscn");
        buttonPrefab = ResourceLoader.Load<PackedScene>("res://Prefabs/Button.tscn");
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    public void CreateCityDialog( City city)
    {
        CityDialog dialog = null;

        for (int i = 0; i < cityDialogs.Count; i++)
        {
            CityDialog pDialog = cityDialogs[i];
            if (!pDialog.Visible)
            {
                dialog = pDialog;
                break;
            }
        }
        if (dialog == null)
        {
            dialog = (CityDialog)cityDialogPrefab.Instance();
            canvas.AddChild(dialog);
            cityDialogs.Add(dialog);
        }
        dialog.ShowCity(city);
    }

    public void CreatePeopleDialog(City city)
    {
        PeopleDialog dialog = null;

        for (int i = 0; i < peopleDialogs.Count; i++)
        {
            PeopleDialog pDialog = peopleDialogs[i];
            if (!pDialog.Visible)
            {
                dialog = pDialog;
                break;
            }
        }
        if (dialog == null)
        {
            dialog = (PeopleDialog) peopleDialogPrefab.Instance();
            canvas.AddChild(dialog);
            peopleDialogs.Add(dialog);
        }
        dialog.ShowPeople(city);
    }

    public void CreatePersonDialog(PersonData person)
    {
        PersonDialog dialog = null;

        for (int i = 0; i < personDialogs.Count; i++)
        {
            PersonDialog pDialog = personDialogs[i];
            if (!pDialog.Visible)
            {
                dialog = pDialog;
                break;
            }
        }
        if (dialog == null)
        {
            dialog = (PersonDialog)personDialogPrefab.Instance();
            canvas.AddChild(dialog);
            personDialogs.Add(dialog);
        }
        dialog.ShowPerson(person);
    }
}
