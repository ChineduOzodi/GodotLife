using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Life.Scripts.Classes.Tiles
{
    public abstract class GroundObject(Vector2I tilePosition, float speedMod, int tileSetSourceId, Vector2I atlasCoords)
    {
        public readonly Vector2I tilePosition = tilePosition;
        public readonly float speedMod = speedMod;
        public readonly int tileSetSourceId = tileSetSourceId;
        public readonly Vector2I atlasCoords = atlasCoords;
    }
}
