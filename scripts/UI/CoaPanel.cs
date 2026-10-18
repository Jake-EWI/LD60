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

    public void OpenCoa(Coa coa)
    {
        coaTitle.Text = coa.Title;
        coaItem.Text = $"Item: {coa.ItemName}";
        coaId.Text = $"Manufacturer ID: {coa.Id}";
        coaDate.Text = $"Date of Authentification: {coa.Date:MM/dd/yyyy}";
        this.Visible = true;
        coaSound.Play();
    }

    public Coa GenerateCoa(ItemInstance item, RandomNumberGenerator rng, DateTime today, ItemDefinition[] itemPool)
    {
        bool isFake = item.IsFake;
        bool misspell = isFake && rng.Randf() < MisspellingChance;
        bool wrongItem = isFake && rng.Randf() < WrongItemChance;
        bool fakeId = isFake && rng.Randf() < FakeIdChance;
        bool fakeDate = isFake && rng.Randf() < FakeDateChance;
        if(isFake && !misspell && !wrongItem && !fakeId && !fakeDate)
            misspell = true;

        return new Coa
        {
            Title = misspell ? MisspellWordInSentence(Title, rng) : Title,
            ItemName = GetItemName(item.Item.ItemName, wrongItem, itemPool, rng),
            Id = fakeId ? GenerateFakeId(item, itemPool, rng) : item.Serial,
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

    private string GenerateFakeId(ItemInstance item, ItemDefinition[] itemPool, RandomNumberGenerator rng)
    {
        if(rng.Randf() < 0.5f)
            return item.Item.GenerateSerial(rng);
        var otherItems = Array.FindAll(itemPool, i => i.SerialPrefix != item.Item.SerialPrefix);
        return otherItems[rng.RandiRange(0, otherItems.Length - 1)].GenerateSerial(rng);
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
