using Godot;
using System;

public partial class NewGameButton : Button
{
    public override void _Ready()
    {
        this.Pressed += OnNewGamePressed;
    }

    private void OnNewGamePressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/shop_loop.tscn");
    }
}
