using Godot;
using LD60.Dialogue;

public partial class Ending_Mystery : Node
{
    [Export] public DialogueRunner Runner { get; set; }
    [Export] public TextureRect Background { get; set; }
    [Export] public AudioStreamPlayer RadioSFX { get; set; }

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
        if (name == "changebg_Black")
            Background.Texture = null;
        if (name == "changebg_Crucified")
            Background.Texture = (Texture2D)GD.Load("res://assets/dialogue/endings/partial mystery ending/BGCrucified.png");
        if (name == "changebg_MysteriousMan")
            Background.Texture = (Texture2D)GD.Load("res://assets/art/Characters/scary guy.png");
        if (name == "changebg_ForestFire")
            Background.Texture = (Texture2D)GD.Load("res://assets/dialogue/endings/partial mystery ending/BG_ForestFire.png");
        if (name == "start_radio_sfx")
            RadioSFX.Play();
        if (name == "end_radio_sfx")
            RadioSFX.Stop();
    }
}
