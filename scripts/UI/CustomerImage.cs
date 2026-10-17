using Godot;
using System;

public partial class CustomerImage : Control
{
    private TextureRect CustomerText;

    public override void _Ready()
    {
        CustomerText = GetNode<TextureRect>("CustomerImage");
    }

    public void ChangeCustomerTexture(Texture2D text)
    {
        if(CustomerText == null)
            return;

        CustomerText.Texture = text;
    }

}
