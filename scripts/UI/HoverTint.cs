using Godot;

public partial class HoverTint : TextureButton
{
    [Export] public Color HoverColor { get; set; } = new(0.8f, 0.8f, 0.8f);
    [Export] public Color PressedColor { get; set; } = new(0.6f, 0.6f, 0.6f);

    public override void _Ready()
    {
        MouseEntered += UpdateTint;
        MouseExited += UpdateTint;
        ButtonDown += UpdateTint;
        ButtonUp += UpdateTint;
    }

    private void UpdateTint()
    {
        SelfModulate = IsPressed() ? PressedColor : IsHovered() ? HoverColor : Colors.White;
    }
}
