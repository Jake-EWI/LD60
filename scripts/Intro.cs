using Godot;
using LD60.Dialogue;

public partial class Intro : Node
{
    [Export] public DialogueRunner Runner { get; set; }
    [Export] public AudioStreamPlayer RadioSFX { get; set; }

    public override void _Ready()
    {
        Runner.CommandIssued += OnCommand;
    }

    private void OnCommand(string name, string[] args)
    {
        switch (name)
        {
            case "start_radio_sfx":
                RadioSFX.Play();
                break;
            case "end_radio_sfx":
                RadioSFX.Stop();
                break;
            case "end_intro":
                GetNode<SceneTransition>("/root/SceneTransition").ChangeScene("res://scenes/shop_loop.tscn");
                break;
        }
    }
}
