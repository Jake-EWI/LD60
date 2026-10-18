using Godot;
using LD60.Dialogue;

public partial class Ending_Escape : Node
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
        if (name == "changebg_Money")
            Background.Texture = (Texture2D)GD.Load("res://assets/dialogue/endings/escaped ending/bgMoney.png");
        if (name == "changebg_Beach")
            Background.Texture = (Texture2D)GD.Load("res://assets/dialogue/endings/escaped ending/bgBeach.png");
        if (name == "changebg_SmuggleShip")
            Background.Texture = (Texture2D)GD.Load("res://assets/dialogue/endings/escaped ending/bgSmuggleShip.png");
        if (name == "changebg_Sunrise")
            Background.Texture = (Texture2D)GD.Load("res://assets/dialogue/endings/escaped ending/bgSunrise.png");
        if (name == "changebg_Ruined")
            Background.Texture = (Texture2D)GD.Load("res://assets/dialogue/endings/escaped ending/bgRuined.png");
    }
}
