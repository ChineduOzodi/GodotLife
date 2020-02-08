using Godot;
using System;
using System.Collections.Generic;

namespace Life.Scripts.Classes
{
    public class PersonData
    {
        public string firstName;
        public string id;
        public string lastName;
        public Gender gender;
        public double birthDate; //
        public bool alive = true;
        public long deathDate;
        public Vector2 position;
        public float elev;
        public PersonAction personAction = PersonAction.Idle;
        public int pathIndex = 0;
        public float walkSpeed = 2;
        public Tile currentTile;
        public string spouseId;
        public string motherId;
        public string fatherId;
        public List<string> childrenIds;
        public string houseId;
        public string workPlaceId;
        public List<string> businessOwnerIds = new List<string>();

        public bool hasSpouse { get { return spouseId != null; } }

        public double GetAge(double Time)
        {
            return new GDate(Time - birthDate).time;
        }

        public override string ToString()
        {
            if (World.Instance != null)
            {
                return $"[ firstName: {firstName}, lastName: {lastName}, age: {(GetAge(World.Instance.Time) / GDate.Year).ToString("0.0")}, hasSpouse: {hasSpouse.ToString()} ]";
            }
            else
            {
                return $"[ firstName: {firstName}, lastName: {lastName}, birthDate: {birthDate}, hasSpouse: {hasSpouse.ToString()} ]";
            }
        }

        public CommercialBuilding GetComercialBuilding(int index)
        {
            if (businessOwnerIds.Count > 0)
            {
                string id = businessOwnerIds[index];
                return World.Instance.buildings[id] as CommercialBuilding;

            } else
            {
                throw new Exception("Person does not own commercial buildings");
            }
        }
    }

    public enum Gender
    {
        male,
        female
    }
}


