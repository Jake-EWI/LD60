using Godot;
using System;
public enum ThermalType
{
    None,
    Low,
    Hot,
}

public enum MaterialType
{
    Plastic,
    Metal,
    Ceramic,
    Glass
}
[GlobalClass]
public partial class ItemDefinition : Resource
{
    [Export] public string ItemName;
    [Export] public int BaseValue;
    [Export] public Texture2D Icon;
    [Export] public float Weight;
    [Export] public ThermalType Thermal;
    [Export] public MaterialType Material;
    //todo: image sprite

}
