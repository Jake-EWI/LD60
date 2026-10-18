using Godot;

public partial class SceneTransition : CanvasLayer
{
    [Export] public float FadeTime { get; set; } = 0.5f;
    private ColorRect fade;

    public override void _Ready()
    {
        Layer = 100;
        fade = new ColorRect();
        fade.Color = Colors.Black;
        fade.Modulate = Colors.Transparent;
        fade.MouseFilter = Control.MouseFilterEnum.Ignore;
        fade.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(fade);
    }

    public void ChangeScene(string path)
    {
        fade.MouseFilter = Control.MouseFilterEnum.Stop;
        Tween tween = CreateTween();
        tween.TweenProperty(fade, "modulate:a", 1f, FadeTime);
        tween.TweenCallback(Callable.From(() => GetTree().ChangeSceneToFile(path)));
        tween.TweenProperty(fade, "modulate:a", 0f, FadeTime);
        tween.TweenCallback(Callable.From(() => fade.MouseFilter = Control.MouseFilterEnum.Ignore));
    }
}
