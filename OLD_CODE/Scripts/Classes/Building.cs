using Godot;
using System;


namespace OldCode
{
	public partial class Building
	{
		public string name;
		public string buildingId;
		public string ownerId;
		public Type type;
		public int capacity;

		public enum Type
		{
			Residential,
			Commercial,
			Industrial
		}
	}

	public partial class BuildingConstructionPlan
	{
		public string name;
		public string[] categories;
		public int workerCapacity;
		public BuildingConstruction construction;
		public BuildingProduction[] production;

	}

	public partial class BuildingConstruction
	{
		public int duration;
		public string[] categories;
		public Material[] materials;
	}

	public partial class BuildingProduction
	{
		public string name;
		public int amount;
		public int duration;
		public string[] categories;
		public Material[] materials;
	}

	public partial class Material
	{
		public String name;
		public bool natural;
		public float amount;
	}
}
