using Godot;
using System;

public partial class OfferedItem : Control
{
    private TextureRect itemTexture;
    private Label itemLabel;

    public override void _Ready()
    {
        itemTexture = GetNode<TextureRect>("ItemTexture");
        itemLabel = GetNode<Label>("ItemLabel");
    }

    public void DisplayItem(Texture2D texture, string itemName)
    {
        itemTexture.Texture = texture;
        this.Visible = true;
    }
}
