using Godot;
using System;

public enum UpgradeType
{
    Scale,
    UVLight,
    Thermal,
    Patience,
    HaggleRoom,
    StealProtection,
    ExpertDiscount,
    BribeDiscount,
    Charity,
    ExtraCustomers
}

[GlobalClass]
public partial class UpgradeDefinition : Resource
{
    [Export] public string UpgradeName;
    [Export(PropertyHint.MultilineText)] public string Description;
    [Export] public Texture2D Icon;
    [Export] public UpgradeType Type;
    [Export] public float Amount;
    [Export] public int Cost;
    [Export] public int CostPerLevel;
    [Export] public int MaxLevel = 1;
    [Export] public bool AlwaysOffered;
    [Export] public bool IsFree;

    public int GetCost(int level) => Cost + CostPerLevel * level;
}
