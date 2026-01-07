using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pathfinding
{
	[GlobalClass]
    public abstract partial class PathfindingAgent: Node
	{

		[Export]
        public bool ShowPath { get; set; } = false;

		[Export]
		public Line2D VisualizePathLine { get; set; } = new Line2D();

		[Export]
		public float PathNodeRange { get; set; } = 0.1f;


		Vector2? _targetPosition;

		public Vector2? TargetPosition { get => _targetPosition; set => _targetPosition = SetTargetPosition(value); }

		PathNode[] path;
		int pathIndex = 0;

		TileMapLayer TileMap { get; set; }

		private Vector2? SetTargetPosition(Vector2? newTargetPosition)
		{
			if (_targetPosition == newTargetPosition) { return newTargetPosition; }

			if (newTargetPosition == null) { return newTargetPosition; }

			string pathStart = PositionToPositionName(GetParent<Node2D>().GlobalPosition);
			string pathEnd = PositionToPositionName(newTargetPosition.Value);

			PathRequestManager.Instance.RequestPath(pathStart, pathEnd, OnFoundPath);

			return newTargetPosition;
		}

		public abstract string PositionToPositionName(Vector2 position);

		public static string GetPositionName(Vector2I position)
		{
			return $"{position.X} {position.Y}";
		}

        public void OnFoundPath(PathNode[] path, bool success)
        {
			if (!success) {
                Console.WriteLine("Person did not find path");
                ResetPathfinding();
                return; 
			}

            this.path = path;
            pathIndex = 0;

            if (ShowPath)
            {
                VisualizePath(this.path);
            }
        }

		public Vector2 GetNextPathPosition()
		{
			if (_targetPosition == null || path == null)
			{
				return GetParent<Node2D>().GlobalPosition;
			}

            if (pathIndex >= path.Length)
            {
                ResetPathfinding();
				return GetParent<Node2D>().GlobalPosition;
            }

            Vector2 nextPosition = path[pathIndex].Position;

			if (GetParent<Node2D>().GlobalPosition.DistanceSquaredTo(nextPosition) <= PathNodeRange)
			{
				pathIndex++;
				return GetNextPathPosition();
			}

			return nextPosition;
		}

		public float GetPathSpeed()
		{
			if (path == null || pathIndex >= path.Length)
				return 0.0f;

			return path[pathIndex].Speed;
		}

		private void ResetPathfinding()
		{
            _targetPosition = null;
			path = [];
			pathIndex = 0;
        }


        private void VisualizePath(PathNode[] path)
		{
			GetParent().GetParent().CallDeferred("add_child", VisualizePathLine);
			VisualizePathLine.Position = Vector2.Zero;
			VisualizePathLine.Width = 5;
			VisualizePathLine.Points = path.Select(node => node.Position).ToArray();
        }

    }
}



