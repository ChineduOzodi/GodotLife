using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldCode.Tiles
{
    public abstract class Tile(int X, int Y)
    {
        public readonly Vector2I coord = new(X, Y);
    }
}
