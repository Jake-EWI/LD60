using Godot;
using System;
using LD60.Dialogue;
using System.Collections.Generic;

public enum ShopPhase { Intro, Barter, Closing, Shot }
public partial class ShopLoop : Node
{
    [Export] public CustomerDefinition[] CustomerPool {get; set;} = [];
    [Export] public CustomerDefinition PoliceOfficer { get; set; } = null;
    [Export] public CustomerDefinition Tutorial { get; set; } = null;
    private List<CustomerDefinition> liveCustomerPool { get; set; } = [];
    [Export] int MaxCustomers {get; set;} = 6;
    [Export] int MinCustomers {get; set;} = 4;
    [Export] public int Rent {get; set;} = 50;
    [Export] public int RunLength {get; set;} = 7;
    [Export] public int ExpertCost {get; set;} = 10;
    [Export] public int JailDays {get; set;} = 3;
    [Export(PropertyHint.File, "*.txt")] public string ExpertDialoguePath {get; set;} = "res://assets/dialogue/Expert.txt";

    //ui elements
    [Export] public DialogueRunner Runner { get; set; }
    [Export] public BarterPanel BarterPanel { get; set; }
    [Export] public CustomerImage CustomerImage { get; set; }
    [Export] public OfferedItem OfferedItemImage { get; set;}
    [Export] public ActionsMenu ActionsMenu { get; set; }
    [Export] public InventoryPanel InventoryPanel { get; set; }
    [Export] public DayScreen DayScreen { get; set; }
    [Export] public GunPOV GunPOV { get; set; }
    [Export] public ObservationPanel ObservationPanel { get; set; }
    [Export] public CoaPanel CoaPanel { get; set; }

    //gamestate

    private ShopPhase phase;
    private Negotiation negotiation;

    private GameState gm;
    private System.Collections.Generic.Queue<CustomerDefinition> customerQueue = new();
    private CustomerDefinition customer;
    private bool customerBuying;
    [Export] public ItemDefinition[] ItemPool { get; set; } = [];
    private ItemInstance offeredItem;
    private int moneyAtStartOfDay;
    private RandomNumberGenerator rng;
    private ItemDefinition itemStolenOnEntrance;
    private bool expertCalling;
    private bool guiltyOfMurder = false;
    private bool hasCoa = false;

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
        Runner.SectionEntered += OnSectionEntered;

        BarterPanel.OfferMade += OnOfferPressed;

        CustomerImage.Entered += () => Runner.Start(customer.DialoguePath, customerBuying ? "buyer" : "start");
        CustomerImage.Left += NextCustomer;

        ActionsMenu.RejectButtonPressed += OnRejectPressed;
        ActionsMenu.InventoryPressed += OnInventoryPressed;
        ActionsMenu.TalkButtonPressed += OnTalkPressed;
        ActionsMenu.PullGunPressed += OnPullGunPressed;
        ActionsMenu.ExpertButtonPressed += OnExpertPressed;
        ActionsMenu.ObservePressed += OnObservePressed;
        ActionsMenu.COAButtonPressed += OnCoaPressed;

        CoaPanel.GoBackPressed += CloseAllActionPanels;
        ObservationPanel.GoBackPressed += CloseAllActionPanels;
        InventoryPanel.GoBackPressed += CloseAllActionPanels;

        DayScreen.DayScreenFinished += () => StartDay();
        
        StartDay();
    }

    public void StartDay()
    {
        customerQueue.Clear();
        liveCustomerPool = new List<CustomerDefinition>(CustomerPool);
        int customersPerDay = rng.RandiRange(MinCustomers, MaxCustomers);
        customersPerDay = Math.Min(customersPerDay, liveCustomerPool.Count);

        if (gm.Day == 1)
            customerQueue.Enqueue(Tutorial);

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
        int fromDay = gm.Day;
        gm.IncrementDay();
        DayScreen.ShowDayScreen(fromDay, gm.Day, profitLoss, Rent, gm.Money);
    }

    public void NextCustomer()
    {
        if (!customerQueue.TryDequeue(out customer))
        {
            EndDay();
            return;
        }

        if (customer == PoliceOfficer)
        {
            itemStolenOnEntrance = null;
            customerBuying = false;
            phase = ShopPhase.Intro;
            CustomerImage.Visible = true;
            CustomerImage.Enter(customer.CharacterImage);
            return;
        }
        itemStolenOnEntrance = null;

        //determine if customer steals an item on entrance
        if (rng.Randf() < customer.StealChance && gm.Inventory.Count > 0)
        {
            itemStolenOnEntrance = gm.Inventory[rng.RandiRange(0, gm.Inventory.Count - 1)].Item;
            gm.Inventory.RemoveAll(i => i.Item == itemStolenOnEntrance);
        }

        //determine buying or selling
        customerBuying = gm.Inventory.Count > 0 && rng.Randf() < customer.BuyerChance;

        //determine if customer has a coa
        hasCoa = rng.Randf() < customer.CoaChance && !customerBuying;


        if(customerBuying)
        {
            //pick random item from inventory to sell
            offeredItem = gm.Inventory[rng.RandiRange(0, gm.Inventory.Count - 1)];
        } else {
            var itemChoices = customer.PossibleItems.Length > 0 ? customer.PossibleItems : ItemPool;
            offeredItem = new ItemInstance(itemChoices[rng.RandiRange(0, itemChoices.Length - 1)])
            {
                IsFake = rng.Randf() < customer.FakeChance
            };
            offeredItem.Imperfection = ObservationPanel.RollImperfection(offeredItem.IsFake, rng);
        }

        phase = ShopPhase.Intro;

        negotiation = new Negotiation(customer, offeredItem.Item, customerBuying, rng);
        Runner.SetVariable("price", negotiation.AskingPrice);
        Runner.SetVariable("item", offeredItem.Item.ItemName);

        CustomerImage.Visible = true;
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
            case "shoot":
                OnShootPressed();
                break;
            case "not_shoot":
                phase = ShopPhase.Closing;
                break;
            case "check_buy":
                if (!gm.CanAfford(ExpertCost))
                {
                    Runner.Play("broke");
                    break;
                }
                gm.AddMoney(-ExpertCost);
                Runner.Play("buy");
                break;
            case "check_if_fake":
                Runner.Play(offeredItem.IsFake ? "fake" : "real");
                break;
            case "check_if_guilty":
                if(guiltyOfMurder)
                {
                    Runner.Play("guilty");
                } else
                {
                    Runner.Play("innocent");
                }
                break;
            case "check_bribe":
                if(gm.CanAfford(10))
                {
                    Runner.Play("can_bribe");
                } else
                {
                    Runner.Play("cant_bribe");
                }
                break;
            case "bribe":
                if(gm.CanAfford(10))
                {
                    gm.AddMoney(-10);
                }
                break;
            case "arrest":
                customerQueue.Clear();
                phase = ShopPhase.Closing;
                break;
            case "tut_show_haggle":
                ShowBarter();
                BarterPanel.MouseBehaviorRecursive = Control.MouseBehaviorRecursiveEnum.Disabled;
                break;
            case "tut_hide_haggle":
                HideBarter();
                BarterPanel.MouseBehaviorRecursive = Control.MouseBehaviorRecursiveEnum.Inherited;
                break;
            case "tut_show_observe":
                TutorialShowButton("ObserveButton");
                break;
            case "tut_show_coa":
                TutorialShowButton("COAButton");
                break;
            case "tut_show_expert":
                TutorialShowButton("ExpertButton");
                break;
            case "tut_show_inventory":
                TutorialShowButton("InventoryButton");
                break;
            case "tut_show_gun":
                TutorialShowButton("PullGunButton");
                break;
            case "tut_hide_all":
                TutorialHideAll();
                break;
            case "tut_finish":
                TutorialHideAll();
                phase = ShopPhase.Closing;
                break;
            case "show_coa":
                HideBarter();
                CoaPanel.OpenCoa(offeredItem.Item.ItemName, offeredItem.IsFake, rng, gm, this);
                break;
        }
    }

    private async void OnSectionEntered(string section)
    {
        if (section != "not_shoot")
            return;
        Runner.Hold();
        await GunPOV.HideGun();
        Runner.Resume();
    }

    private void TutorialShowButton(string buttonName)
    {
        if (!ActionsMenu.Visible)
        {
            ActionsMenu.SetAllButtonsVisible(false);
            ActionsMenu.MouseBehaviorRecursive = Control.MouseBehaviorRecursiveEnum.Disabled;
            ActionsMenu.Visible = true;
        }
        ActionsMenu.ShowButton(buttonName);
    }

    private void TutorialHideAll()
    {
        HideBarter();
        BarterPanel.MouseBehaviorRecursive = Control.MouseBehaviorRecursiveEnum.Inherited;
        ActionsMenu.Visible = false;
        ActionsMenu.SetAllButtonsVisible(true);
        ActionsMenu.MouseBehaviorRecursive = Control.MouseBehaviorRecursiveEnum.Inherited;
    }

    public void OnDialogueEnded()
    {
        if (expertCalling)
        {
            expertCalling = false;
            Runner.Load(customer.DialoguePath);
        }

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
                HideOfferedItem();
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
        ActionsMenu.Visible = false;
        Runner.Play(customerBuying ? "buyer_chat" : "chat");
    }

    public void OnObservePressed()
    {
        HideBarter();
        // ActionsMenu.Visible = false;
        OfferedItemImage.Visible = false;
        //close if open
        if(ObservationPanel.Visible)
        {
            ObservationPanel.Visible = false;
            OfferedItemImage.Visible = true;
            ShowBarter();
            return;
        }

        ObservationPanel.OpenObservation(offeredItem.Item.Icon, offeredItem.Imperfection);
    }

    public void OnCoaPressed()
    {
        if(CoaPanel.Visible)
        {
            CoaPanel.Visible = false;
            ShowBarter();
            return;
        }
        HideBarter();
        if(hasCoa)
        {
            Runner.Play("coa");
        } else
        {
            Runner.Play("no_coa");
        }
    }

    public async void OnPullGunPressed()
    {
        HideBarter();
        ActionsMenu.Visible = false;
        await GunPOV.ShowGun();
        Runner.Play("pull_gun");
    }

    public void OnExpertPressed()
    {
        HideBarter();
        expertCalling = true;
        Runner.SetVariable("expertCost", ExpertCost);
        Runner.SetVariable("baseAmount", offeredItem.Item.BaseValue);
        Runner.Start(ExpertDialoguePath);
    }

    public async void OnShootPressed()
    {
        phase = ShopPhase.Shot;
        ActionsMenu.Visible = false;
        ActionsMenu.ShowRejectButton(false);
        await GunPOV.ShootGun(() =>
        {
            CustomerImage.CustomerText.Modulate = new Color(0, 0, 0, 0);
            HideOfferedItem();
        });
        if(itemStolenOnEntrance != null)
        {
            gm.Inventory.Add(new ItemInstance(itemStolenOnEntrance));
            itemStolenOnEntrance = null;
            guiltyOfMurder = false;
        } else
        {
            guiltyOfMurder = true;
        }
        Queue<CustomerDefinition> storeQueue = new Queue<CustomerDefinition>();
        storeQueue.Enqueue(PoliceOfficer);
        foreach (var c in customerQueue)
        {
            storeQueue.Enqueue(c);
        }
        customerQueue.Clear();
        customerQueue = storeQueue;
        //remove customer from pool after killing them
        CustomerPool = Array.FindAll(CustomerPool, c => c != customer);

        NextCustomer();
    }

    public void HandleSteal()
    {
        if (customerBuying)
            gm.Inventory.Remove(offeredItem);
        else if (gm.Inventory.Count > 0)
            gm.Inventory.RemoveAt(rng.RandiRange(0, gm.Inventory.Count - 1));
    }

    public void CloseAllActionPanels()
    {
        ObservationPanel.Visible = false;
        CoaPanel.Visible = false;
        InventoryPanel.Visible = false;
        ShowBarter();
        ShowOfferedItem();
    }
}
