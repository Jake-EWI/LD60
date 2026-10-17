using Godot;
using System;
using System.Threading.Tasks;

public partial class GunPOV : Control 
{
    private TextureRect gunImage;
    private AudioStreamPlayer gunCockSound;
    private AudioStreamPlayer gunShotSound;
    private ColorRect fader;
    private ColorRect shootFlash;

    public override void _Ready()
    {
        gunImage = GetNode<TextureRect>("GunText");
        gunCockSound = GetNode<AudioStreamPlayer>("GunCock");
        gunShotSound = GetNode<AudioStreamPlayer>("GunShoot");
        fader = GetNode<ColorRect>("FadeToBlackLayer/ColorRect");
        shootFlash = GetNode<ColorRect>("FadeToBlackLayer/FlashRect");
    }

    public async Task ShowGun()
    {
        await FadeOut();
        this.Visible = true;
        gunCockSound.Play();
        await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
        await FadeIn();
    }

    public async Task ShootGun(Action onFlash)
    {
        gunShotSound.Play();
        await Flash(onFlash);
        await ToSignal(GetTree().CreateTimer(1f), SceneTreeTimer.SignalName.Timeout);
        await HideGun();
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

    public async Task Flash(Action atPeak, float time = 0.25f)
    {
        var tween = CreateTween();
        tween.TweenProperty(shootFlash, "modulate:a", 1f, time);
        tween.TweenCallback(Callable.From(atPeak));
        tween.TweenProperty(shootFlash, "modulate:a", 0f, time);
        await ToSignal(tween, Tween.SignalName.Finished);
    }
}
