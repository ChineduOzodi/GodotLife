using Godot;
using System;
using System.Collections.Generic;

namespace Life.Scripts.Classes
{
    public class CommercialBuilding : Building
    {
        public String managerId;
        public float productionTime;
        public List<String> workerIds;
        public List<Item> requiredResources;
        public List<Item> requiredItems;
        public List<Item> producedItems;
        public List<Item> storage  = new List<Item>();


        public static CommercialBuilding CreateWoodShop(PersonData manager, Tile tile)
        {
            CommercialBuilding comercialBuilding = new CommercialBuilding();
            comercialBuilding.buildingId = "commercialbuilding " + World.Instance.Random.Randi().ToString();
            comercialBuilding.managerId = manager.id;
            comercialBuilding.capacity = 3;
            comercialBuilding.productionTime = 20;
            comercialBuilding.workerIds = new List<string>();
            comercialBuilding.requiredResources = new List<Item>()
            {
                new Item() { amount = 1, name = MapResourceName.Tree}
            };
            comercialBuilding.requiredItems = new List<Item>()
            {

            };
            comercialBuilding.producedItems = new List<Item>()
            {
                new Item() { amount = 20, name = ItemName.Wood}
            };
            manager.businessOwnerIds.Add(comercialBuilding.buildingId);
            World.Instance.buildings[comercialBuilding.buildingId] = comercialBuilding;
            tile.AddBuilding(comercialBuilding);
            return comercialBuilding;
        }

        public int AvaliableWork()
        {
            return capacity - workerIds.Count;
        }

        public void AddWorker(PersonData person)
        {
            if (AvaliableWork() > 0)
            {
                workerIds.Add(person.id);
                if (person.workPlaceId != null)
                {
                    throw new Exception("person already has a place to work");
                }
                person.workPlaceId = buildingId;
            } else
            {
                throw new Exception("building capacity already full");
            }
        }

    }
}

