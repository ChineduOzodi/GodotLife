using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathfinding
{

    public partial class PathRequestManager
    {
        private static PathRequestManager _instance;
        public static PathRequestManager Instance { get => _instance ??= new(); }

        private readonly Queue<PathRequest> pathRequestQueue = new();
        private PathRequest currentPathRequest;
        private readonly Dictionary<String, PathNodeFound> paths = [];
        private readonly float updateTime = 10;

        readonly Pathfinder pathfinding = Pathfinder.Instance;

        bool isProcessingPath;

        public void RequestPath(string pathStart, string pathEnd, Action<PathNode[], bool> callback)
        {
            PathRequest newRequest = new(pathStart, pathEnd, callback);
            pathRequestQueue.Enqueue(newRequest);
            //Console.WriteLine($"Path requested, queue: {Instance.pathRequestQueue.Count}");
            TryProcessNext();
        }

        void TryProcessNext()
        {
            if (!isProcessingPath && pathRequestQueue.Count > 0)
            {
                //Console.WriteLine($"PathRequestManager: Finding path start ({currentPathRequest.pathStart.x},{currentPathRequest.pathStart.y}) to ({currentPathRequest.pathEnd.x},{currentPathRequest.pathEnd.y})");
                currentPathRequest = pathRequestQueue.Dequeue();

                if (paths.ContainsKey($"({currentPathRequest.pathStart}),({currentPathRequest.pathEnd})"))
                {
                    PathNodeFound found = paths[$"({currentPathRequest.pathStart}),({currentPathRequest.pathEnd})"];
                    if (found.lastUpdated + updateTime > Time.GetUnixTimeFromSystem() || pathRequestQueue.Count > 10)
                    {
                        //Console.WriteLine("Path found in mem");
                        FinishedProcessingPath(found.path, true);
                    } else
                    {
                        isProcessingPath = true;
                        pathfinding.StartFindPath(
                            currentPathRequest.pathStart,
                            currentPathRequest.pathEnd,
                            FinishedProcessingPath
                            );
                    }
                }
                else
                {
                    isProcessingPath = true;
                    pathfinding.StartFindPath(
                        currentPathRequest.pathStart,
                        currentPathRequest.pathEnd,
                        FinishedProcessingPath
                        );
                }
            }
        }

        public void FinishedProcessingPath(PathNode[] path, bool success)
        {
            if (isProcessingPath)
            {
                paths[$"({currentPathRequest.pathStart}),({currentPathRequest.pathEnd})"] =
                    new PathNodeFound(path, Godot.Time.GetUnixTimeFromSystem());
                //Console.WriteLine("Path added to mem");
            }
            //Console.WriteLine($"PathRequestManager: Finding path finished: {pathRequestQueue.Count}");
            currentPathRequest.callback(path, success);
            isProcessingPath = false;
            TryProcessNext();
        }

        struct PathRequest(string _start, string _end, Action<PathNode[], bool> _callback)
        {
            public string pathStart = _start;
            public string pathEnd = _end;
            public Action<PathNode[], bool> callback = _callback;
        }
    }

    public partial class PathNodeFound
    {
        public PathNode[] path;
        public double lastUpdated;

        public PathNodeFound() { }
        public PathNodeFound(PathNode[] path, double lastUpdated)
        {
            this.path = path;
            this.lastUpdated = lastUpdated;
        }
    }
}


