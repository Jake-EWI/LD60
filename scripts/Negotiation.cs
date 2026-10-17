using Godot;
using System;

public enum HaggleResult { Accepted, Countered, Left, Stole }
public class Negotiation
{
    public int AskingPrice {get; private set;}
    public float Patience {get; private set;}

    private readonly int limit;
    private readonly bool buying;
    private readonly CustomerDefinition customer;
    //todo: player stats
    private readonly RandomNumberGenerator rng;

    public Negotiation(CustomerDefinition customer, ItemDefinition item, bool buying, RandomNumberGenerator rng)
    {
        this.customer = customer;
        this.buying = buying;
        this.rng = rng;
        int value = item.BaseValue;
        if (buying)
        {
            limit = Mathf.RoundToInt(value * rng.RandfRange(1.1f, 1.4f));
            AskingPrice = Mathf.RoundToInt(value * (1 - customer.Greed));
        }
        else
        {
            limit = Mathf.RoundToInt(value * rng.RandfRange(0.6f, 0.9f));
            AskingPrice = Mathf.RoundToInt(value * (1 + customer.Greed));
        }
        Patience = customer.Patience;
    }

    public HaggleResult Offer(int offer)
    {
        bool meetsPrice = buying ? offer <= AskingPrice : offer >= AskingPrice;
        if(meetsPrice)
            return HaggleResult.Accepted;

        bool withinLimit = buying ? offer <= limit : offer >= limit;
        if (withinLimit && rng.Randf() < 0.5f)
        {
            AskingPrice = offer;
            return HaggleResult.Accepted;
        }

        float insult = Mathf.Abs(offer - AskingPrice) / (float)AskingPrice;
        Patience -= Mathf.RoundToInt(insult * 100);
        if (Patience <= 0)
        {
            float badRoll = rng.RandfRange(0, 1);

            if (badRoll <= customer.StealChance)
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
