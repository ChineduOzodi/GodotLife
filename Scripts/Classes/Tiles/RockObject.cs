using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Life.Scripts.Classes.Tiles
{
    internal class RockObject : GroundObject
    {
        public RockObject(Vector2I tilePosition, float speedMod, int tileSetSourceId, Vector2I atlasCoords) : base(tilePosition, speedMod, tileSetSourceId, atlasCoords)
        {
        }
    }
}
