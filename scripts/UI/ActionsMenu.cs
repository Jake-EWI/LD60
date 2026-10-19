using Godot;
using System;

public partial class ActionsMenu : Control
{
    [Signal] public delegate void InventoryPressedEventHandler();
    [Signal] public delegate void PullGunPressedEventHandler();
    [Signal] public delegate void ObservePressedEventHandler();
    [Signal] public delegate void COAButtonPressedEventHandler();
    [Signal] public delegate void ExpertButtonPressedEventHandler();
    [Signal] public delegate void RejectButtonPressedEventHandler();
    [Signal] public delegate void TalkButtonPressedEventHandler();
    private TextureButton inventoryButton;
    private TextureButton gunButton;
    private TextureButton observeButton;
    private TextureButton coaButton;
    private TextureButton expertButton;
    private TextureButton rejectButton;
    private TextureButton talkButton;

    public override void _Ready()
    {
        inventoryButton = GetNode<TextureButton>("HBoxContainer/InventoryButton");
        gunButton = GetNode<TextureButton>("HBoxContainer/PullGunButton");
        observeButton = GetNode<TextureButton>("HBoxContainer/ObserveButton");
        coaButton = GetNode<TextureButton>("HBoxContainer/COAButton");
        expertButton = GetNode<TextureButton>("HBoxContainer/ExpertButton");
        rejectButton = GetNode<TextureButton>("HBoxContainer/RejectButton");
        talkButton = GetNode<TextureButton>("HBoxContainer/TalkButton");

        inventoryButton.Pressed += () => EmitSignal(SignalName.InventoryPressed);
        gunButton.Pressed += () => EmitSignal(SignalName.PullGunPressed);
        observeButton.Pressed += () => EmitSignal(SignalName.ObservePressed);
        coaButton.Pressed += () => EmitSignal(SignalName.COAButtonPressed);
        expertButton.Pressed += () => EmitSignal(SignalName.ExpertButtonPressed);
        rejectButton.Pressed += () => EmitSignal(SignalName.RejectButtonPressed);
        talkButton.Pressed += () => EmitSignal(SignalName.TalkButtonPressed);
    }

    public void ShowRejectButton(bool show)
    {
        rejectButton.Visible = show;
    }

    public void ShowCoaButton(bool show)
    {
        coaButton.Visible = show;
    }

    public void SetAllButtonsVisible(bool visible)
    {
        foreach (var button in new[] { inventoryButton, gunButton, observeButton, coaButton, expertButton, rejectButton, talkButton })
            button.Visible = visible;
    }

    public void ShowButton(string buttonName)
    {
        GetNode<TextureButton>($"HBoxContainer/{buttonName}").Visible = true;
    }

}
