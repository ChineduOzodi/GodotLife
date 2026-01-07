using Godot;
using OldCode;
using Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldGen;

internal class WorldPathfinding
{
    World World { get; set; }

    public void SetupWorldPathfinding(World world)
    {
        World = world;

        Dictionary<string, MoveSpeed> moveSpeedMap = [];

        void CreateMoveSpeed(Vector2I coords)
        {
            string name = $"{coords.X} {coords.Y}";

            MoveSpeed moveSpeed = new(name, 1f, World.ToGlobal(World.TerrainLayer.MapToLocal(coords)));

            Pathfinder.Instance.map[name] = moveSpeed;

            moveSpeed.speedMultipliers.Add("terrain", SetTerrainSpeedModifier);
            moveSpeed.speedMultipliers.Add("ground", SetGroundSpeedModifier);
        }

        void SetMoveSpeedNeighbors(Vector2I coords)
        {
            string name = $"{coords.X} {coords.Y}";
            List<MoveSpeed> neighbors = GetNeighbors(coords);

            Pathfinder.Instance.map[name].Neighbors = new HashSet<MoveSpeed>(neighbors);
        }

        World.IterateThroughTilesAndCall(CreateMoveSpeed);
        World.IterateThroughTilesAndCall(SetMoveSpeedNeighbors);
    }

    float SetTerrainSpeedModifier(MoveSpeed moveSpeed)
    {
        Vector2I coords = World.TerrainLayer.LocalToMap(World.ToLocal(moveSpeed.GlobalPosition));
        TileData tileData = World.TerrainLayer.GetCellTileData(coords);
        Variant speed = tileData.GetCustomData("move_speed");
        return speed.As<float>();
    }

    float SetGroundSpeedModifier(MoveSpeed moveSpeed)
    {
        Vector2I coords = World.GroundLayer.LocalToMap(World.ToLocal(moveSpeed.GlobalPosition));

        if (World.GroundLayerObjects.ContainsKey(coords))
        {
            return 0;
        }
        return 1;
    }

    /// <summary>
    /// Finds grid Neighbors that already exist in the Pathfinder map, which should be populated before calling this.
    /// </summary>
    /// <param name="coords"></param>
    /// <returns></returns>
    private static List<MoveSpeed> GetNeighbors(Vector2I coords)
    {
        List<MoveSpeed> neighbors = [];

        for (int nX = coords.X-1; nX <= coords.X + 1; nX++) 
        {
            for (int nY  = coords.Y-1; nY <= coords.Y + 1;nY++) 
            {
                if (nX == coords.X && nY == coords.Y)
                {
                    continue;
                }

                string neighborName = $"{nX} {nY}";

                if (Pathfinder.Instance.map.ContainsKey(neighborName))
                {
                    neighbors.Add(Pathfinder.Instance.map[neighborName]);
                }
            }
        }

        return neighbors;
    }
}
