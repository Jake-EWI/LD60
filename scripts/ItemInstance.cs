using Godot;
using System;

public partial class ItemInstance
{
    public ItemDefinition Item { get; }
    public bool IsFake { get; set; } = false;
    public int PricePaid { get; set; }

    public ItemInstance(ItemDefinition item)
    {
        Item = item;
    }
}
