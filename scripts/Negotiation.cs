using Godot;
using System;

public enum HaggleResult { Accepted, Countered, Left, Stole }
public class Negotiation
{
    public int AskingPrice {get; private set;}
    public float Patience {get; private set;}

    private readonly int limit;
    private readonly CustomerDefinition customer;
    //todo: player stats
    private readonly RandomNumberGenerator rng;

    public Negotiation(CustomerDefinition customer, RandomNumberGenerator rng)
    {
        this.customer = customer;
        this.rng = rng;
        int value = customer.Item.BaseValue;
        limit = Mathf.RoundToInt(value * rng.RandfRange(0.6f, 0.9f));
        AskingPrice = Mathf.RoundToInt(value * (1 + customer.Greed));
        Patience = customer.Patience;
    }

    public HaggleResult Offer(int offer)
    {
        if(offer >= AskingPrice)
            return HaggleResult.Accepted;

        if (offer >= limit && rng.Randf() < 0.5f)
        {
            AskingPrice = offer;
            return HaggleResult.Accepted;
        }

        float insult = (AskingPrice - offer) / (float)AskingPrice;
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
        
        AskingPrice = Mathf.RoundToInt(MathF.Max(limit, Mathf.RoundToInt(Mathf.Lerp(AskingPrice, offer, 0.4f))));
        return HaggleResult.Countered;
    }
}
