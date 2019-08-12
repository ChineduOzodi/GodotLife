using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Life.Scripts.Classes
{
    public class MapResource
    {
        private string name;
        private List<string> tags = new List<string>();
        private ResourceType resourceType;
        private float regenRate;
        private float productionDepletionRate;

        public float resourceMax;

        public string Name { get => name; set => name = value; }
        public ResourceType ResourceType { get => resourceType; }
        public float RegenRate { get => regenRate;}
        public float ProductionDepletionRate { get => productionDepletionRate; }

        public MapResource(string name, List<string> tags, ResourceType resourceType, float regenRate, float productionDepletionRate)
        {
            this.name = name;
            this.tags = tags;
            this.resourceType = resourceType;
            this.regenRate = regenRate;
            this.productionDepletionRate = productionDepletionRate;
        }

        public bool HasTag(string tag)
        {
            return tags.Contains(tag);
        }
    }

    public class MapResourceData
    {

        public string resource;
        /// <summary>
        /// Production rate per hour per person or actual amount depending on resource
        /// </summary>
        public float amount;
        public float maxResource;
        public MapResource mapResource;

        public MapResourceData() { }

        private MapResourceData(string resource, float amount, float maxResource, MapResource mapResource)
        {
            this.resource = resource;
            this.amount = amount;
            this.maxResource = maxResource;
            this.mapResource = mapResource;
        }

        public static MapResourceData CreateMinable(MapResource resource, float productionRate)
        {
            return new MapResourceData(resource.Name, productionRate, 0, resource);
        }

        public static MapResourceData CreateRooted(MapResource resource, float maxResource)
        {
            return new MapResourceData(resource.Name, maxResource, maxResource, resource);
        }

    }

    public class MapResourceTags
    {
        public const string CanBeChopped = "Can be chopped";
        public const string CanBeKilled = "Can be killed";
    }

    public class MapResourceName
    {
        public const string Rock = "Rock";
        public const string IronOre = "Iron Ore";
        public const string GoldOre = "Gold Ore";
        public const string Tree = "Tree";
        public const string FruitTree = "Fruit Tree";
        public const string BerryBush = "BerryBush";
        public const string SeaFish = "Sea Fish";
        public const string Dear = "Dear";
    }


    public enum ResourceType
    {
        Rooted,
        Animal,
        Minable
    }
}
