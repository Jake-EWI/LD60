using Godot;
using System;
using LD60.Dialogue;

public enum ShopPhase { Intro, Barter, Closing }
public partial class ShopLoop : Node
{
    [Export] public CustomerDefinition[] CustomerPool {get; set;} = [];
    [Export] int MaxCustomers {get; set;} = 6;
    [Export] int MinCustomers {get; set;} = 4;
    [Export] public int Rent {get; set;} = 50;
    [Export] public int RunLength {get; set;} = 7;
    [Export] public DialogueRunner Runner { get; set; }
    [Export] public BarterPanel BarterPanel { get; set; }
    [Export] public CustomerImage CustomerImage { get; set; }

    private ShopPhase phase;
    private Negotiation negotiation;

    private GameState gm;
    private readonly System.Collections.Generic.Queue<CustomerDefinition> customerQueue = new();
    private CustomerDefinition customer;
    private RandomNumberGenerator rng;

    public override void _Ready()
    {
        gm = GetNode<GameState>("/root/GameState");
        gm.Reset();
        rng = new RandomNumberGenerator();
        rng.Randomize();
        Runner.DialogueEnded += OnDialogueEnded;
        Runner.CommandIssued += OnCommand;

        BarterPanel.OfferMade += OnOfferPressed;
        
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

        phase = ShopPhase.Intro;

        CustomerImage.ChangeCustomerTexture(customer.CharacterImage);

        negotiation = new Negotiation(customer, rng);
        Runner.SetVariable("price", negotiation.AskingPrice);
        Runner.SetVariable("item", customer.Item.ItemName);
        Runner.Start(customer.DialoguePath);
    }

    public void OnCommand(string name, string[] args)
    {
        switch (name)
        {
            case "barter":
                phase = ShopPhase.Barter;
                break;
        }
    }

    public void OnDialogueEnded()
    {
        switch(phase)
        {
            case ShopPhase.Barter:
                ShowBarter();
                break;
            case ShopPhase.Intro:
            case ShopPhase.Closing:
                NextCustomer();
                break;
        }
    }

    private void ShowBarter()
    {
        BarterPanel.Visible = true;
        BarterPanel.Open(negotiation.AskingPrice, gm.Money);
    }

    private void HideBarter()
    {
        BarterPanel.Visible = false;
    }

    private void OnOfferPressed(int offer)
    {
        HideBarter();

        var result = negotiation.Offer(offer);
        Runner.SetVariable("price", negotiation.AskingPrice);

        switch(result)
        {
            case HaggleResult.Accepted:
                phase = ShopPhase.Closing;
                Runner.Play("accepted");
                gm.AddMoney(-negotiation.AskingPrice);
                break;
            case HaggleResult.Countered:
                Runner.Play("countered");
                break;
            case HaggleResult.Left:
                phase = ShopPhase.Closing;
                Runner.Play("left");
                break;
            case HaggleResult.Stole:
                //todo: steal code
                phase = ShopPhase.Closing;
                Runner.Play("stole");
                break;
        }
    }

    public void HandleSeller()
    {

    }

    public void HandleBuyer()
    {

    }
}
