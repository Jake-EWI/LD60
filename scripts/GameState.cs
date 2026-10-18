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


    public void Reset()
    {
        Money = startingMoney;
        Day = 1;
        GoodKarma = 0;
        BadKarma = 0;
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
    

}
