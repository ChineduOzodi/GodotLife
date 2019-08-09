using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Life.Scripts.Pathfinding
{
    public class Pathfinding: Node
    {

        public bool displayGizmos = false;
        PathRequestManager requestManager;
        public PackedScene pathNodeVisScene;
        private bool visualizeSearch = false;
    
        World world;
        Dictionary<String,PathNode> grid;
        List<PathNode> path;
        List<Node2D> pathVis = new List<Node2D>();
        private Dictionary<String, float> gCosts = new Dictionary<string, float>();

        public int maxLocalSize
        {
            get
            {
                return world.Width * world.Height;
            }
        }


        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            world = GetParent<World>();
            grid = new Dictionary<string, PathNode>();
            requestManager = GetNode<PathRequestManager>(new NodePath("../PathRequestManager"));
            pathNodeVisScene = ResourceLoader.Load<PackedScene>("res://Prefabs/PathNodeVis.tscn");
        }

        internal async void StartFindPath(Coord startPos, Coord targetPos)
        {
            //Console.WriteLine("Pathfinding: async path task start");
            PathNode[] path = await Task.Run( () => FindLocalPath(startPos, targetPos));

            //Console.WriteLine("Pathfinding: async path task done");

            if (path.Length > 0)
            {
                requestManager.FinishedProcessingPath(path, true);
            }else
            {
                requestManager.FinishedProcessingPath(path, false);
            }
        }

        private PathNode[] FindLocalPath(Coord startPos, Coord targetPos)
        {
            //Console.WriteLine($"Pathfinding: finding path start: ({startPos.x},{startPos.y}) to ({targetPos.x},{targetPos.y})");
            for(int i = pathVis.Count - 1; i >= 0; i--)
            {
                pathVis[i].GetChild<Sprite>(0).SetModulate(new Color(0.0f, 0, 0, 0.0f));
            }
            //pathVis = new List<Node2D>();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            PathNode[] waypoints = new PathNode[0];
            bool pathSuccess = false;

            PathNode startNode = PathNode.PathNodeFromPosition(startPos, world);
            PathNode targetNode = PathNode.PathNodeFromPosition(targetPos, world);

            startNode.coord = startPos;
            targetNode.coord = targetPos;

            //Save Nodes to Grid
            grid[$"{startPos.x}, {startPos.y}"] = startNode;
            grid[$"{targetPos.x}, {targetPos.y}"] = targetNode;

            if (visualizeSearch)
            {
                //startNode vis
                Node2D viz = GetNode<Node2D>(new NodePath($"{startPos.x}, {startPos.y}"));
                if (viz != null)
                {
                    viz.GetChild<Sprite>(0).SetModulate(new Color(0, 1, 0, 0.5f));
                } else
                {
                    viz = (Node2D)pathNodeVisScene.Instance();
                    AddChild(viz);
                    viz.GetChild<Sprite>(0).SetModulate(new Color(0, 1, 0, 0.5f));
                    viz.SetName($"{startPos.x}, {startPos.y}");
                    viz.SetPosition(startNode.worldPosition);
                    pathVis.Add(viz);
                }

                //targetNode vis
                 Node2D targetViz = GetNode<Node2D>(new NodePath($"{targetPos.x}, {targetPos.y}"));
                if (targetNode != null)
                {
                    targetViz.GetChild<Sprite>(0).SetModulate(new Color(.5f, 0, 1));
                }
                else {
                    targetViz = (Node2D)pathNodeVisScene.Instance();
                    targetViz.GetChild<Sprite>(0).SetModulate(new Color(.5f, 0, 1));
                    AddChild(targetViz);
                    targetViz.SetName($"{targetPos.x}, {targetPos.y}");
                    targetViz.SetPosition(targetNode.worldPosition);
                    pathVis.Add(targetViz);
                }
            }

            

            //Console.WriteLine($"startNode speed: {startNode.moveSpeed}, targetNode speed: {targetNode.moveSpeed}");

            if (startNode.moveSpeed > 0 && targetNode.moveSpeed > 0)
            {
                Heap<PathNode> openSet = new Heap<PathNode>(maxLocalSize);
                HashSet<PathNode> closedSet = new HashSet<PathNode>();
                openSet.Add(startNode);

                

                while (openSet.Count > 0)
                {
                    //Console.WriteLine($"Pathfinding: looking...{closedSet.Count}, {openSet.Count}");
                    PathNode currentNode = openSet.RemoveFirst();

                    closedSet.Add(currentNode);

                    if (visualizeSearch)
                    {
                        //change node color
                        Node2D viz = GetNode<Node2D>(new NodePath($"{currentNode.coord.x}, {currentNode.coord.y}"));
                        viz.GetChild<Sprite>(0).SetModulate(new Color(0.2f, 0, 0, 0.5f));
                    }
                    

                    //Console.WriteLine($"Pathfinding: current node fCost ({currentNode.fCost}) hCost ({currentNode.hCost}) gCost ({currentNode.gCost}), pos ({currentNode.coord.x},{currentNode.coord.y})");

                    if (currentNode.Equals(targetNode))
                    {
                        sw.Stop();
                        Console.WriteLine("Path found:" + sw.ElapsedMilliseconds + " ms");
                        pathSuccess = true;
                        break;
                    }

                    foreach (PathNode neighbor in GetNeighbors(currentNode))
                    {

                        if (neighbor.moveSpeed == 0 || closedSet.Contains(neighbor))
                        {
                            //Console.WriteLine($"Pathfinding: skip neighbor cost: {neighbor.moveSpeed}");
                            continue;
                        }

                        float newMovementCostToNeighbor;

                        if (currentNode.biome == TileType.Water && neighbor.biome == TileType.Water)
                        {
                            newMovementCostToNeighbor = currentNode.gCost + (GetDistance(currentNode, neighbor)) / (currentNode.moveSpeed);
                        }
                        else
                        {
                            newMovementCostToNeighbor = currentNode.gCost + (GetDistance(currentNode, neighbor) + (Mathf.Abs(currentNode.elev - neighbor.elev) * world.ElevChangeCost * world.TileSize)) / (currentNode.moveSpeed);
                        }
                        //Console.WriteLine($"Pathfinding: neighbor cost: {newMovementCostToNeighbor}");
                        if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                        {
                            gCosts[$"({neighbor.coord.x},{neighbor.coord.y})-({startNode.coord.x},{startNode.coord.y})"] = newMovementCostToNeighbor;
                            neighbor.gCost = newMovementCostToNeighbor;
                            if (gCosts.ContainsKey($"({neighbor.coord.x},{neighbor.coord.y})-({targetNode.coord.x},{targetNode.coord.y})"))
                            {
                                neighbor.hCost = gCosts[$"({neighbor.coord.x},{neighbor.coord.y})-({targetNode.coord.x},{targetNode.coord.y})"];
                            } else
                            {
                                neighbor.hCost = GetDistance(neighbor, targetNode) / 0.5f;
                            }
                            
                            neighbor.parent = currentNode;

                            if (!openSet.Contains(neighbor))
                            {
                                openSet.Add(neighbor);
                            }
                            else
                                openSet.UpdateItem(neighbor);
                        }

                    }

                }
            }

            if (pathSuccess)
            {
                world.GetTile(targetNode.worldPosition).distanceToLoctaion[$"{startNode.worldPosition.ToString()}"] = targetNode.fCost;
                world.GetTile(startNode.worldPosition).distanceToLoctaion[$"{targetNode.worldPosition.ToString()}"] = targetNode.fCost;
                waypoints = RetracePath(grid[$"{startPos.x}, {startPos.y}"], grid[$"{targetPos.x}, {targetPos.y}"]);
            }
            return waypoints;
        }

        PathNode[] RetracePath(PathNode startNode, PathNode endNode)
        {
            path = new List<PathNode>();
            PathNode[] waypoints;
            PathNode currentNode = endNode;

            while(currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            //waypoints = SimplifyPath(path);
            waypoints = path.ToArray();
            Array.Reverse(waypoints);

            return waypoints;
        }

        PathNode[] SimplifyPath(List<PathNode> path)
        {
            List<PathNode> waypoints = new List<PathNode>();

            Coord directionOld = Coord.zero;

            for (int i = 1; i < path.Count; i++)
            {
                Coord directionNew = path[i - 1].coord - path[i].coord;

                if (directionNew != directionOld)
                {
                    waypoints.Add(path[i]);
                }
                directionOld = directionNew;
            }
            return waypoints.ToArray();
        }
        int GetDistance(PathNode nodeA, PathNode nodeB)
        {
            int distX = Mathf.Abs(nodeA.coord.x - nodeB.coord.x);
            int distY = Mathf.Abs(nodeA.coord.y - nodeB.coord.y);

            if (distX > distY)
                return 14 * distY + 10 * (distX - distY);
            return 14 * distX + 10 * (distY - distX);
        }
        public List<PathNode> GetNeighbors(PathNode node)
        {
            List<PathNode> neighbors = new List<PathNode>();
            for (int x = -world.TileSize; x <= world.TileSize; x+=world.TileSize)
            {
                for (int y = -world.TileSize; y <= world.TileSize; y+=world.TileSize)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    int checkX = node.coord.x + x;
                    int checkY = node.coord.y + y;

                    //Console.WriteLine($"Pathfinding: checkX,checkY {checkX},{checkY}");

                    if (checkX >= -world.XLimit && checkX < world.XLimit && checkY >= -world.YLimit && checkY < world.YLimit)
                    {
                        if (!grid.ContainsKey($"{checkX}, {checkY}"))
                        {
                            //Console.WriteLine($"Pathfinding: grid doesn't contain check");
                            PathNode pathNode = PathNode.PathNodeFromPosition(new Coord(checkX, checkY), world);
                            neighbors.Add(pathNode);
                            grid[$"{checkX}, {checkY}"] = pathNode;

                            if (visualizeSearch)
                            {
                                Node2D viz = GetNode<Node2D>(new NodePath($"{checkX}, {checkY}"));
                                if (viz != null)
                                {
                                    viz.GetChild<Sprite>(0).SetModulate(new Color(0, 1, 0, 0.5f));
                                }
                                else
                                {
                                    viz = (Node2D)pathNodeVisScene.Instance();
                                    AddChild(viz);
                                    viz.GetChild<Sprite>(0).SetModulate(new Color(0, 1, 0, 0.5f));
                                    viz.SetName($"{checkX}, {checkY}");
                                    viz.SetPosition(pathNode.worldPosition);
                                    pathVis.Add(viz);
                                }
                            }
                        }
                        else
                        {
                            //Console.WriteLine($"Pathfinding: grid already contains neighbor");
                            neighbors.Add(grid[$"{checkX}, {checkY}"]);
                        }
                    }
                }
            }
            //Console.WriteLine($"Pathfinding: neighbors count: {neighbors.Count}");
            return neighbors;
        }
    }
}



