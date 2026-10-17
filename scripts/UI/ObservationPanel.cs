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

    public Texture2D RollImperfection(bool isFake, RandomNumberGenerator rng)
    {
        if(isFake)
            return ImperfectionTextures[rng.RandiRange(0, ImperfectionTextures.Length - 1)];
        if(rng.RandiRange(0, 100) < 50)
            return RedHerringTextures[rng.RandiRange(0, RedHerringTextures.Length - 1)];
        return null;
    }

    public void OpenObservation(Texture2D itemTexture, Texture2D imperfection)
    {
        itemText.Texture = itemTexture;
        imperfectionText.Texture = imperfection;
        imperfectionText.Visible = imperfection != null;
        this.Visible = true;
    }
}
