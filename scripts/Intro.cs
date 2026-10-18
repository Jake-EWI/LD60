using Godot;
using LD60.Dialogue;

public partial class Intro : Node
{
    [Export] public DialogueRunner Runner { get; set; }

    public override void _Ready()
    {
        Runner.CommandIssued += OnCommand;
    }

    private void OnCommand(string name, string[] args)
    {
        if (name == "end_intro")
            GetNode<SceneTransition>("/root/SceneTransition").ChangeScene("res://scenes/shop_loop.tscn");
    }
}
