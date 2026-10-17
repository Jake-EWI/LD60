using Godot;
using System;
using LD60.Dialogue;
using System.Collections.Generic;

public enum ShopPhase { Intro, Barter, Closing }
public partial class ShopLoop : Node
{
    [Export] public CustomerDefinition[] CustomerPool {get; set;} = [];
    private List<CustomerDefinition> liveCustomerPool { get; set; } = [];
    [Export] int MaxCustomers {get; set;} = 6;
    [Export] int MinCustomers {get; set;} = 4;
    [Export] public int Rent {get; set;} = 50;
    [Export] public int RunLength {get; set;} = 7;

    //ui elements
    [Export] public DialogueRunner Runner { get; set; }
    [Export] public BarterPanel BarterPanel { get; set; }
    [Export] public CustomerImage CustomerImage { get; set; }
    [Export] public OfferedItem OfferedItemImage { get; set;}
    [Export] public ActionsMenu ActionsMenu { get; set; }
    [Export] public InventoryPanel InventoryPanel { get; set; }
    [Export] public DayScreen DayScreen { get; set; }

    //gamestate

    private ShopPhase phase;
    private Negotiation negotiation;

    private GameState gm;
    private readonly System.Collections.Generic.Queue<CustomerDefinition> customerQueue = new();
    private CustomerDefinition customer;
    private bool customerBuying;
    [Export] public ItemDefinition[] ItemPool { get; set; } = [];
    private ItemInstance offeredItem;
    private int moneyAtStartOfDay;
    private RandomNumberGenerator rng;

    public override void _Ready()
    {
        gm = GetNode<GameState>("/root/GameState");
        gm.Reset();
        rng = new RandomNumberGenerator();
        rng.Randomize();
        Runner.Rng = rng;
        //signal binding
        Runner.DialogueEnded += OnDialogueEnded;
        Runner.CommandIssued += OnCommand;

        BarterPanel.OfferMade += OnOfferPressed;

        CustomerImage.Entered += () => Runner.Start(customer.DialoguePath, customerBuying ? "buyer" : "start");
        CustomerImage.Left += NextCustomer;

        ActionsMenu.RejectButtonPressed += OnRejectPressed;
        ActionsMenu.InventoryPressed += OnInventoryPressed;
        ActionsMenu.TalkButtonPressed += OnTalkPressed;

        DayScreen.DayScreenFinished += () => StartDay();
        
        StartDay();
    }

    public void StartDay()
    {
        customerQueue.Clear();
        liveCustomerPool = new List<CustomerDefinition>(CustomerPool);
        int customersPerDay = rng.RandiRange(MinCustomers, MaxCustomers);
        customersPerDay = Math.Min(customersPerDay, liveCustomerPool.Count);

        for (int i = 0; i < customersPerDay; i++)
        {
            int customerIndex = rng.RandiRange(0, liveCustomerPool.Count - 1);
            customerQueue.Enqueue(liveCustomerPool[customerIndex]);
            liveCustomerPool.RemoveAt(customerIndex);
        }
        
        moneyAtStartOfDay = gm.Money;
        NextCustomer();
        
    }

    public void EndDay()
    {
        int profitLoss = gm.Money - moneyAtStartOfDay;
        gm.AddMoney(-Rent);
        gm.Day++;
        DayScreen.ShowDayScreen(gm.Day, profitLoss, Rent, gm.Money);
    }

    public void NextCustomer()
    {
        if (!customerQueue.TryDequeue(out customer))
        {
            EndDay();
            return;
        }

        //determine buying or selling
        customerBuying = gm.Inventory.Count > 0 && rng.Randf() < customer.BuyerChance;

        if(customerBuying)
        {
            offeredItem = gm.Inventory[rng.RandiRange(0, gm.Inventory.Count - 1)];
        } else {
            var itemChoices = customer.PossibleItems.Length > 0 ? customer.PossibleItems : ItemPool;
            offeredItem = new ItemInstance(itemChoices[rng.RandiRange(0, itemChoices.Length - 1)])
            {
                IsFake = rng.Randf() < customer.FakeChance
            };
        }

        phase = ShopPhase.Intro;

        negotiation = new Negotiation(customer, offeredItem.Item, customerBuying, rng);
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
                CustomerImage.ShakeCharacter(float.Parse(args[0], System.Globalization.CultureInfo.InvariantCulture));
                break;
        }
    }

    public void OnDialogueEnded()
    {
        switch(phase)
        {
            case ShopPhase.Barter:
                ShowBarter();
                ActionsMenu.Visible = true;
                ActionsMenu.ShowRejectButton(true);
                break;
            case ShopPhase.Intro:
            case ShopPhase.Closing:
                CustomerImage.Leave();
                ActionsMenu.ShowRejectButton(false);
                ActionsMenu.Visible = false;
                break;
        }
    }

    private void ShowBarter()
    {
        BarterPanel.Visible = true;
        BarterPanel.Open(negotiation.AskingPrice, customerBuying ? offeredItem.Item.BaseValue * 3 : gm.Money);
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
                if (!customerBuying)
                {
                    gm.AddMoney(-negotiation.AskingPrice);
                    offeredItem.PricePaid = negotiation.AskingPrice;
                    gm.Inventory.Add(offeredItem);
                    Runner.Play("accepted");
                }
                else if (offeredItem.IsFake && rng.Randf() < customer.Savvy)
                {
                    Runner.Play("spotted_fake");
                }
                else
                {
                    gm.Inventory.Remove(offeredItem);
                    gm.AddMoney(negotiation.AskingPrice);
                    Runner.Play("sold");
                }
                break;
            case HaggleResult.Countered:
                Runner.Play(customerBuying ? "buyer_countered" : "countered");
                break;
            case HaggleResult.Left:
                phase = ShopPhase.Closing;
                Runner.Play(customerBuying ? "buyer_left" : "left");
                break;
            case HaggleResult.Stole:
                phase = ShopPhase.Closing;
                Runner.Play(customerBuying ? "buyer_stole" : "stole");
                HandleSteal();
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

    public void OnInventoryPressed()
    {
        if(InventoryPanel.Visible)
        {
            InventoryPanel.Visible = false;
            return;
        }
        InventoryPanel.Visible = true;
    }

    public void OnRejectPressed()
    {
        HideBarter();
        phase = ShopPhase.Closing;
        Runner.Play("pass");
        HideOfferedItem();
    }

    public void OnTalkPressed()
    {
        HideBarter();
        Runner.Play(customerBuying ? "buyer_chat" : "chat");
    }

    public void HandleSteal()
    {
        if (customerBuying)
            gm.Inventory.Remove(offeredItem);
        else if (gm.Inventory.Count > 0)
            gm.Inventory.RemoveAt(rng.RandiRange(0, gm.Inventory.Count - 1));
    }
}
