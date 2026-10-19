using Godot;
using System;

public partial class BarterPanel : Control
{
    [Signal] public delegate void OfferMadeEventHandler(int offer);

    private SpinBox offerInput; 
    private Button offerButton;

    public override void _Ready()
    {
        offerInput = GetNode<SpinBox>("Panel/OfferInput");
        offerButton = GetNode<Button>("Panel/OfferButton");
        offerButton.Pressed += () =>
        {
            offerInput.Apply();
            EmitSignal(SignalName.OfferMade, (int)offerInput.Value);
        };
    }

    public void Open(int askingPrice, int money, bool selling)
    {
        offerInput.MaxValue = selling ? money : Mathf.Min(money, askingPrice);
        offerInput.MinValue = selling ? askingPrice : 0;
        offerInput.Value = askingPrice;
        offerButton.Text = selling ? "Offer to Sell" : "Offer to Buy";
    }

}
