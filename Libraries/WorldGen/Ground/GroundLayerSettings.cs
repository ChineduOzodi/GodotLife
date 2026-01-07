using Godot;
using Godot.Collections;
using System;
using System.Linq;

namespace WorldGen
{
    [GlobalClass]
    public partial class GroundLayerSettings : Resource
    {

        [Export]
        public TileSet TileSet { get; set; } = GD.Load<TileSet>("res://Life/World/Assets/WorldTileSet.tres");

        [Export]
        public Dictionary<string, GroundObjectSettings> GroundObjectGen { get; set; } = new ()
        {
            { "tree", new GroundObjectSettings(new TreeGroundGen(), GD.Load<PackedScene>("res://Life/World/Ground/Tree/Tree.tscn"))},
        };
    }
}
