using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

namespace LD60.Dialogue;

public abstract record DialogueStep;
public sealed record LineStep(string Speaker, string[] Texts) : DialogueStep;
public sealed record ChoiceStep(List<DialogueChoice> Choices) : DialogueStep;
public sealed record JumpStep(string Target) : DialogueStep;
public sealed record CommandStep(string Name, string[] Args) : DialogueStep;
public sealed record ChangeVoice(string Path) : DialogueStep;
public sealed record DialogueChoice(string Text, string Target, string Condition);

public static partial class DialogueParser
{
    public const string StartSection = "start";
    public const string EndTarget = "end";

    [GeneratedRegex(@"^(?<speaker>[^:\[\]]{1,32}):\s+(?<text>.+)$")]
    private static partial Regex SpeakerLine();

    public static Dictionary<string, List<DialogueStep>> Parse(string source, string fileName = "<dialogue>")
    {
        var sections = new Dictionary<string, List<DialogueStep>> { [StartSection] = new() };
        var declared = new HashSet<string>();
        var targets = new List<(string Target, int Line)>();
        var current = sections[StartSection];

        var lines = source.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith("//"))
                continue;

            if (line.StartsWith("=="))
            {
                var name = line[2..].Trim();
                if (!sections.TryGetValue(name, out current))
                    sections[name] = current = new();
            }
            else if (line.StartsWith('*'))
            {
                var (text, target) = SplitArrow(line[1..]);
                if (target != null)
                    targets.Add((target, i));

                string condition = null;
                if (text.StartsWith('?'))
                {
                    var parts = text[1..].Split(' ', 2, StringSplitOptions.TrimEntries);
                    condition = parts[0];
                    text = parts[1];
                }

                if (current.Count > 0 && current[^1] is ChoiceStep menu)
                    menu.Choices.Add(new DialogueChoice(text, target, condition));
                else
                    current.Add(new ChoiceStep(new() { new DialogueChoice(text, target, condition) }));
            }
            else if (line.StartsWith("->"))
            {
                var target = line[2..].Trim();
                targets.Add((target, i));
                current.Add(new JumpStep(target));
            }
            else if (line.StartsWith('>'))
            {
                var parts = line[1..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                current.Add(new CommandStep(parts[0], parts[1..]));
            }
            else if (line.StartsWith(';'))
            {
                var voice = line[1..].Trim();
                current.Add(new ChangeVoice(voice));
            }
            else
            {
                var match = SpeakerLine().Match(line);
                current.Add(match.Success ? new LineStep(match.Groups["speaker"].Value.Trim(), SplitAlternatives(match.Groups["text"].Value)) : new LineStep("", SplitAlternatives(line)));
            }
        }

        foreach (var (target, line) in targets)
        {
            if (target != EndTarget && !sections.ContainsKey(target))
                throw Error(fileName, line, $"Unknown section '{target}'");
        }

        return sections;
    }

    private static string[] SplitAlternatives(string s) =>
        Regex.Replace(s, @"\s*\\n\s*", "\n").Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    private static (string Text, string Target) SplitArrow(string s)
    {
        int arrow = s.LastIndexOf("->", StringComparison.Ordinal);
        if (arrow < 0)
            return (s.Trim(), null);
        var target = s[(arrow + 2)..].Trim();
        return (s[..arrow].Trim(), target.Length > 0 ? target : null);
    }

    private static FormatException Error(string fileName, int lineIndex, string message) =>
        new($"{fileName}:{lineIndex + 1}: {message}");
}
