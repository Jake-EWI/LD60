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
        offerButton.Pressed += () => EmitSignal(SignalName.OfferMade, (int)offerInput.Value);
    }

    public void Open(int askingPrice, int money, bool selling)
    {
        offerInput.MaxValue = money;
        offerInput.Value = askingPrice;
        offerButton.Text = selling ? "Offer to Sell" : "Offer to Buy";
    }

}
