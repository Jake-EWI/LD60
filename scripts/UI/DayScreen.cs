using Godot;
using System;

public partial class DayScreen : Control
{
    [Signal] public delegate void DayScreenFinishedEventHandler();
    private Label dayLabel;
    private Label profitLossLabel;
    private Label rentLabel;
    private Label totalLabel;
    private AudioStreamPlayer daySound;

    public override void _Ready()
    {
        dayLabel = GetNode<Label>("Panel/DayLabel");
        profitLossLabel = GetNode<Label>("Panel/VBoxContainer/ProfitLossLabel");
        rentLabel = GetNode<Label>("Panel/VBoxContainer/RentLabel");
        totalLabel = GetNode<Label>("Panel/VBoxContainer/TotalLabel");
        daySound = GetNode<AudioStreamPlayer>("DaySound");
    }

    public async void ShowDayScreen(int day, int profitloss, int rent, int total)
    {
        dayLabel.Visible = true;
        daySound.Play();
        profitLossLabel.Visible = false;
        rentLabel.Visible = false;
        totalLabel.Visible = false;
        dayLabel.Text = $"Day {day - 1}";
        Show();
        await ToSignal(GetTree().CreateTimer(3f), "timeout");
        dayLabel.Text = $"Day {day}";
        await ToSignal(GetTree().CreateTimer(2f), "timeout");
        profitLossLabel.Visible = true;
        profitLossLabel.Text = $"Profit/Loss: {Money(profitloss)}";
        await ToSignal(GetTree().CreateTimer(2f), "timeout");
        rentLabel.Visible = true;
        rentLabel.Text = $"Rent: {Money(rent)}";
        await ToSignal(GetTree().CreateTimer(2f), "timeout");
        totalLabel.Visible = true;
        totalLabel.Text = $"Total: {Money(total)}";
        await ToSignal(GetTree().CreateTimer(10f), "timeout");
        Hide();
        EmitSignal(SignalName.DayScreenFinished);
    }

    private string Money(int amount) => amount < 0 ? $"-${-amount}" : $"${amount}";
}
