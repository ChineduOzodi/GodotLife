using Goap;
using Godot;
using InventoryManagement;
using Life.Pathfinding;
using System;
using System.Collections.Generic;

namespace Life; 
public partial class Person : CharacterBody2D, IGoap
{

    [Export]
    float MovementSpeed { get; set; } = 50;

    [Export]
    Node2D Target { get; set; }

    [Export]
    LifePathfindingAgent NavigationAgent { get; set; }

    [Export]
    Area2D Vision {  get; set; }

    private GoapAgent goapAgent;

    private readonly Dictionary<Node2D, List<GoapAction>> actionMap = [];

    public override void _Ready()
    {
        NavigationAgent ??= GetNode<LifePathfindingAgent>("LifePathfindingAgent");
        StartNavigation();
        goapAgent = new GoapAgent(this, []);
        SetupVision(goapAgent);
    }

    private void SetupVision(GoapAgent goapAgent)
    {
        Vision ??= GetNode<Area2D>("Vision");

        void OnVisionBodyEnter(Node2D body)
        {
            if (body.GetType() == typeof(Tree))
            {
                var tree = (Tree)body;
                List<GoapAction> actions = tree.GetActions(this);
                foreach(GoapAction action in actions)
                {
                    goapAgent.AddAction(action);
                }
                actionMap[body] = actions;
            }
        }

        void OnVisionBodyExit(Node2D body)
        {
            if (!actionMap.TryGetValue(body, out List<GoapAction> value))
            {
                return;
            }

            foreach (GoapAction action in value)
            {
                goapAgent.RemoveAction(action);
            }
        }

        Vision.BodyEntered += OnVisionBodyEnter;
        Vision.BodyExited += OnVisionBodyExit;
    }



    private void StartNavigation()
    {
        if (Target != null)
        {
            NavigationAgent.TargetPosition = Target.GlobalPosition;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (NavigationAgent == null) {
            return;
        }

        if (Target == null)
        {
            return;
        }

        Vector2 nextPathPosition = NavigationAgent.GetNextPathPosition();
        Velocity = GlobalPosition.DirectionTo(nextPathPosition) * MovementSpeed * NavigationAgent.GetPathSpeed();

        MoveAndSlide();
    }

    public void ActionsFinished()
    {
        
    }

    public HashSet<KeyValuePair<string, object>> CreateGoalState()
    {
        return [new("has_fruit", true)];
    }

    public HashSet<KeyValuePair<string, object>> GetWorldState()
    {
        HashSet<KeyValuePair<string, object>> worldState = [];

        AddInventoryToWorldState(worldState);

        return worldState;
    }

    public bool MoveAgent(GoapAction nextAction)
    {
        NavigationAgent.TargetPosition = nextAction.target.GlobalPosition;
        return true;
    }

    public void PlanAborted(GoapAction aborter)
    {
        
    }

    public void PlanFailed(HashSet<KeyValuePair<string, object>> failedGoal)
    {
        
    }

    public void PlanFound(HashSet<KeyValuePair<string, object>> goal, Queue<GoapAction> actions)
    {
        
    }

    private void AddInventoryToWorldState(HashSet<KeyValuePair<string, object>> worldState)
    {
        Inventory inventory = GetNode<Inventory>("Inventory");

        foreach( var kvp in inventory.Items)
        {
            worldState.Add(new($"has_{kvp.Key}",true));
        }
    }
}
