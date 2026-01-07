using Godot;
using System;


namespace InventoryManagement;
public partial class InventoryItem: Resource
{
    [Export]
    public string Name { get; set; }

    [Export]
    public int Amount { get; set; }

    [Export]
    public PackedScene PackedScene { get; set; }
}
