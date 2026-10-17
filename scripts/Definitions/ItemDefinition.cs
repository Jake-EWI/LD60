using Godot;
using System;
[GlobalClass]
public partial class ItemDefinition : Resource
{
    [Export] public string ItemName;
    [Export] public int BaseValue;
    [Export] public Texture2D Icon;
    //todo: image sprite

}
