using Godot;
using LD60.Dialogue;

public partial class Ending_Rauz : Node
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
        if (name == "changebg_VHSTape")
            Background.Texture = (Texture2D)GD.Load("res://assets/dialogue/endings/full mystery ending/VHSTape.png");
        if (name == "changebg_BlankTV")
            Background.Texture = (Texture2D)GD.Load("res://assets/dialogue/endings/full mystery ending/BlankTV.png");
        if (name == "changebg_Pain")
            Background.Texture = (Texture2D)GD.Load("res://assets/dialogue/endings/full mystery ending/Pain.png");
        if (name == "changebg_Entity")
            Background.Texture = (Texture2D)GD.Load("res://assets/dialogue/endings/full mystery ending/Entity.png");
        if (name == "changebg_Black")
            Background.Texture = null;
    }
}
