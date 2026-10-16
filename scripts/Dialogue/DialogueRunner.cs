using System.Collections.Generic;
using System.Linq;
using Godot;

namespace LD60.Dialogue;

public partial class DialogueRunner : Node
{
    [Signal] public delegate void DialogueStartedEventHandler();
    [Signal] public delegate void DialogueEndedEventHandler();
    [Signal] public delegate void CommandIssuedEventHandler(string name, string[] args);
    [Export] public DialogueBox Box { get; set; }
    [Export(PropertyHint.File, "*.txt")] public string ScriptPath { get; set; } = "";
    [Export] public bool AutoStart { get; set; }

    public bool IsRunning => _steps != null;

    private Dictionary<string, List<DialogueStep>> _sections;
    private List<DialogueStep> _steps;
    private int _index;
    private ChoiceStep _choice;

    public override void _Ready()
    {
        Box.Advanced += Continue;
        Box.LineFinished += OnLineFinished;
        Box.ChoiceSelected += OnChoiceSelected;

        if (AutoStart && ScriptPath != "")
            Start(ScriptPath);
    }

    public void Start(string path, string section = DialogueParser.StartSection)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PushError("file not found?");
            return;
        }

        _sections = DialogueParser.Parse(file.GetAsText(), path);

        // Catch typo'd voices up front instead of when the line is reached.
        foreach (var step in _sections.Values.SelectMany(s => s).OfType<ChangeVoice>())
        {
            if (!IsDefaultVoice(step.Path) && ResolveVoice(step.Path) == null)
                GD.PushError($"{path}: voice '{step.Path}' not found");
        }

        Box.SetVoice(null);
        EmitSignal(SignalName.DialogueStarted);
        JumpTo(section);
    }

    public void Stop()
    {
        if (_steps == null)
            return;
        _steps = null;
        _choice = null;
        Box.Close();
        EmitSignal(SignalName.DialogueEnded);
    }

    private void JumpTo(string section)
    {
        if (section == DialogueParser.EndTarget)
        {
            Stop();
            return;
        }
        if (!_sections.TryGetValue(section, out var steps))
        {
            GD.PushError("couldn't find section");
            Stop();
            return;
        }

        _steps = steps;
        _index = 0;
        Continue();
    }

    private void Continue()
    {
        var steps = _steps;
        while (steps != null && _index < steps.Count)
        {
            switch (steps[_index++])
            {
                case LineStep line:
                    _choice = _index < steps.Count ? steps[_index] as ChoiceStep : null;
                    if (_choice != null)
                        _index++;
                    Box.ShowLine(line.Speaker, line.Text);
                    return;

                case ChoiceStep choice:
                    _choice = choice;
                    ShowChoices();
                    return;

                case JumpStep jump:
                    JumpTo(jump.Target);
                    return;

                case ChangeVoice voice:
                    if (IsDefaultVoice(voice.Path))
                        Box.SetVoice(null);
                    else if (ResolveVoice(voice.Path) is { } path)
                        Box.SetVoice(GD.Load<AudioStream>(path));
                    break;

                case CommandStep command:
                    EmitSignal(SignalName.CommandIssued, command.Name, command.Args);
                    if (_steps != steps)
                        return;
                    break;
            }
        }

        Stop();
    }

    private void ShowChoices() => Box.ShowChoices(_choice.Choices.Select(c => c.Text).ToArray());

    private const string VoiceFolder = "res://assets/audio/sfx/";
    private static readonly string[] VoiceExtensions = { "", ".wav", ".ogg", ".mp3" };

    private static bool IsDefaultVoice(string voice) => voice is "" or "default";

    private static string ResolveVoice(string voice)
    {
        var basePath = voice.StartsWith("res://") ? voice : VoiceFolder + voice;
        return VoiceExtensions.Select(ext => basePath + ext).FirstOrDefault(p => ResourceLoader.Exists(p));
    }

    private void OnLineFinished()
    {
        if (_choice != null)
            ShowChoices();
    }

    private void OnChoiceSelected(int index)
    {
        var picked = _choice.Choices[index];
        _choice = null;
        if (picked.Target == null)
            Continue();
        else
            JumpTo(picked.Target);
    }
}
