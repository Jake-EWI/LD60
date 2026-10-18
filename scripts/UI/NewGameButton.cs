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
        GetNode<SceneTransition>("/root/SceneTransition").ChangeScene("res://scenes/intro.tscn");
    }
}
