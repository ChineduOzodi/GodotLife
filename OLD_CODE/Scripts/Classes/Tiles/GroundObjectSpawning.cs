using Godot.Collections;

namespace OldCode.Tiles
{

    public static class GroundObjectFactory
    {
        static readonly GroundObjectSpawning TREE_SPAWN = new(
        "Tree",
        elevDiffSpawnRate: null,
        moistureSpawnRate: new(1, 0.2f),
        floorTileTypes: [FloorTileType.land],
        floorTypesAreInclusive: true,
        scale: .75f
        );

        static readonly GroundObjectSpawning ROCK_SPAWN = new(
        "Rock",
        elevDiffSpawnRate: new(100),
        moistureSpawnRate: null,
        floorTileTypes: [FloorTileType.land, FloorTileType.snow],
        floorTypesAreInclusive: true
        );

        public static GroundObjectSpawning[] GetGroundObjectSpawnings()
        {
            return
            [
                GroundObjectFactory.TREE_SPAWN,
                GroundObjectFactory.ROCK_SPAWN,
            ];
        }
    }

    public readonly struct GroundObjectSpawning(string name, SpawnRate? elevDiffSpawnRate, SpawnRate? moistureSpawnRate, Array<FloorTileType> floorTileTypes, bool floorTypesAreInclusive = true, float scale = 0, bool spawnOnRiver = false)
    {
        public string Name { get; } = name;
        public SpawnRate? ElevDiffSpawnRate { get; } = elevDiffSpawnRate;
        public SpawnRate? MoistureSpawnRate { get; } = moistureSpawnRate;
        public Array<FloorTileType> FloorTileTypes { get; } = floorTileTypes;
        public bool FloorTypesAreInclusive { get; } = floorTypesAreInclusive;
        public float Scale { get; } = scale;
        public bool SpawnOnRiver { get; } = spawnOnRiver;
    }

    public readonly struct SpawnRate(float rate, float min = 0, float max = float.MaxValue)
    {
        public float Rate { get; } = rate;
        public float Min { get; } = min;
        public float Max { get; } = max;
    }
}
