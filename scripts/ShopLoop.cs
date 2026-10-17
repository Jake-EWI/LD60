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

    //ui elements
    [Export] public DialogueRunner Runner { get; set; }
    [Export] public BarterPanel BarterPanel { get; set; }
    [Export] public CustomerImage CustomerImage { get; set; }
    [Export] public OfferedItem OfferedItemImage { get; set;}

    //gamestate

    private ShopPhase phase;
    private Negotiation negotiation;

    private GameState gm;
    private readonly System.Collections.Generic.Queue<CustomerDefinition> customerQueue = new();
    private CustomerDefinition customer;
    [Export] public ItemDefinition[] ItemPool { get; set; } = [];
    private ItemInstance offeredItem;
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

        CustomerImage.Entered += () => Runner.Start(customer.DialoguePath);
        CustomerImage.Left += NextCustomer;
        
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

        var itemChoices = customer.PossibleItems.Length > 0 ? customer.PossibleItems : ItemPool;
        offeredItem = new ItemInstance(itemChoices[rng.RandiRange(0, itemChoices.Length - 1)])
        {
            IsFake = rng.Randf() < customer.FakeChance
        };

        negotiation = new Negotiation(customer, offeredItem.Item, rng);
        Runner.SetVariable("price", negotiation.AskingPrice);
        Runner.SetVariable("item", offeredItem.Item.ItemName);

        CustomerImage.Enter(customer.CharacterImage);
    }

    public void OnCommand(string name, string[] args)
    {
        switch (name)
        {
            case "barter":
                phase = ShopPhase.Barter;
                break;
            case "show_item":
                ShowOfferedItem();
                break;
            case "shake":
                CustomerImage.ShakeCharacter(float.Parse(args[0]));
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
                CustomerImage.Leave();
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

    private void ShowOfferedItem()
    {
        OfferedItemImage.DisplayItem(offeredItem.Item.Icon, offeredItem.Item.ItemName);
    }

    private void HideOfferedItem()
    {
        OfferedItemImage.Visible = false;
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
                offeredItem.PricePaid = negotiation.AskingPrice;
                gm.Inventory.Add(offeredItem);
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

        switch(result)
        {
            case HaggleResult.Accepted:
            case HaggleResult.Left:
            case HaggleResult.Stole:
            HideOfferedItem();
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
