using Godot;
using System;

public partial class ObservationPanel : Control
{
    [Signal] public delegate void GoBackPressedEventHandler();
    private TextureRect itemText;
    private TextureRect imperfectionText;
    private Button goBackButton;
    private TextureButton weighButton;
    private TextureButton uvButton;
    private TextureButton thermalButton;
    private TextureButton thumpButton;
    private AudioStreamPlayer thumpAudio;
    private bool isFake;

    [Export] public Texture2D[] ImperfectionTextures;
    [Export] public Texture2D[] RedHerringTextures;

    public override void _Ready()
    {
        itemText = GetNode<TextureRect>("Panel/ItemTexture");
        imperfectionText = GetNode<TextureRect>("Panel/ImperfectionTexture");
        goBackButton = GetNode<Button>("GoBackButton");
        goBackButton.Pressed += () => EmitSignal(SignalName.GoBackPressed);
        weighButton = GetNode<TextureButton>("WeighButton");
        uvButton = GetNode<TextureButton>("UVButton");
        thermalButton = GetNode<TextureButton>("ThermalButton");
        thumpButton = GetNode<TextureButton>("ThumpButton");

        weighButton.Pressed += () => ToggleWeight();
        uvButton.Pressed += () => ToggleUV();
        thermalButton.Pressed += () => ToggleThermal();
        thumpButton.Pressed += () => PlayThump();
    }

    public Texture2D RollImperfection(bool isFake, RandomNumberGenerator rng)
    {
        if(isFake)
            return ImperfectionTextures[rng.RandiRange(0, ImperfectionTextures.Length - 1)];
        if(rng.RandiRange(0, 100) < 35)
            return RedHerringTextures[rng.RandiRange(0, RedHerringTextures.Length - 1)];
        return null;
    }

    public void OpenObservation(Texture2D itemTexture, Texture2D imperfection, bool isFake)
    {
        itemText.Texture = itemTexture;
        imperfectionText.Texture = imperfection;
        imperfectionText.Visible = imperfection != null;
        this.isFake = isFake;
        this.Visible = true;
    }

    public void ToggleWeight()
    {

    }

    public void ToggleUV()
    {
        
    }

    public void ToggleThermal()
    {
        
    }

    public void PlayThump()
    {
        thumpAudio.Play();
        //shake the object image

    }
}
