using Godot;
using System;
using System.Collections.Generic;

public partial class UpgradePanel : Control
{
    [Signal] public delegate void FinishedEventHandler();
    [Export] public int RandomOffers { get; set; } = 3;
    private HBoxContainer cards;
    private Button continueButton;
    private GameState gm;
    private List<UpgradeDefinition> offers = [];

    public override void _Ready()
    {
        gm = GetNode<GameState>("/root/GameState");
        cards = GetNode<HBoxContainer>("Panel/Cards");
        continueButton = GetNode<Button>("Panel/ContinueButton");
        continueButton.Pressed += () =>
        {
            Hide();
            EmitSignal(SignalName.Finished);
        };
    }

    public void Open(UpgradeDefinition[] pool, RandomNumberGenerator rng)
    {
        offers.Clear();
        var randomPool = new List<UpgradeDefinition>();
        foreach (var upgrade in pool)
        {
            if (gm.GetUpgradeLevel(upgrade) >= upgrade.MaxLevel)
                continue;
            if (upgrade.AlwaysOffered)
                offers.Add(upgrade);
            else
                randomPool.Add(upgrade);
        }
        for (int i = 0; i < RandomOffers && randomPool.Count > 0; i++)
        {
            int index = rng.RandiRange(0, randomPool.Count - 1);
            offers.Add(randomPool[index]);
            randomPool.RemoveAt(index);
        }
        BuildCards();
        Show();
    }

    private void BuildCards()
    {
        foreach (var child in cards.GetChildren())
            child.QueueFree();

        foreach (var upgrade in offers)
        {
            int level = gm.GetUpgradeLevel(upgrade);
            int cost = upgrade.GetCost(level);
            bool maxed = level >= upgrade.MaxLevel;

            var card = new VBoxContainer { CustomMinimumSize = new Vector2(180, 0) };
            card.AddChild(new TextureRect
            {
                Texture = upgrade.Icon,
                CustomMinimumSize = new Vector2(0, 96),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
            });
            card.AddChild(new Label { Text = upgrade.UpgradeName, HorizontalAlignment = HorizontalAlignment.Center });
            card.AddChild(new Label
            {
                Text = upgrade.Description,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                HorizontalAlignment = HorizontalAlignment.Center,
                SizeFlagsVertical = SizeFlags.ExpandFill
            });
            if (upgrade.MaxLevel > 1)
                card.AddChild(new Label { Text = $"Level {level}/{upgrade.MaxLevel}", HorizontalAlignment = HorizontalAlignment.Center });

            var buyButton = new Button
            {
                Text = maxed ? "Owned" : $"Buy ${cost}",
                Disabled = maxed || !gm.CanAfford(cost)
            };
            buyButton.Pressed += () =>
            {
                gm.BuyUpgrade(upgrade);
                BuildCards();
            };
            card.AddChild(buyButton);
            cards.AddChild(card);
        }
    }
}
