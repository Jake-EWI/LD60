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
    private Button inventoryButton;
    private Button gunButton;
    private Button observeButton;
    private Button coaButton;
    private Button expertButton;
    private Button rejectButton;
    private Button talkButton;

    public override void _Ready()
    {
        inventoryButton = GetNode<Button>("HBoxContainer/InventoryButton");
        gunButton = GetNode<Button>("HBoxContainer/PullGunButton");
        observeButton = GetNode<Button>("HBoxContainer/ObserveButton");
        coaButton = GetNode<Button>("HBoxContainer/COAButton");
        expertButton = GetNode<Button>("HBoxContainer/ExpertButton");
        rejectButton = GetNode<Button>("HBoxContainer/RejectButton");
        talkButton = GetNode<Button>("HBoxContainer/TalkButton");

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

}
