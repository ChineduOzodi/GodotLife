using Godot;
using Godot.Collections;
using System;

namespace InventoryManagement;

[GlobalClass]
public partial class Inventory : Node
{
    [Export]
    public Dictionary<string, InventoryItem> Items { get; set; } = [];
}
