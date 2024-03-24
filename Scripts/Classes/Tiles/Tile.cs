using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Life.Scripts.Classes.Tiles
{
    public abstract class Tile
    {
        public readonly Vector2I tilePosition;

        public Tile(int X, int Y)
        {
            tilePosition = new Vector2I(X, Y);
        }
    }
}
