using Godot;
using System;
using System.Threading.Tasks;

public partial class GunPOV : Control 
{
    private TextureRect gunImage;
    private AudioStreamPlayer gunCockSound;
    private ColorRect fader;

    public override void _Ready()
    {
        gunImage = GetNode<TextureRect>("GunText");
        gunCockSound = GetNode<AudioStreamPlayer>("GunCock");
        fader = GetNode<ColorRect>("FadeToBlackLayer/ColorRect");
    }

    public async Task ShowGun()
    {
        await FadeOut();
        this.Visible = true;
        gunCockSound.Play();
        await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
        await FadeIn();
    }

    public void ShootGun()
    {

    }

    public async Task HideGun()
    {
        await FadeOut();
        this.Visible = false;
        await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
        await FadeIn();
    }

    public async Task FadeOut(float time = 0.5f)
    {
        var tween = CreateTween();
        tween.TweenProperty(fader, "modulate:a", 1f, time);
        await ToSignal(tween, Tween.SignalName.Finished);
    }

    public async Task FadeIn(float time = 0.5f)
    {
        var tween = CreateTween();
        tween.TweenProperty(fader, "modulate:a", 0f, time);
        await ToSignal(tween, Tween.SignalName.Finished);
    }
}
