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
    private Label refName;
    private Label refId;
    private Label refWeight;
    private Label refMaterial;
    private Label refContains;
    private TextureRect referenceSheet;
    private AudioStreamPlayer paperAudio;
    private Tween referenceTween;
    [Export] public float ReferencePeekHeight { get; set; } = 45f;
    [Export] public float ReferenceSlideTime { get; set; } = 0.25f;
    private static readonly string[] ContainsText = ["No Power Source", "Battery", "Power Supply"];
    private ItemInstance item;
    private ObservationCheck? activeCheck;

    [Export] public int MinFakeFails { get; set; } = 1;
    [Export] public int MaxFakeFails { get; set; } = 3;
    [Export] public float RealFailChance { get; set; } = 0.35f;
    [Export] public ShaderMaterial ThermalMaterial { get; set; }
    [Export] public Color UVTint { get; set; } = new(0.6f, 0.4f, 1f);
    [Export] public float UVZoomWidth { get; set; } = 0.4f;
    [Export] public float UVRevealTime { get; set; } = 1f;
    private Control uvView;
    private TextureRect uvZoom;
    private Label uvSerial;
    private Tween uvTween;
    private AudioStreamPlayer uvAudio;
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

        scaleImage = GetNode<TextureRect>("Panel/ScaleRect");
        refName = GetNode<Label>("ReferenceSheet/VBoxContainer/RefNameLabel");
        refId = GetNode<Label>("ReferenceSheet/VBoxContainer/RefContainsLabel2");
        refWeight = GetNode<Label>("ReferenceSheet/VBoxContainer/RefWeightLabel");
        refMaterial = GetNode<Label>("ReferenceSheet/VBoxContainer/RefMaterialLabel");
        refContains = GetNode<Label>("ReferenceSheet/VBoxContainer/RefContainsLabel");
        referenceSheet = GetNode<TextureRect>("ReferenceSheet");
        paperAudio = GetNode<AudioStreamPlayer>("PaperSFX");
        uvAudio = GetNode<AudioStreamPlayer>("UVSFX");
        referenceSheet.MouseEntered += () =>
        {
            paperAudio.Play();
            SlideReferenceSheet(Size.Y - referenceSheet.Size.Y);
        };
        referenceSheet.MouseExited += () => SlideReferenceSheet(Size.Y - ReferencePeekHeight);
        uvView = GetNode<Control>("Panel/UVView");
        uvZoom = GetNode<TextureRect>("Panel/UVView/UVZoom");
        uvSerial = GetNode<Label>("Panel/UVView/UVSerialLabel");
        uvButton.Pressed += () => ToggleCheck(ObservationCheck.UV);
        thermalButton.Pressed += () => ToggleCheck(ObservationCheck.Thermal);
        thumpButton.Pressed += () => PlayThump();
        weighButton.Pressed += () => ToggleCheck(ObservationCheck.Weigh);
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
        item.UVSerial = RollUVSerial(item, rng);
        item.MeasuredWeight = RollWeight(item, rng);
    }

    private float RollWeight(ItemInstance item, RandomNumberGenerator rng)
    {
        float offset = rng.RandfRange(0f, 0.03f);
        if (item.FailedChecks.Contains(ObservationCheck.Weigh))
            offset = item.IsFake ? rng.RandfRange(0.1f, 0.25f) : rng.RandfRange(0.04f, 0.07f);
        if (rng.Randf() < 0.5f)
            offset = -offset;
        return item.Item.Weight * (1 + offset);
    }

    private string RollUVSerial(ItemInstance item, RandomNumberGenerator rng)
    {
        if (!item.FailedChecks.Contains(ObservationCheck.UV))
            return item.Serial;

        var serial = item.Serial.ToCharArray();
        int prefixLength = item.Item.SerialPrefix.Length;
        if (!item.IsFake)
        {
            for (int i = 0; i < 2; i++)
                serial[rng.RandiRange(prefixLength + 1, serial.Length - 1)] = '_';
            return new string(serial);
        }

        int roll = rng.RandiRange(0, 2);
        if (roll == 0)
            return "";
        if (roll == 1)
            return item.Item.GenerateSerial(rng);
        Array.Reverse(serial, 0, prefixLength);
        return new string(serial);
    }

    public void OpenObservation(ItemInstance item)
    {
        this.item = item;
        itemText.Texture = item.Item.Icon;
        refName.Text = $"Name: {item.Item.ItemName}";
        refId.Text = $"ID: {item.Item.SerialPrefix}-######";
        refWeight.Text = $"Average Weight: {item.Item.Weight * 1000:0}g";
        refMaterial.Text = $"Material: {item.Item.Material}";
        refContains.Text = $"Contains: {ContainsText[(int)item.Item.Thermal]}";
        referenceTween?.Kill();
        referenceSheet.Position = new Vector2(referenceSheet.Position.X, Size.Y - ReferencePeekHeight);
        activeCheck = null;
        ApplyCheck();
        this.Visible = true;
    }

    private void SlideReferenceSheet(float y)
    {
        referenceTween?.Kill();
        referenceTween = CreateTween();
        referenceTween.TweenProperty(referenceSheet, "position:y", y, ReferenceSlideTime).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
    }

    public void ToggleCheck(ObservationCheck check)
    {
        activeCheck = activeCheck == check ? null : check;
        ApplyCheck();
    }

    private void ApplyCheck()
    {
        scaleImage.Visible = activeCheck == ObservationCheck.Weigh;
        uvView.Visible = activeCheck == ObservationCheck.UV;
        itemText.Material = null;
        if (activeCheck == ObservationCheck.UV)
        {
            ShowUV();
        }
        else if (activeCheck == ObservationCheck.Thermal)
        {
            itemText.Material = ThermalMaterial;
            ThermalMaterial.SetShaderParameter("heat", GetHeat());
        }
        if (activeCheck == ObservationCheck.Weigh)
        {
            ShowWeigh();
        }
    }

    private void ShowUV()
    {
        var iconSize = item.Item.Icon.GetSize();
        var regionSize = new Vector2(iconSize.X * UVZoomWidth, iconSize.X * UVZoomWidth * uvView.Size.Y / uvView.Size.X);
        uvZoom.Texture = new AtlasTexture
        {
            Atlas = item.Item.Icon,
            Region = new Rect2((iconSize - regionSize) / 2, regionSize)
        };
        uvZoom.Modulate = UVTint;

        uvSerial.Text = item.UVSerial;
        uvView.Modulate = Colors.Transparent;
        uvAudio.Play();
        uvTween?.Kill();
        uvTween = CreateTween();
        uvTween.TweenProperty(uvView, "modulate", Colors.White, UVRevealTime);
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
        activeCheck = null;
        ApplyCheck();
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

    public void ShowWeigh()
    {
        scaleValue.Text = $"{item.MeasuredWeight * 1000:0} g";
    }
}
