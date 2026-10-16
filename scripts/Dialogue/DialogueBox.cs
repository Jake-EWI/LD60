using Godot;

namespace LD60.Dialogue;

public partial class DialogueBox : Control
{
    [Signal] public delegate void LineFinishedEventHandler();
    [Signal] public delegate void AdvancedEventHandler();
    [Signal] public delegate void ChoiceSelectedEventHandler(int index);
    [Export] public float CharactersPerSecond { get; set; } = 40f;
    [Export] public float PunctuationPause { get; set; } = 0.2f;
    [Export(PropertyHint.Range, "1,10")] public int BlipEvery { get; set; } = 2;
    [Export] public Vector2 BlipPitchRange { get; set; } = new(0.82f, 1.28f);

    private enum State { Idle, Typing, Waiting, Choosing }

    private Label _name;
    private RichTextLabel _text;
    private Control _continueIndicator;
    private VBoxContainer _choices;
    public AudioStreamPlayer blip;
    private AudioStream _defaultVoice;

    private State _state = State.Idle;
    private string _parsed = "";
    private double _delay;
    private int _blipCounter;
    private double _time;

    public bool IsTyping => _state == State.Typing;

    public override void _Ready()
    {
        _name = GetNode<Label>("%Name");
        _text = GetNode<RichTextLabel>("%Text");
        _continueIndicator = GetNode<Control>("%Continue");
        _choices = GetNode<VBoxContainer>("%Choices");
        blip = GetNode<AudioStreamPlayer>("%Blip");
        _defaultVoice = blip.Stream;
        Close();
    }

    public void SetVoice(AudioStream voice)
    {
        blip.Stop();
        blip.Stream = voice ?? _defaultVoice;
    }

    public void ShowLine(string speaker, string text)
    {
        ClearChoices();
        _name.Text = speaker;
        _name.Visible = speaker != "";
        _text.Text = text;
        _text.VisibleCharacters = 0;
        _parsed = _text.GetParsedText();
        _delay = 0;
        _blipCounter = 0;
        _continueIndicator.Visible = false;
        Show();

        _state = State.Typing;
        if (_parsed.Length == 0)
            FinishLine();
    }

    public void ShowChoices(string[] options)
    {
        ClearChoices();
        _continueIndicator.Visible = false;
        Show();

        for (int i = 0; i < options.Length; i++)
        {
            int index = i;
            var button = new Button { Text = options[i] };
            button.Pressed += () => Choose(index);
            _choices.AddChild(button);
        }

        _choices.Show();
        _state = State.Choosing;
        if (options.Length > 0)
            _choices.GetChild<Button>(0).GrabFocus();
    }

    public void FinishLine()
    {
        if (_state != State.Typing)
            return;
        _text.VisibleCharacters = -1;
        _state = State.Waiting;
        _continueIndicator.Visible = true;
        EmitSignal(SignalName.LineFinished);
    }

    public void Close()
    {
        ClearChoices();
        _state = State.Idle;
        Hide();
    }

    public override void _Process(double delta)
    {
        _time += delta;
        if (_state == State.Waiting)
        {
            var color = _continueIndicator.Modulate;
            color.A = 0.5f + 0.5f * Mathf.Sin((float)_time * 6f);
            _continueIndicator.Modulate = color;
        }

        if (_state != State.Typing)
            return;

        _delay -= delta;
        bool blipped = false;
        while (_delay <= 0 && _state == State.Typing)
        {
            int index = _text.VisibleCharacters;
            char c = _parsed[index];
            _text.VisibleCharacters = index + 1;
            _delay += 1.0 / CharactersPerSecond;

            bool endOfSentence = c is '.' or '!' or '?' &&
                index + 1 < _parsed.Length && char.IsWhiteSpace(_parsed[index + 1]);
            if (endOfSentence)
                _delay += PunctuationPause;

            bool startsWord = !char.IsWhiteSpace(c) && (index == 0 || char.IsWhiteSpace(_parsed[index - 1]));
            if (startsWord && !blipped)
            {
                PlayBlip();
                blipped = true;
            }

            if (index + 1 >= _parsed.Length)
                FinishLine();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_state is not (State.Typing or State.Waiting))
            return;

        bool advance = @event.IsActionPressed("ui_accept")
            || @event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true };
        if (!advance)
            return;

        GetViewport().SetInputAsHandled();
        if (_state == State.Typing)
        {
            FinishLine();
        }
        else
        {
            _state = State.Idle;
            _continueIndicator.Visible = false;
            EmitSignal(SignalName.Advanced);
        }
    }

    private void Choose(int index)
    {
        if (_state != State.Choosing)
            return;
        ClearChoices();
        _state = State.Idle;
        EmitSignal(SignalName.ChoiceSelected, index);
    }

    private void ClearChoices()
    {
        foreach (var child in _choices.GetChildren())
        {
            _choices.RemoveChild(child);
            child.QueueFree();
        }
        _choices.Hide();
    }

    private void PlayBlip()
    {
        if (blip.Stream == null)
            return;
        if  (blip.Playing)
            blip.Stop();
        blip.PitchScale = (float)GD.RandRange(BlipPitchRange.X, BlipPitchRange.Y);
        blip.Play();
    }
}
