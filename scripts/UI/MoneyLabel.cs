using Godot;
using System;

public partial class MoneyLabel : Label
{
    private GameState gm;

    public override void _Ready()
    {
        gm = GetNode<GameState>("/root/GameState");
        gm.MoneyChanged += OnMoneyChanged;
        OnMoneyChanged(gm.Money);
    }

    public override void _ExitTree()
    {
        gm.MoneyChanged -= OnMoneyChanged;
    }

    private void OnMoneyChanged(int money)
    {
        Text = $"${money}";
    }


}
