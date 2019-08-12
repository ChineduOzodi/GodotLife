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
    private Tile currentTile;
    private float elev;
    
    public PathNode[] path;

    public PersonAction PersonAction { get => personAction; }
    public int PathIndex { get => pathIndex; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        world = GetParent<World>();
        pathRequestManager = GetNode<PathRequestManager>(new NodePath("../PathRequestManager"));
        currentTile = world.GetTile(GetPosition());
        elev = currentTile.elev;
    }
    
    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        switch (personAction)
        {
            case PersonAction.Idle:
                personAction = PersonAction.Waiting;
                PathRequestManager.RequestPath(currentTile.position, GotoCity(), FoundPath);
                break;
            //case PersonAction.Waiting:
            //    if (path != null)
            //    {
            //        personAction = PersonAction.Moving;
            //    }
            //    break;
            case PersonAction.Moving:
                
                if (pathIndex < path.Length)
                {
                    Tile tile = world.GetTile(path[pathIndex].worldPosition);
                    float remainingDelta = delta;

                    while (remainingDelta > 0) {
                        float distanceIndex;
                        float distanceDelta = tile.speedMod * walkSpeed * delta * world.TileSize * tile.riverCrossingSpeed;
                        if ((currentTile.biome == TileType.Water || currentTile.biome == TileType.SeaIce) && (tile.biome == TileType.SeaIce || tile.biome == TileType.Water))
                        {
                            distanceIndex = GetPosition().DistanceTo(path[pathIndex].worldPosition);
                        } else
                        {
                            distanceIndex = GetPosition().DistanceTo(path[pathIndex].worldPosition)  + (Mathf.Abs(elev - tile.elev) * world.ElevChangeCost * world.TileSize);
                            //Console.WriteLine($"distanceIndex: {distanceIndex}");
                            //Console.WriteLine($"deltatIndex: {distanceDelta}");
                        }

                        if (distanceDelta <= distanceIndex) {

                            if ((currentTile.biome == TileType.Water || currentTile.biome == TileType.SeaIce) && (tile.biome == TileType.SeaIce || tile.biome == TileType.Water))
                            {
                                SetPosition(GetPosition().LinearInterpolate(path[pathIndex].worldPosition, (distanceDelta) / distanceIndex));
                            }
                            else
                            {
                                float interpolation = (distanceDelta) / distanceIndex;
                                SetPosition(GetPosition().LinearInterpolate(path[pathIndex].worldPosition, interpolation));
                                elev += (tile.elev - elev) * interpolation;
                                //Console.WriteLine($"Interpolation: {(distanceDelta) / distanceIndex}");
                            }
                            
                            remainingDelta = 0;
                        } else {
                            if (tile.biome == TileType.land || tile.biome == TileType.snow)
                            {                                
                                tile.speedMod += .02f * walkSpeed;
                                if (tile.speedMod > tile.maxSpeedMod)
                                {
                                    tile.speedMod = tile.maxSpeedMod;
                                }
                            }
                            pathIndex++;
                            if (pathIndex >= path.Length) {
                                SetPosition(tile.position);
                                remainingDelta = 0;
                                break;
                            }
                            currentTile = tile;
                            elev = currentTile.elev;
                            tile = world.GetTile(path[pathIndex].worldPosition);
                            remainingDelta = distanceIndex / ( distanceDelta / delta);

                            if (tile.speedMod > tile.baseSpeedMod)
                            {
                                tile.speedMod -= (float) (world.Time - tile.lastUpdated) * tile.recoveryRate;
                                if (tile.speedMod < tile.baseSpeedMod)
                                    tile.speedMod = tile.baseSpeedMod;
                                float mod = (tile.speedMod - tile.baseSpeedMod) / (tile.maxSpeedMod - tile.baseSpeedMod);
                                tile.lastUpdated = world.Time;

                            }

                        }
                    }
                } else
                {
                    //reached goal
                    pathIndex = 0;
                    path = null;
                    //Console.WriteLine("reached goal");
                    personAction = PersonAction.Idle;
                }
                break;
        }
    }

    private Vector2 GotoCity()
    {
        float[] distances = new float[world.cities.Count];
        float totalDistance = 0;
        for (int i = 0; i < world.cities.Count; i++)
        {
            City selectedCity = world.cities[i];
            if (selectedCity.GetPosition().Equals(currentTile.position))
            {
                distances[i] = 0;
            } else
            {
                if (currentTile.distanceToLoctaion.ContainsKey($"{selectedCity.GetPosition().ToString()}"))
                {
                    distances[i] = 10000/ Mathf.Pow(currentTile.distanceToLoctaion[$"{selectedCity.GetPosition().ToString()}"],2);
                } else
                {
                    distances[i] = 10000/ Mathf.Pow(selectedCity.GetPosition().DistanceTo(GetPosition()),2);
                }
                
            }
            totalDistance += distances[i];
        }

        float randomNum = world.Random.Randf() * totalDistance;

        for (int i = 0; i < world.cities.Count; i++)
        {
            //Console.WriteLine($"City probability: {distances[i] / totalDistance}")
            if (distances[i] >= randomNum)
            {
                return world.cities[i].GetPosition();

            } else
            {
                randomNum -= distances[i];
            }
        }

        Console.WriteLine("ERROR: DID NOT FIND WEIGHTED CITY");

        City city = world.cities[world.Random.RandiRange(0, world.cities.Count - 1)];
        int tries = 0;
        while (city.GetPosition().Equals(currentTile.position))
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
            pathIndex = 0;
            personAction = PersonAction.Moving;
            //Console.WriteLine("Found path, moving");
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
