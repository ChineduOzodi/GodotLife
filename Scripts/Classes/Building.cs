using Godot;
using System;


namespace Life.Scripts.Classes
{
    public class Building
    {
        public string name;
        public Type type;
        public int capacity;

        public enum Type
        {
            Residential,
            Commercial,
            Industrial
        }
    }


}
