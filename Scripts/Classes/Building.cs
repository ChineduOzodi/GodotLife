using Godot;
using System;


namespace Life.Scripts.Classes
{
	public class Building
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

	public class BuildingConstructionPlan
	{
		public string name;
		public string[] categories;
		public int workerCapacity;
		public BuildingConstruction construction;
		public BuildingProduction[] production;

	}

	public class BuildingConstruction
	{
		public int duration;
		public string[] categories;
		public Material[] materials;
	}

	public class BuildingProduction
	{
		public string name;
		public int amount;
		public int duration;
		public string[] categories;
		public Material[] materials;
	}

	public class Material
	{
		public String name;
		public bool natural;
		public float amount;
	}
}
