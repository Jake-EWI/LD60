using Godot;
using LD60.Dialogue;

public partial class Ending_Rauz : Node
{
    [Export] public DialogueRunner Runner { get; set; }
    [Export] public TextureRect Background { get; set; }
    [Export] public Control Stage { get; set; }
    [Export] public CustomerImage CustomerImage { get; set; }
    [Export] public CustomerDefinition Mystery { get; set; }
    [Export] public float MysteryRiseTime { get; set; } = 4f;

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

    private async void MysteryEnter()
    {
        Runner.Hold();
        await ToSignal(CustomerImage.RiseFromCounter(Mystery.CharacterImage, MysteryRiseTime), Tween.SignalName.Finished);
        Runner.Resume();
    }

    private async void MysteryLeave()
    {
        Runner.Hold();
        CustomerImage.SinkBelowCounter(MysteryRiseTime / 2);
        await ToSignal(CustomerImage, CustomerImage.SignalName.Left);
        Stage.Visible = false;
        Runner.Resume();
    }

    private void OnCommand(string name, string[] args)
    {
        if (name == "mystery_enter_scene")
            MysteryEnter();
        if (name == "mystery_leave")
            MysteryLeave();
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
