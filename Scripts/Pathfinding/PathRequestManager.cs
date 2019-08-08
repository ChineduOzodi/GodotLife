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
        private World world;
        Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
        PathRequest currentPathRequest;
        Dictionary<String, PathNodeFound> paths = new Dictionary<string, PathNodeFound>();

        static PathRequestManager instance;
        Pathfinding pathfinding;

        bool isProcessingPath;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            instance = this;
            pathfinding = GetNode<Pathfinding>(new NodePath("../Pathfinding"));
            world = GetParent<World>();
        }

        public static void RequestPath(Vector2 pathStart, Vector2 pathEnd, Action<PathNode[], bool> callback)
        {
            PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);
            instance.pathRequestQueue.Enqueue(newRequest);
            Console.WriteLine($"Path requested, queue: {instance.pathRequestQueue.Count}");
            instance.TryProcessNext();
        }

        void TryProcessNext()
        {
            if (!isProcessingPath && pathRequestQueue.Count > 0)
            {
                //Console.WriteLine($"PathRequestManager: Finding path start ({currentPathRequest.pathStart.x},{currentPathRequest.pathStart.y}) to ({currentPathRequest.pathEnd.x},{currentPathRequest.pathEnd.y})");
                currentPathRequest = pathRequestQueue.Dequeue();

                if (paths.ContainsKey($"({currentPathRequest.pathStart.ToString()}),({currentPathRequest.pathEnd.ToString()})"))
                {
                    PathNodeFound found = paths[$"({currentPathRequest.pathStart.ToString()}),({currentPathRequest.pathEnd.ToString()})"];
                    if (found.lastUpdated + 50000 > world.Time)
                    {
                        //Console.WriteLine("Path found in mem");
                        FinishedProcessingPath(found.path, true);
                    } else
                    {
                        isProcessingPath = true;
                        pathfinding.StartFindPath(Coord.Vector2ToCoord(currentPathRequest.pathStart), Coord.Vector2ToCoord(currentPathRequest.pathEnd));
                    }
                } else
                {
                    isProcessingPath = true;
                    pathfinding.StartFindPath(Coord.Vector2ToCoord(currentPathRequest.pathStart), Coord.Vector2ToCoord(currentPathRequest.pathEnd));
                }
            }
        }

        public void FinishedProcessingPath(PathNode[] path, bool success)
        {
            if (isProcessingPath)
            {
                paths[$"({currentPathRequest.pathStart.ToString()}),({currentPathRequest.pathEnd.ToString()})"] =
                    new PathNodeFound(path, world.Time);
                //Console.WriteLine("Path added to mem");
            }
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

    public class PathNodeFound
    {
        public PathNode[] path;
        public long lastUpdated;

        public PathNodeFound() { }
        public PathNodeFound(PathNode[] path, long lastUpdated)
        {
            this.path = path;
            this.lastUpdated = lastUpdated;
        }
    }
}


