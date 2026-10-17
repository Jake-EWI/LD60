using Godot;
using System;

public partial class CustomerImage : Control
{
    [Signal] public delegate void EnteredEventHandler();
    [Signal] public delegate void LeftEventHandler();

    [Export] public int Steps { get; set; } = 4;
    [Export] public float StepTime { get; set; } = 0.3f;
    [Export] public float StepHeight { get; set; } = 10f;
    [Export] public float RevealTime { get; set; } = 0.6f;
    private AudioStreamPlayer StepSound;
    private TextureRect CustomerText;
    private Vector2 RestPosition;

    public override void _Ready()
    {
        CustomerText = GetNode<TextureRect>("CustomerImage");
        StepSound = GetNode<AudioStreamPlayer>("StepSound");
        RestPosition = CustomerText.Position;
        CustomerText.PivotOffset = new Vector2(CustomerText.Size.X / 2, CustomerText.Size.Y);
        CustomerText.Modulate = new Color(0, 0, 0, 0);
    }

    public void Enter(Texture2D texture)
    {
        CustomerText.Texture = texture;
        CustomerText.Modulate = new Color(0, 0, 0, 0);
        CustomerText.Scale = Vector2.One * 0.85f;
        PlaySteps();

        var tween = CreateTween();
        tween.TweenProperty(CustomerText, "modulate", Colors.Black, Steps * StepTime);
        tween.Parallel().TweenProperty(CustomerText, "scale", Vector2.One, Steps * StepTime);
        tween.TweenProperty(CustomerText, "modulate", Colors.White, RevealTime);
        tween.Finished += () => EmitSignal(SignalName.Entered);
    }

    public void Leave()
    {
        PlaySteps();

        var tween = CreateTween();
        tween.TweenProperty(CustomerText, "modulate", Colors.Black, RevealTime);
        tween.TweenProperty(CustomerText, "modulate", new Color(0, 0, 0, 0), Steps * StepTime);
        tween.Parallel().TweenProperty(CustomerText, "scale", Vector2.One * 0.85f, Steps * StepTime);
        tween.Finished += () => EmitSignal(SignalName.Left);
    }

    public void PlaySteps()
    {
        var tween = CreateTween();
        for (int i = 0; i < Steps; i++)
        {
            tween.TweenCallback(Callable.From(PlayStepSound));
            tween.TweenProperty(CustomerText, "position:y", RestPosition.Y - StepHeight, StepTime / 2).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
            tween.TweenProperty(CustomerText, "position:y", RestPosition.Y, StepTime / 2).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
        }
    }

    public void PlayStepSound()
    {
        StepSound.PitchScale = (float)GD.RandRange(0.9, 1.1);
        StepSound.Play();
    }

}
