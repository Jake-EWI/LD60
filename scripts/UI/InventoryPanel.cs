using Godot;
using System;

public partial class InventoryPanel : Control
{
    [Signal] public delegate void GoBackPressedEventHandler();
    [Export] public Vector2 IconSize { get; set; } = new(64, 64);

    private GameState gm;
    private GridContainer grid;
    private Button goBackButton;

    public override void _Ready()
    {
        gm = GetNode<GameState>("/root/GameState");
        grid = GetNode<GridContainer>("%Grid");
        goBackButton = GetNode<Button>("GoBackButton");
        VisibilityChanged += Refresh;
        goBackButton.Pressed += () => EmitSignal(SignalName.GoBackPressed);
        Refresh();
    }

    public void Refresh()
    {
        if (!Visible)
            return;

        foreach (var child in grid.GetChildren())
            child.QueueFree();

        foreach (var item in gm.Inventory)
        {
            grid.AddChild(new TextureRect
            {
                Texture = item.Item.Icon,
                CustomMinimumSize = IconSize,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                MouseFilter = MouseFilterEnum.Stop,
                TooltipText = $"{item.Item.ItemName}\n${item.Item.BaseValue}"
            });
        }
    }
}
