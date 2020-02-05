using Godot;
using System;
using System.Collections.Generic;

namespace Life.Scripts.Classes
{
    public class ComercialBuilding : Building
    {
        public String managerId;
        public List<String> workerIds;
        public MapResourceData requiredResource;

    }
}

