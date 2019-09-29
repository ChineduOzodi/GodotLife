using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Life.Scripts.Pathfinding
{
    public class PathNode : IHeapItem<PathNode>
    {
        public float moveSpeed;
        public TileType biome;
        public Vector2 worldPosition;
        public Coord coord;
        public PathNode parent;
        public float elev;

        public float gCost; //cost from node position to the start node
        public float hCost; //cost from the node position to the target node
        int heapIndex;

        public float fCost
        {
            get { return gCost + hCost; }
        }

        public int HeapIndex
        {
            get
            {
                return heapIndex;
            }

            set
            {
                heapIndex = value;
            }
        }

        public PathNode(float _walkSpeed, Vector2 _worldPosition)
        {
            moveSpeed = _walkSpeed;
            worldPosition = _worldPosition;
        }
        public static PathNode PathNodeFromPosition(Coord pos, World model)
        {
            //Console.WriteLine($"Pos. ({pos.x},{pos.y})");

            //Get correct walk speed modification
            int x = pos.x / model.TileSize - model.XOffset;
            int y = pos.y / model.TileSize - model.XOffset;
            Tile tile = model.tiles[x][y];
            float speed = tile.speedMod * tile.riverCrossingSpeed;


            Vector2 worldPosition = new Vector2(pos.x,pos.y);

            PathNode node = new PathNode(speed, worldPosition);

            node.coord = pos; //set position
            node.elev = tile.elev;
            return node;
        }

        public int CompareTo(PathNode nodeToCompare)
        {
            int compare = fCost.CompareTo(nodeToCompare.fCost);
            if (compare == 0)
            {
                compare = hCost.CompareTo(nodeToCompare.hCost);
            }
            return -compare;
        }
    }
}
