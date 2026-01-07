using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldCode.Tiles
{
    public class GroundTile : Tile
    {
        GroundTileType type = GroundTileType.None;
        GroundObject obj = null;

        public GroundTileType Type { get { return type; } }
        public GroundObject Obj { get { return obj; } }

        public GroundTile(int X, int Y) : base(X, Y)
        {
        }

        public void AddTree()
        {
            obj = new TreeObject(this.coord, 0.5f, 0, new(1,15));
            type = GroundTileType.Vegetation;
        }

        public void AddRock()
        {
            obj = new RockObject(this.coord, 0, 0, new(5, 11));
        }

        public bool HasObject()
        {
            return obj != null;
        }

        public void RemoveObj()
        {
            obj = null;
            type = GroundTileType.None;
        }
    }


    public enum GroundTileType
    {
        None = 0,
        Vegetation = 1,
        Rock = 2,
        Building = 3,
    }
}
