using Godot;
using System;
using System.Collections.Generic;

public enum ObservationCheck { Weigh, UV, Thermal, Thump }
public partial class ObservationPanel : Control
{
    [Signal] public delegate void GoBackPressedEventHandler();
    private TextureRect itemText;
    private Button goBackButton;
    private TextureButton weighButton;
    private TextureButton uvButton;
    private TextureButton thermalButton;
    private TextureButton thumpButton;
    private TextureRect scaleImage;
    private Label scaleValue;
    private AudioStreamPlayer thumpAudio;
    private ItemInstance item;
    private ObservationCheck? activeCheck;

    [Export] public int MinFakeFails { get; set; } = 1;
    [Export] public int MaxFakeFails { get; set; } = 3;
    [Export] public float RealFailChance { get; set; } = 0.35f;
    [Export] public ShaderMaterial UVMaterial { get; set; }
    [Export] public ShaderMaterial ThermalMaterial { get; set; }
    [Export] public float ThumpShakeStrength { get; set; } = 10f;
    [Export] public AudioStream ThumpCeramic { get; set; }
    [Export] public AudioStream ThumpMetal { get; set; }
    [Export] public AudioStream ThumpPlastic { get; set; }
    private Vector2 itemRestPosition;
    private Tween thumpTween;

    public override void _Ready()
    {
        itemText = GetNode<TextureRect>("Panel/ItemTexture");
        itemRestPosition = itemText.Position;
        goBackButton = GetNode<Button>("GoBackButton");
        goBackButton.Pressed += () => EmitSignal(SignalName.GoBackPressed);
        weighButton = GetNode<TextureButton>("VBoxContainer/WeighButton");
        scaleValue = GetNode<Label>("Panel/ScaleRect/ScaleValueLabel");
        uvButton = GetNode<TextureButton>("VBoxContainer/UVButton");
        thermalButton = GetNode<TextureButton>("VBoxContainer/ThermalButton");
        thumpButton = GetNode<TextureButton>("VBoxContainer/ThumpButton");
        thumpAudio = GetNode<AudioStreamPlayer>("ThumpSFX");

        weighButton.Pressed += () => ToggleCheck(ObservationCheck.Weigh);
        scaleImage = GetNode<TextureRect>("Panel/ScaleRect");
        scaleValue = GetNode<Label>("Panel/ScaleValueLabel");
        uvButton.Pressed += () => ToggleCheck(ObservationCheck.UV);
        thermalButton.Pressed += () => ToggleCheck(ObservationCheck.Thermal);
        thumpButton.Pressed += () => PlayThump();
    }

    public void RollChecks(ItemInstance item, RandomNumberGenerator rng)
    {
        var checks = new List<ObservationCheck>(Enum.GetValues<ObservationCheck>());
        int failCount = item.IsFake ? rng.RandiRange(MinFakeFails, MaxFakeFails) : (rng.Randf() < RealFailChance ? 1 : 0);
        for (int i = 0; i < failCount; i++)
        {
            int index = rng.RandiRange(0, checks.Count - 1);
            item.FailedChecks.Add(checks[index]);
            checks.RemoveAt(index);
        }
    }

    public void OpenObservation(ItemInstance item)
    {
        this.item = item;
        itemText.Texture = item.Item.Icon;
        activeCheck = null;
        ApplyCheck();
        this.Visible = true;
    }

    public void ToggleCheck(ObservationCheck check)
    {
        activeCheck = activeCheck == check ? null : check;
        ApplyCheck();
    }

    private void ApplyCheck()
    {
        scaleImage.Visible = activeCheck == ObservationCheck.Weigh;
        itemText.Material = null;
        if (activeCheck == ObservationCheck.UV)
        {
            itemText.Material = UVMaterial;
        }
        else if (activeCheck == ObservationCheck.Thermal)
        {
            itemText.Material = ThermalMaterial;
            ThermalMaterial.SetShaderParameter("heat", GetHeat());
        }
    }

    private float GetHeat()
    {
        int thermal = (int)item.Item.Thermal;
        if (item.FailedChecks.Contains(ObservationCheck.Thermal))
            thermal = (thermal + 1) % 3;
        return thermal / 2f;
    }

    public void PlayThump()
    {
        var material = item.Item.Material;
        if (item.FailedChecks.Contains(ObservationCheck.Thump))
            material = material == MaterialType.Plastic ? MaterialType.Ceramic : MaterialType.Plastic;
        thumpAudio.Stream = ThumpCeramic;
        if (material == MaterialType.Metal)
            thumpAudio.Stream = ThumpMetal;
        else if (material == MaterialType.Plastic)
            thumpAudio.Stream = ThumpPlastic;
        thumpAudio.PitchScale = (float)GD.RandRange(0.95, 1.05);
        thumpAudio.Play();
        thumpTween?.Kill();
        thumpTween = CreateTween();
        for (int i = 0; i < 6; i++)
        {
            var offset = new Vector2((float)GD.RandRange(-1.0, 1.0), (float)GD.RandRange(-1.0, 1.0)) * ThumpShakeStrength;
            thumpTween.TweenProperty(itemText, "position", itemRestPosition + offset, 0.04f);
        }
        thumpTween.TweenProperty(itemText, "position", itemRestPosition, 0.04f);
    }
}
