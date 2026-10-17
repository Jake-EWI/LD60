using Godot;
using System;
using LD60.Dialogue;

public partial class ShopLoop : Node
{
    [Export] public CustomerDefinition[] CustomerPool {get; set;} = [];
    [Export] int MaxCustomers {get; set;} = 6;
    [Export] int MinCustomers {get; set;} = 4;
    [Export] public int Rent {get; set;} = 50;
    [Export] public int RunLength {get; set;} = 7;
    [Export] public DialogueRunner Runner { get; set; }
    private Negotiation negotiation;

    private GameState gm;
    private readonly System.Collections.Generic.Queue<CustomerDefinition> customerQueue = new();
    private CustomerDefinition customer;
    private RandomNumberGenerator rng;

    public override void _Ready()
    {
        gm = GetNode<GameState>("/root/GameState");
        rng = new RandomNumberGenerator();
        rng.Randomize();
        Runner.DialogueEnded += () => CallDeferred(MethodName.NextCustomer);
        
        StartDay();
    }

    public void StartDay()
    {
        customerQueue.Clear();
        int customersPerDay = rng.RandiRange(MinCustomers, MaxCustomers);

        for (int i = 0; i < customersPerDay; i++)
            customerQueue.Enqueue(CustomerPool[rng.RandiRange(0, CustomerPool.Length - 1)]);
        
        NextCustomer();
        
    }

    public void EndDay()
    {
    
    }

    public void NextCustomer()
    {
        if (!customerQueue.TryDequeue(out customer))
        {
            EndDay();
            return;
        }

        negotiation = new Negotiation(customer, rng);
        Runner.SetVariable("price", negotiation.AskingPrice);
        Runner.SetVariable("item", customer.Item.ItemName);
        Runner.Start(customer.DialoguePath);
    }

    public void HandleSeller()
    {

    }

    public void HandleBuyer()
    {

    }
}
