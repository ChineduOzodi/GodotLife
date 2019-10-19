using Godot;
using System;
using System.Collections.Generic;

public class PersonData
{
    public string firstName;
    public string personId;
    public string lastName;
    public Gender gender;
    public long birthDate; //
    public bool alive = true;
    public long deathDate;
    public Vector2 position;
    public float elev;
    public PersonAction personAction = PersonAction.Idle;
    public int pathIndex = 0;
    public float walkSpeed = 2;
    public Tile currentTile;
    public bool hasSpouse;
    public string spouseId;
    public string motherId;
    public string fatherId;
    public List<string> childrenIds;
    public string houseId;
    public string workPlaceId;
}

public enum Gender
{
    male,
    female
}
