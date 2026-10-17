using Godot;
using System;
[GlobalClass]
public partial class CustomerDefinition : Resource
{
    [Export] public string CustomerName;
    [Export] public bool IsSeller;
    [Export] public ItemDefinition Item;
    [Export] public float Greed;
    [Export] public float Patience {get; set;} = 100;
    [Export] public float StealChance {get; set;} = .5f;
    [Export(PropertyHint.File, "*.txt")] public string DialoguePath {get; set;} = "";
}
