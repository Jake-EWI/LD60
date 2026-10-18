using Godot;
using System;
using System.Numerics;

public partial class CustomerImage : Control
{
    [Signal] public delegate void EnteredEventHandler();
    [Signal] public delegate void LeftEventHandler();

    [Export] public int Steps { get; set; } = 4;
    [Export] public float StepTime { get; set; } = 0.3f;
    [Export] public float StepHeight { get; set; } = 10f;
    [Export] public float RevealTime { get; set; } = 0.6f;
    private AudioStreamPlayer StepSound;
    public TextureRect CustomerText;
    private Godot.Vector2 RestPosition;

    public override void _Ready()
    {
        CustomerText = GetNode<TextureRect>("CustomerImage");
        StepSound = GetNode<AudioStreamPlayer>("StepSound");
        RestPosition = CustomerText.Position;
        CustomerText.PivotOffset = new Godot.Vector2(CustomerText.Size.X / 2, CustomerText.Size.Y);
        CustomerText.Modulate = new Color(0, 0, 0, 0);
    }

    public void Enter(Texture2D texture)
    {
        CustomerText.Texture = texture;
        CustomerText.Modulate = new Color(0, 0, 0, 0);
        CustomerText.Scale = Godot.Vector2.One * 0.85f;
        PlaySteps();

        var tween = CreateTween();
        tween.TweenProperty(CustomerText, "modulate", Colors.Black, Steps * StepTime);
        tween.Parallel().TweenProperty(CustomerText, "scale", Godot.Vector2.One, Steps * StepTime);
        tween.TweenProperty(CustomerText, "modulate", Colors.White, RevealTime);
        tween.Finished += () => EmitSignal(SignalName.Entered);
    }

    public void Leave()
    {
        PlaySteps();

        var tween = CreateTween();
        tween.TweenProperty(CustomerText, "modulate", Colors.Black, RevealTime);
        tween.TweenProperty(CustomerText, "modulate", new Color(0, 0, 0, 0), Steps * StepTime);
        tween.Parallel().TweenProperty(CustomerText, "scale", Godot.Vector2.One * 0.85f, Steps * StepTime);
        tween.Finished += () => EmitSignal(SignalName.Left);
    }

    public Tween RiseFromCounter(Texture2D texture, float duration)
    {
        CustomerText.Texture = texture;
        CustomerText.Modulate = Colors.White;
        CustomerText.Scale = Godot.Vector2.One;
        CustomerText.Position = RestPosition + new Godot.Vector2(0, CustomerText.Size.Y);

        var tween = CreateTween();
        tween.TweenProperty(CustomerText, "position", RestPosition, duration).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
        return tween;
    }

    public void SinkBelowCounter(float duration)
    {
        var tween = CreateTween();
        tween.TweenProperty(CustomerText, "position", RestPosition + new Godot.Vector2(0, CustomerText.Size.Y), duration).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Sine);
        tween.TweenCallback(Callable.From(() =>
        {
            CustomerText.Modulate = new Color(0, 0, 0, 0);
            CustomerText.Position = RestPosition;
        }));
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

    public void ShakeCharacter(float strength)
    {
        var tween = CreateTween();
        for (int i = 0; i < 6; i++)
        {
            var offset = new Godot.Vector2((float)GD.RandRange(-1.0, 1.0), (float)GD.RandRange(-1.0, 1.0)) * strength * 20;
            tween.TweenProperty(CustomerText, "position", RestPosition + offset, 0.04f);
        }
        tween.TweenProperty(CustomerText, "position", RestPosition, 0.04f);
    }
}
