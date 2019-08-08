using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Life.Scripts.Pathfinding
{

    public class PathRequestManager : Node
    {

        Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
        PathRequest currentPathRequest;

        static PathRequestManager instance;
        Pathfinding pathfinding;

        bool isProcessingPath;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            instance = this;
            pathfinding = GetNode<Pathfinding>(new NodePath("../Pathfinding"));
        }

        public static void RequestPath(Vector2 pathStart, Vector2 pathEnd, Action<PathNode[], bool> callback)
        {
            PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);

            instance.pathRequestQueue.Enqueue(newRequest);
            instance.TryProcessNext();
        }

        void TryProcessNext()
        {
            if (!isProcessingPath && pathRequestQueue.Count > 0)
            {
                Console.WriteLine($"PathRequestManager: Finding path start ({currentPathRequest.pathStart.x},{currentPathRequest.pathStart.y}) to ({currentPathRequest.pathEnd.x},{currentPathRequest.pathEnd.y})");
                currentPathRequest = pathRequestQueue.Dequeue();
                isProcessingPath = true;
                pathfinding.StartFindPath(Coord.Vector2ToCoord(currentPathRequest.pathStart), Coord.Vector2ToCoord(currentPathRequest.pathEnd));
            }
        }

        public void FinishedProcessingPath(PathNode[] path, bool success)
        {
            currentPathRequest.callback(path, success);
            isProcessingPath = false;
            TryProcessNext();
        }

        struct PathRequest
        {
            public Vector2 pathStart;
            public Vector2 pathEnd;
            public Action<PathNode[], bool> callback;

            public PathRequest(Vector2 _start, Vector2 _end, Action<PathNode[], bool> _callback)
            {
                pathStart = _start;
                pathEnd = _end;
                callback = _callback;
            }
        }
    }

}
