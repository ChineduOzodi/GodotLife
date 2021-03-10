using Godot;
using System;

namespace Life.Scripts.Classes
{
    public class Item
    {
        public String name;
        public float amount;
    }

    public class ItemIdentification
    {
        public String name;
        public String[] categories;
    }

    public class ItemName
    {
        public const string Rock = "Rock";
        public const string IronOre = "Iron Ore";
        public const string GoldOre = "Gold Ore";
        public const string Tree = "Tree";
        public const string FruitTree = "Fruit Tree";
        public const string BerryBush = "BerryBush";
        public const string SeaFish = "Sea Fish";
        public const string FreshWaterFish = "Fresh Water Fish";
        public const string Dear = "Dear";
        public const string Wood = "Wood";
    }
}



