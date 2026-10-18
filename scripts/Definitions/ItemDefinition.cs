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
    [Export] public string SerialPrefix;
    //todo: image sprite

    public string GenerateSerial(RandomNumberGenerator rng)
    {
        string serial = SerialPrefix + "-";
        for (int i = 0; i < 6; i++)
            serial += rng.RandiRange(0, 9);
        return serial;
    }
}
