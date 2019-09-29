using Godot;
using System;
using System.Collections.Generic;

public class PersonData
{
    public Vector2 position;
    public float elev;
    public PersonAction personAction = PersonAction.Idle;
    public int pathIndex = 0;
    public float walkSpeed = 2;
    public Tile currentTile;
    public string personId;
    public bool hasSpouse;
    public string spouseId;
    public string motherId;
    public string fatherId;
    public List<string> childrenIds;
    public string houseId;
    public string workPlaceId;
}
