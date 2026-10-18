using Godot;
using LD60.Dialogue;

public partial class Ending_Broke : Node
{
    [Export] public DialogueRunner Runner { get; set; }
    [Export] public TextureRect Background { get; set; }

    public override void _Ready()
    {
        Runner.CommandIssued += OnCommand;
        Runner.DialogueEnded += OnDialogueEnded;
    }

    private async void OnDialogueEnded()
    {
        await ToSignal(GetTree().CreateTimer(2), SceneTreeTimer.SignalName.Timeout);
        GetNode<SceneTransition>("/root/SceneTransition").ChangeScene("res://scenes/title_screen.tscn");
    }

    private void OnCommand(string name, string[] args)
    {
        if (name == "changebg_BrokeEnding2")
            Background.Texture = (Texture2D)GD.Load("res://assets/dialogue/endings/broke ending/Broke-Ending2.png");
        if (name == "changebg_bgexplosion")
            Background.Texture = (Texture2D)GD.Load("res://assets/dialogue/endings/broke ending/bgexplosion.png");
    }
}
