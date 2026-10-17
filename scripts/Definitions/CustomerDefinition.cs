using Godot;
using System;
[GlobalClass]
public partial class CustomerDefinition : Resource
{
    [Export] public string CustomerName;
    [Export(PropertyHint.Range, "0,1")] public float BuyerChance { get; set; } = 0.5f;
    [Export] public ItemDefinition[] PossibleItems { get; set; } = [];
    [Export(PropertyHint.Range, "0,1")] public float FakeChance { get; set; } = 0.3f;
    [Export(PropertyHint.Range, "0,1")] public float Greed { get; set; } = 0.5f;
    [Export(PropertyHint.Range, "0,1")] public float Savvy { get; set; } = 0.5f;
    [Export(PropertyHint.Range, "0,100")] public float Patience {get; set;} = 100;
    [Export(PropertyHint.Range, "0,1")] public float StealChance {get; set;} = .5f;
    [Export(PropertyHint.Range, "0,1")] public float CoaChance {get; set;} = .5f;
    [Export(PropertyHint.File, "*.txt")] public string DialoguePath {get; set;} = "";
    [Export] public Texture2D CharacterImage;
}
