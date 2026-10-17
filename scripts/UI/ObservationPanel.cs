using Godot;
using System;

public partial class ObservationPanel : Control
{
    private TextureRect itemText;
    private TextureRect imperfectionText;
    [Export] public Texture2D[] ImperfectionTextures;
    [Export] public Texture2D[] RedHerringTextures;

    public override void _Ready()
    {
        itemText = GetNode<TextureRect>("Panel/ItemTexture");
        imperfectionText = GetNode<TextureRect>("Panel/ImperfectionTexture");
    }

    public void OpenObservation(bool isFake, Texture2D itemTexture, RandomNumberGenerator rng)
    {
        itemText.Texture = itemTexture;
        if(isFake)
        {
            imperfectionText.Texture = ImperfectionTextures[rng.Randi() % ImperfectionTextures.Length];
            imperfectionText.Visible = true;
        }
        else
        {
            int redHerringChance = rng.RandiRange(0, 100);
            if(redHerringChance < 50)
            {
                imperfectionText.Texture = RedHerringTextures[rng.Randi() % RedHerringTextures.Length];
                imperfectionText.Visible = true;
            }
            else
            {
                imperfectionText.Visible = false;
            }

            imperfectionText.Visible = false;
        }
        this.Visible = true;
    }
}
