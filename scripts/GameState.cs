using Godot;
using System;
using System.Collections.Generic;

public enum MoneyResponse { Success, OutOfMoney}

   

public partial class GameState : Node
{
    public int Money {get; set;} = 100;
    public int Day {get; set;} = 1;
    public List<ItemInstance> Inventory = new List<ItemInstance>();


    public void Reset()
    {
        Money = 0;
        Day = 1;
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
    

}
