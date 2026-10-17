using Godot;
using System;

public partial class CoaPanel : Control
{
    [Signal] public delegate void GoBackPressedEventHandler();
    [Export] public float MisspellingChance {get; set;} = 0.3f;
    [Export] public float FakeDateChance {get; set;} = 0.3f;
    [Export] public float FakeIdChance {get; set;} = 0.3f;
    [Export] public float WrongItemChance {get; set;} = 0.3f;
    private const string Title = "Certificate of Authenticity";
    private Label coaTitle;
    private Label coaItem;
    private Label coaId;
    private Label coaDate;
    private Button goBackButton;
    private AudioStreamPlayer coaSound;

    public override void _Ready()
    {
        coaTitle = GetNode<Label>("CoaTexture/CoaTitle");
        coaItem = GetNode<Label>("CoaTexture/VBoxContainer/CoaItem");
        coaId = GetNode<Label>("CoaTexture/VBoxContainer/CoaId");
        coaDate = GetNode<Label>("CoaTexture/VBoxContainer/CoaDate");
        coaSound = GetNode<AudioStreamPlayer>("CoaSound");
        goBackButton = GetNode<Button>("GoBackButton");
        goBackButton.Pressed += () => EmitSignal(SignalName.GoBackPressed);
    }

    public void OpenCoa(string itemName, bool isFake, RandomNumberGenerator rng, GameState gm, ShopLoop shop)
    {
        var coa = GenerateCoa(itemName, isFake, rng, gm.CurrentDate, shop.ItemPool);
        coaTitle.Text = coa.Title;
        coaItem.Text = $"Item: {coa.ItemName}";
        coaId.Text = $"Manufacturer ID: {coa.Id}";
        coaDate.Text = $"Date of Authentification: {coa.Date:MM/dd/yyyy}";
        this.Visible = true;
        coaSound.Play();
    }

    public Coa GenerateCoa(string itemName, bool isFake, RandomNumberGenerator rng, DateTime today, ItemDefinition[] itemPool)
    {
        bool misspell = isFake && rng.Randf() < MisspellingChance;
        bool wrongItem = isFake && rng.Randf() < WrongItemChance;
        bool fakeId = isFake && rng.Randf() < FakeIdChance;
        bool fakeDate = isFake && rng.Randf() < FakeDateChance;
        if(isFake && !misspell && !wrongItem && !fakeId && !fakeDate)
            misspell = true;

        return new Coa
        {
            Title = misspell ? MisspellWordInSentence(Title, rng) : Title,
            ItemName = GetItemName(itemName, wrongItem, itemPool, rng),
            Id = GenerateRandomId(rng, fakeId),
            Date = GenerateDate(rng, fakeDate, today)
        };
    }

    private string MisspellWordInSentence(string sentence, RandomNumberGenerator rng)
    {
        var words = sentence.Split(" ");
        var wordIndex = rng.RandiRange(0, words.Length - 1);
        var charArray = words[wordIndex].ToCharArray();
        int charIndex;
        do
        {
            charIndex = rng.RandiRange(0, charArray.Length - 2);
        } while (charArray[charIndex] == charArray[charIndex + 1]);
        (charArray[charIndex], charArray[charIndex + 1]) = (charArray[charIndex + 1], charArray[charIndex]);
        words[wordIndex] = new string(charArray);
        return string.Join(" ", words);
    }  

    private string GenerateRandomId(RandomNumberGenerator rng, bool isFake)
    {
        int idLength = isFake ? 6 : rng.RandiRange(7, 8);
        string id = "";
        for(int i = 0; i < idLength; i++)
        {
            if(rng.RandiRange(0, 1) == 0)
                id += (char)rng.RandiRange(65, 90);
            else
                id += rng.RandiRange(0, 9).ToString();
        }
        return id;
    }

    private DateTime GenerateDate(RandomNumberGenerator rng, bool isFake, DateTime today)
    {
        var startingDate = today;
        if(isFake)
            startingDate = startingDate.AddYears(rng.RandiRange(1, 10));
        else
            startingDate = startingDate.AddYears(rng.RandiRange(-15, 0));

        return startingDate;
    }

    private string GetItemName(string itemName, bool isFake, ItemDefinition[] itemPool, RandomNumberGenerator rng)
    {
        if(isFake)
        {
            var otherItems = Array.FindAll(itemPool, i => i.ItemName != itemName);
            return otherItems[rng.RandiRange(0, otherItems.Length - 1)].ItemName;
        }
        else
            return itemName;
    }
}
