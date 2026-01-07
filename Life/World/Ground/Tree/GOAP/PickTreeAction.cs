using Goap;
using Godot;
using InventoryManagement;
using System;
using System.Reflection;

namespace Life.GOAP.Actions;

public partial class PickTreeAction : GoapAction
{

    private bool _isDone = false;
    public PickTreeAction(Tree tree, float distance)
    {
        target = tree;
        AddEffect("has_fruit", true);
        cost = distance;
    }
    public override bool CheckProceduralPrecondition(Node2D agent)
    {
        try
        {
            Tree tree = (Tree)target;
            return tree.TreeData.FruitCount > 0;
        } catch (TargetException)
        {
            return false;
        }
    }

    public override bool IsDone()
    {
        return _isDone;
    }

    public override bool Perform(Node2D agent)
    {
        Inventory inventory = agent.GetNode<Inventory>("Inventory");
        if (inventory == null) return false;

        try
        {
            Tree tree = GetTree();
            InventoryItem inventoryItem = new()
            {
                Name = "fruit",
                Amount = tree.TreeData.FruitCount,

            };
            inventory.Items.Add("fruit", inventoryItem);
        } catch (TargetException e)
        {
            Console.WriteLine(e.ToString());
            return false;
        }
        _isDone = true;
        return _isDone;
    }

    public override bool RequiresInRange()
    {
        return true;
    }

    public override void Reset()
    {
        _isDone = false;
    }

    private Tree GetTree()
    {
        if (target == null) throw new TargetException("No target Selected");
        if (target.GetType() != typeof(Tree)) throw new TargetException($"Wrong target type, looking for tree type but got: {target.GetType()}");

        return (Tree)target;
    }
}
