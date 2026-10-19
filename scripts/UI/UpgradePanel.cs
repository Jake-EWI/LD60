using Godot;
using System;
using System.Collections.Generic;

public partial class UpgradePanel : Control
{
    [Signal] public delegate void FinishedEventHandler();
    [Export] public int MaxOffers { get; set; } = 3;
    private HBoxContainer cards;
    private Button continueButton;
    private Label dateLabel;
    private GameState gm;
    private List<UpgradeDefinition> offers = [];
    private bool purchasedFreeUpgradeThisDay;

    public override void _Ready()
    {
        gm = GetNode<GameState>("/root/GameState");
        cards = GetNode<HBoxContainer>("Cards");
        continueButton = GetNode<Button>("ContinueButton");
        dateLabel = GetNode<Label>("DateLabel");
        continueButton.Pressed += () =>
        {
            Hide();
            EmitSignal(SignalName.Finished);
        };
        purchasedFreeUpgradeThisDay = false;
    }

    public void Open(UpgradeDefinition[] pool, RandomNumberGenerator rng)
    {
        offers.Clear();
        purchasedFreeUpgradeThisDay = false;
        var date = gm.CurrentDate;
        string suffix = date.Day is 11 or 12 or 13 ? "th" : (date.Day % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" };
        dateLabel.Text = $"{date:MMMM} {date.Day}{suffix}, {date.Year}";
        var alwaysPool = new List<UpgradeDefinition>();
        var randomPool = new List<UpgradeDefinition>();
        foreach (var upgrade in pool)
        {
            if (gm.GetUpgradeLevel(upgrade) >= upgrade.MaxLevel)
                continue;
            if (upgrade.AlwaysOffered)
                alwaysPool.Add(upgrade);
            else
                randomPool.Add(upgrade);
        }
        PickOffers(alwaysPool, rng);
        PickOffers(randomPool, rng);
        BuildCards();
        Show();
    }

    private void PickOffers(List<UpgradeDefinition> source, RandomNumberGenerator rng)
    {
        while (offers.Count < MaxOffers && source.Count > 0)
        {
            int index = rng.RandiRange(0, source.Count - 1);
            offers.Add(source[index]);
            source.RemoveAt(index);
        }
    }

    private void BuildCards()
    {
        foreach (var child in cards.GetChildren())
            child.QueueFree();

        foreach (var upgrade in offers)
        {
            if (upgrade.IsFree && purchasedFreeUpgradeThisDay)
                continue;

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
                card.AddChild(new Label { Text = $"Current Level {level}", HorizontalAlignment = HorizontalAlignment.Center });

            var buyButton = new Button
            {
                Text = maxed ? "Owned" : $"Buy ${cost}",
                Disabled = maxed || !gm.CanAfford(cost)
            };
            buyButton.Pressed += () =>
            {
                gm.BuyUpgrade(upgrade);
                if (upgrade.IsFree)
                {
                    purchasedFreeUpgradeThisDay = true;
                }
                BuildCards();
            };
            card.AddChild(buyButton);
            cards.AddChild(card);
        }
    }
}
