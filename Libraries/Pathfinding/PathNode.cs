using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathfinding
{
    public partial class PathNode(MoveSpeed moveSpeed) : IHeapItem<PathNode>
    {
        public PathNode Parent { get; set; }

        /// <summary>
        /// Cost from node position to the start node.
        /// </summary>
        public float GCost { get; set; }

        /// <summary>
        /// Cost from the node position to the target node.
        /// </summary>
        public float HCost { get; set; }


        /// <summary>
        /// Total cost of HCost and GCost.
        /// </summary>
        public float FCost
        {
            get { return GCost + HCost; }
        }

        public string Name { get => moveSpeed.name; }

        public float Speed { get => moveSpeed.Speed; }

        public Vector2 Position { get => moveSpeed.GlobalPosition; }

        public int HeapIndex { get; set; }

        public readonly MoveSpeed moveSpeed = moveSpeed;

        public int CompareTo(PathNode nodeToCompare)
        {
            int compare = FCost.CompareTo(nodeToCompare.FCost);
            if (compare == 0)
            {
                compare = HCost.CompareTo(nodeToCompare.HCost);
            }
            return -compare;
        }
    }
}
