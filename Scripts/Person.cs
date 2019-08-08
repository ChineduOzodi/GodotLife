using Godot;
using Life.Scripts.Pathfinding;
using System;

public class Person : Node2D
{
    private World world;
    private PathRequestManager pathRequestManager;
    private PersonAction personAction = PersonAction.Idle;
    private int pathIndex = 0;
    private float walkSpeed = 2;
    private Vector2 gridWorldPosition;
    
    public PathNode[] path;

    public PersonAction PersonAction { get => personAction; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        world = GetParent<World>();
        pathRequestManager = GetNode<PathRequestManager>(new NodePath("../PathRequestManager"));
        gridWorldPosition = GetPosition();
    }
    
    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        switch (personAction)
        {
            case PersonAction.Idle:
                PathRequestManager.RequestPath(gridWorldPosition, GotoCity(), FoundPath);
                personAction = PersonAction.Waiting;
                break;
            case PersonAction.Moving:
                
                if (pathIndex < path.Length)
                {
                    Tile tile = world.GetTile(path[pathIndex].worldPosition);
                    gridWorldPosition = tile.position;

                    float d = GetPosition().DistanceTo(path[pathIndex].worldPosition);
                    if (d > 5)
                    {
                        SetPosition(GetPosition().LinearInterpolate(path[pathIndex].worldPosition, (tile.speedMod * walkSpeed * delta * world.TileSize) / d));
                    }
                    else
                    {
                        SetPosition(path[pathIndex].worldPosition);
                        if (tile.biome == TileType.Grassland)
                        {
                            if (!world.roads.ContainsKey($"{gridWorldPosition.ToString()}"))
                            {
                                world.GenerateRoad(gridWorldPosition, tile);
                            }
                            
                            tile.speedMod += .02f * walkSpeed;
                            if (tile.speedMod > tile.maxSpeedMod)
                            {
                                tile.speedMod = tile.maxSpeedMod;
                            }
                        }
                        pathIndex++;
                    }
                } else
                {
                    //reached goal
                    pathIndex = 0;
                    //Console.WriteLine("reached goal");
                    personAction = PersonAction.Idle;
                }
                break;
        }
    }

    private Vector2 GotoCity()
    {
        City city = world.cities[world.Random.RandiRange(0, world.cities.Count - 1)];
        int tries = 0;
        while (city.GetPosition().Equals(gridWorldPosition))
        {
            tries++;
            city = world.cities[world.Random.RandiRange(0, world.cities.Count - 1)];
            if (tries > 100)
            {
                Console.WriteLine("Person.cs ERROR: possible loop in GotoCity function");
                break;
            }
        }
        
        return city.GetPosition();
    }

    public void FoundPath(PathNode[] path, bool success)
    {
        if (success)
        {
            this.path = path;
            personAction = PersonAction.Moving;
        } else
        {
            Console.WriteLine("Person did not find path");
            personAction = PersonAction.Idle;
        }
    }
}

public enum PersonAction
{
    Idle,
    Waiting,
    Moving,
    Done
}
