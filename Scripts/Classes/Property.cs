using Godot;
using System;
using System.Collections.Generic;

namespace Life.Scripts.Classes {
    public class Property
    {
        public string name;
        public int x;
        public int y;
        public string ownerId;
        public bool hasStructure;
        public bool hasRoad;
        public bool hasRiver;
        public bool hasBasin;
        public String buildingId;
        public float value;
        public int treeCount;
        public List<MapResourceData> resources = new List<MapResourceData>();

        public Building Building { get => World.Instance.buildings[buildingId]; }

    }
}
