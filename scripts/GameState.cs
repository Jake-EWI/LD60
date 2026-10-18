using Godot;
using System;
using System.Collections.Generic;

public enum MoneyResponse { Success, OutOfMoney}

   

public partial class GameState : Node
{
    public int Money {get; set;}
    private int startingMoney { get; set; } = 150;
    public int Day {get; set;} = 1;
    public DateTime CurrentDate {get; set;} = new DateTime(2052, 10, 15);
    public List<ItemInstance> Inventory = new List<ItemInstance>();
    public int GoodKarma { get; set; } = 0;
    public int BadKarma { get; set; } = 0;
    public Dictionary<UpgradeDefinition, int> Upgrades = new();


    public void Reset()
    {
        Money = startingMoney;
        Day = 1;
        GoodKarma = 0;
        BadKarma = 0;
        Upgrades.Clear();
        EmitSignal(SignalName.MoneyChanged, Money);
    }

    [Signal]
    public delegate void MoneyChangedEventHandler(int money);
    public bool CanAfford(int amount) => Money >= amount;
    public void AddMoney(int amount)
    {
        Money += amount;
        EmitSignal(SignalName.MoneyChanged, Money);
    }

    public void IncrementDay()
    {
        Day++;
        CurrentDate = CurrentDate.AddDays(1);
    }

    public int GetKarma()
    {
        return GoodKarma - BadKarma;
    }

    public int GetUpgradeLevel(UpgradeDefinition upgrade) => Upgrades.GetValueOrDefault(upgrade);

    public bool HasUpgrade(UpgradeType type)
    {
        foreach (var upgrade in Upgrades.Keys)
            if (upgrade.Type == type)
                return true;
        return false;
    }

    public float GetBonus(UpgradeType type)
    {
        float bonus = 0;
        foreach (var (upgrade, level) in Upgrades)
            if (upgrade.Type == type)
                bonus += upgrade.Amount * level;
        return bonus;
    }

    public void BuyUpgrade(UpgradeDefinition upgrade)
    {
        int level = GetUpgradeLevel(upgrade);
        AddMoney(-upgrade.GetCost(level));
        Upgrades[upgrade] = level + 1;
    }
    

}
