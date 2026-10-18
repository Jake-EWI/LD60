using Godot;
using System;
using System.Collections.Generic;

public partial class ItemInstance
{
    public ItemDefinition Item { get; }
    public bool IsFake { get; set; } = false;
    public int PricePaid { get; set; }
    public HashSet<ObservationCheck> FailedChecks { get; } = [];
    public Coa Coa { get; set; }

    public ItemInstance(ItemDefinition item)
    {
        Item = item;
    }
}
