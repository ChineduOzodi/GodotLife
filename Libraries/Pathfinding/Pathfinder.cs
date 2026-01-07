using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Pathfinding
{
	/// <summary>
	/// Finds the paths from point a to b on its map. Has an instance class. Map needs to be populated with MoveSpeed before use.
	/// </summary>
    public partial class Pathfinder
	{

		private static Pathfinder _instance;
		public static Pathfinder Instance { get => _instance ??= new(); }

        public readonly Dictionary<string,MoveSpeed> map = [];
		private readonly Dictionary<string, float> gCosts = [];

		public static int MaxLocalSize { get; set; } = 10000;

		public static void SetMaxLocalSize (int width,  int height)
		{
			MaxLocalSize = width * height;
		}

		internal async void StartFindPath(string startPos, string targetPos, Action<PathNode[], bool> callback)
		{
			//Console.WriteLine("Pathfinder: async path task start");
			//PathNode[] path = await Task.Run( () => FindLocalPath(startPos, targetPos));
            PathNode[] path = FindLocalPath(startPos, targetPos);

            //Console.WriteLine("Pathfinder: async path task done");

            callback(path, path.Length > 0);
		}

		private PathNode[] FindLocalPath(string startPos, string targetPos)
		{
			//Console.WriteLine($"Pathfinder: finding path start: ({startPos.X},{startPos.Y}) to ({targetPos.X},{targetPos.Y})");
			if (startPos == targetPos) return [];

			Stopwatch sw = new();
			sw.Start();

			PathNode[] waypoints = [];
			bool pathSuccess = false;

			PathNode startNode = new(map[startPos]);
			PathNode targetNode = new(map[targetPos]);

			Dictionary<string, PathNode> pathNodes = new()
            {
				{ startNode.moveSpeed.name, startNode},
				{ targetNode.moveSpeed.name, targetNode},
			};

            //Console.WriteLine($"startNode speed: {startNode.moveSpeed}, targetNode speed: {targetNode.moveSpeed}");

            if (startNode.moveSpeed.Speed <= 0 || targetNode.moveSpeed.Speed <= 0)
			{
				Console.WriteLine($"ERROR: speed equal or less than zero: startNode speed: {startNode.moveSpeed}, targetNode speed: {targetNode.moveSpeed}");
				return waypoints;
            }

            Heap<PathNode> openSet = new(MaxLocalSize);
            HashSet<PathNode> closedSet = [];
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                //Console.WriteLine($"Pathfinder: looking...{closedSet.Count}, {openSet.Count}");
                PathNode currentNode = openSet.RemoveFirst();

                closedSet.Add(currentNode);


                //Console.WriteLine($"Pathfinder: current node fCost ({currentNode.fCost}) HCost ({currentNode.HCost}) GCost ({currentNode.GCost}), pos ({currentNode.coord.x},{currentNode.coord.y})");

                if (currentNode.Equals(targetNode))
                {
                    sw.Stop();
                    //Console.WriteLine("Path found:" + sw.ElapsedMilliseconds + " ms");
                    pathSuccess = true;
                    break;
                }

                foreach (PathNode neighbor in GetNeighbors(currentNode, pathNodes))
                {

                    if (neighbor.Speed == 0 || closedSet.Contains(neighbor))
                    {
                        //Console.WriteLine($"Pathfinder: skip neighbor cost: {neighbor.moveSpeed}");
                        continue;
                    }

                    float newMovementCostToNeighbor = currentNode.GCost + (GetDistance(currentNode, neighbor) / currentNode.Speed);
                    //Console.WriteLine($"Pathfinder: neighbor cost: {newMovementCostToNeighbor}");

                    if (newMovementCostToNeighbor < neighbor.GCost || !openSet.Contains(neighbor))
                    {
                        gCosts[$"({neighbor.Name})-({startNode.Name})"] = newMovementCostToNeighbor;
                        neighbor.GCost = newMovementCostToNeighbor;
                        if (gCosts.ContainsKey($"({neighbor.Name})-({targetNode.Name})"))
                        {
                            neighbor.HCost = gCosts[$"({neighbor.Name})-({targetNode.Name})"];
                        }
                        else
                        {
                            neighbor.HCost = GetDistance(neighbor, targetNode) / 0.5f;
                        }

                        neighbor.Parent = currentNode;

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                        else
                            openSet.UpdateItem(neighbor);
                    }

                }

            }

            if (pathSuccess)
			{
				waypoints = RetracePath(startNode, targetNode);
			}
			return waypoints;
		}

        static PathNode[] RetracePath(PathNode startNode, PathNode endNode)
		{
            List<PathNode> path = [];
			PathNode[] waypoints;
			PathNode currentNode = endNode;

			while(currentNode != startNode)
			{
				path.Add(currentNode);
				currentNode = currentNode.Parent;
			}
			//waypoints = SimplifyPath(path);
			waypoints = [.. path];
			Array.Reverse(waypoints);

            return waypoints;
		}

        static PathNode[] SimplifyPath(List<PathNode> path)
		{
			List<PathNode> waypoints = [];

			if (path.Count == 0)
			{
				return [.. waypoints];
			}

			Vector2 directionOld = Vector2.Zero;

			for (int i = 1; i < path.Count; i++)
			{
				PathNode previousNode = path[i-1];
				PathNode currentNode = path[i];

				Vector2 directionNew = previousNode.Position - currentNode.Position;

				if (directionNew != directionOld)
				{
					waypoints.Add(previousNode);
				}
				else if (previousNode.Speed != currentNode.Speed)
                {
                    waypoints.Add(previousNode);
                }
				directionOld = directionNew;
			}

			waypoints.Add(path[^1]);
			return [.. waypoints];
		}

        static float GetDistance(PathNode nodeA, PathNode nodeB)
		{
			float distX = Mathf.Abs(nodeA.moveSpeed.GlobalPosition.X - nodeB.moveSpeed.GlobalPosition.X);
            float distY = Mathf.Abs(nodeA.moveSpeed.GlobalPosition.Y - nodeB.moveSpeed.GlobalPosition.Y);

			if (distX > distY)
				return 14 * distY + 10 * (distX - distY);
			return 14 * distX + 10 * (distY - distX);
		}
		public List<PathNode> GetNeighbors(PathNode node, Dictionary<string, PathNode> pathNodes)
		{
			List<PathNode> neighbors = [];
			MoveSpeed moveSpeed = node.moveSpeed;

			foreach (var neighbor in moveSpeed.Neighbors)
			{
                if (pathNodes.TryGetValue(neighbor.name, out PathNode value))
                {
                    //Console.WriteLine($"Pathfinder: map already contains neighbor");
                    neighbors.Add(value);
                    
                }
                else
                {
                    //Console.WriteLine($"Pathfinder: map doesn't contain check");
                    PathNode pathNode = new(map[neighbor.name]);
					pathNodes[neighbor.name] = pathNode;
                    neighbors.Add(pathNode);
                }
            }

			//Console.WriteLine($"Pathfinder: Neighbors count: {Neighbors.Count}");
			return neighbors;
		}
	}
}



