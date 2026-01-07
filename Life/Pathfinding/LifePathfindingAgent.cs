using Godot;
using Pathfinding;
using System;
using WorldGen;

namespace Life.Pathfinding;

[GlobalClass]
public partial class LifePathfindingAgent : PathfindingAgent
{
    [Export]
    World World { get; set; }

    public override void _Ready()
    {
        World ??= GetNode<World>("/root/Main/World");
    }

    public override string PositionToPositionName(Vector2 position)
    {
        Vector2I coords = World.GroundLayer.LocalToMap(GetParent<Node2D>().ToLocal(position));
        return $"{coords.X} {coords.Y}";
    }
}
