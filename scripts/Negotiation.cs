using Godot;
using System;

public enum HaggleResult { Accepted, Countered, Left, Stole }
public class Negotiation
{
    public int AskingPrice {get; set;}
    public float Patience {get; private set;}

    private readonly int limit;
    private readonly bool buying;
    private readonly CustomerDefinition customer;
    private readonly float stealProtection;
    private readonly RandomNumberGenerator rng;

    public Negotiation(CustomerDefinition customer, ItemDefinition item, bool buying, RandomNumberGenerator rng, GameState gm)
    {
        this.customer = customer;
        this.buying = buying;
        this.rng = rng;
        int value = item.BaseValue;
        float haggleRoom = gm.GetBonus(UpgradeType.HaggleRoom);
        if (buying)
        {
            limit = Mathf.RoundToInt(value * rng.RandfRange(1.25f, 1.6f) * (1 + haggleRoom));
            AskingPrice = Mathf.RoundToInt(value * (1 - customer.Greed * 0.5f));
        }
        else
        {
            limit = Mathf.RoundToInt(value * rng.RandfRange(0.5f, 0.8f) * (1 - haggleRoom));
            AskingPrice = Mathf.RoundToInt(value * (1 + customer.Greed));
        }
        Patience = customer.Patience + gm.GetBonus(UpgradeType.Patience);
        stealProtection = gm.GetBonus(UpgradeType.StealProtection);
    }

    public HaggleResult Offer(int offer)
    {
        bool meetsPrice = buying ? offer <= AskingPrice : offer >= AskingPrice;
        if(meetsPrice)
            return HaggleResult.Accepted;

        bool withinLimit = buying ? offer <= limit : offer >= limit;
        if (withinLimit && rng.Randf() < Mathf.InverseLerp(limit, AskingPrice, offer))
        {
            AskingPrice = offer;
            return HaggleResult.Accepted;
        }

        float insult = Mathf.Abs(offer - AskingPrice) / (float)Mathf.Max(offer, AskingPrice);
        Patience -= insult >= 0.6f ? Patience : Mathf.RoundToInt(insult * 50);
        if (Patience <= 0)
        {
            float badRoll = rng.RandfRange(0, 1);

            if (badRoll <= customer.StealChance - stealProtection)
            {
                return HaggleResult.Stole;
            } else
            {
                return HaggleResult.Left;
            }
        }
        int counter = Mathf.RoundToInt(Mathf.Lerp(AskingPrice, offer, 0.4f));
        AskingPrice = buying ? Mathf.Min(limit, counter) : Mathf.Max(limit, counter);
        return HaggleResult.Countered;
    }
}
