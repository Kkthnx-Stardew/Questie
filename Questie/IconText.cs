using System.Text;
using System.Text.RegularExpressions;

namespace Questie
{
    public enum IconKind
    {
        Npc,
        Item,
    }

    /// <summary>Where one picture goes in a block of laid out text.</summary>
    /// <param name="Line">The line of the block, counting from zero.</param>
    /// <param name="X">How far in from the left edge of the text, in pixels.</param>
    /// <param name="Kind">Whether it is a villager's portrait or an item's picture.</param>
    /// <param name="Id">The villager's internal name or the item's qualified id.</param>
    public readonly record struct IconSpot(int Line, float X, IconKind Kind, string Id);

    /// <summary>Text with room left for the pictures, and where the pictures go.</summary>
    public sealed record IconLayout(string Text, IReadOnlyList<IconSpot> Spots, int LineCount);

    /// <summary>
    /// Names in a note can carry a picture, the villager's portrait or the item's icon, in front of them. A name is written into the
    /// text between private marker characters, so the rules that build the text need no idea of pictures. This file has no game
    /// dependencies so the tests can compile it.
    /// </summary>
    public static class IconText
    {
        private const char Start = '';
        private const char Split = '';
        private const char End = '';
        private const char Joined = ''; // stands for a space inside a name while a line is split into words

        private static readonly Regex Marked = new($"{Start}([ni])([^{Split}]*){Split}([^{End}]*){End}", RegexOptions.Compiled);

        /// <summary>A name that carries a picture. Left as it is, it reads as just the name once <see cref="Strip"/> has run.</summary>
        public static string Mark(IconKind kind, string id, string display) => $"{Start}{(kind == IconKind.Npc ? 'n' : 'i')}{id}{Split}{display}{End}";

        /// <summary>The text with every marker taken out and the plain names left.</summary>
        public static string Strip(string text) => text.IndexOf(Start) < 0 ? text : Marked.Replace(text, m => m.Groups[3].Value);

        /// <summary>
        /// Wraps the text to a width and leaves room in front of each marked name for its picture, so no line is longer than the width
        /// and a name stays whole on one line. Lines the text already breaks into stay as they are, and a line that starts with a dash
        /// and a space is a bullet whose wrapped lines are indented to sit under its text.
        /// </summary>
        /// <param name="text">The text, with names marked.</param>
        /// <param name="width">The widest a line may be, in pixels.</param>
        /// <param name="measure">How wide a string is when drawn, in pixels.</param>
        /// <param name="iconWidth">The room to leave for a picture, in pixels.</param>
        public static IconLayout Layout(string text, float width, Func<string, float> measure, float iconWidth)
        {
            var spaceWidth = Math.Max(1f, measure(" "));
            var gap = new string(' ', Math.Max(1, (int)Math.Ceiling(iconWidth / spaceWidth)));

            var lines = new List<string>();
            var spots = new List<IconSpot>();

            foreach (var paragraph in text.Replace("\r", "").Split('\n'))
            {
                var line = new StringBuilder();
                var pending = new List<IconSpot>(); // spots of the line being built, with the line number filled in when it is closed

                // a bullet's lines after the first start under its text and not under its dash, so the dashes read as a list
                var hang = paragraph.StartsWith("- ", StringComparison.Ordinal) ? new string(' ', Math.Max(1, (int)Math.Ceiling(measure("- ") / spaceWidth))) : "";
                var hasWord = false;
                foreach (var word in Words(paragraph))
                {
                    // a word is whatever sits between spaces, so a name keeps the comma or full stop that follows it
                    var spotsInWord = new List<(int Prefix, IconKind Kind, string Id)>();
                    var unit = Expand(word, gap, spotsInWord);
                    var candidate = hasWord ? line + " " + unit : unit;
                    if (hasWord && measure(candidate) > width)
                    {
                        Close(lines, spots, line, pending);
                        candidate = hang + unit;
                    }
                    hasWord = true;
                    var start = candidate.Length - unit.Length;
                    foreach (var (prefix, kind, id) in spotsInWord)
                        pending.Add(new IconSpot(0, measure(candidate.Substring(0, start + prefix)), kind, id));
                    line.Clear().Append(candidate);
                }
                Close(lines, spots, line, pending);
            }
            return new IconLayout(string.Join("\n", lines), spots, lines.Count);
        }

        private static void Close(List<string> lines, List<IconSpot> spots, StringBuilder line, List<IconSpot> pending)
        {
            foreach (var spot in pending)
                spots.Add(spot with { Line = lines.Count });
            pending.Clear();
            lines.Add(line.ToString());
            line.Clear();
        }

        /// <summary>The words of a line. The spaces inside a marked name are kept out of the split so the name stays in one word.</summary>
        private static IEnumerable<string> Words(string paragraph) =>
            Marked.Replace(paragraph, m => m.Value.Replace(' ', Joined)).Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(w => w.Replace(Joined, ' '));

        /// <summary>One word with each marked name written out as room for its picture and then the name, and where each picture starts.</summary>
        private static string Expand(string word, string gap, List<(int Prefix, IconKind Kind, string Id)> spots)
        {
            var text = new StringBuilder();
            var at = 0;
            foreach (Match m in Marked.Matches(word))
            {
                text.Append(word, at, m.Index - at);
                spots.Add((text.Length, m.Groups[1].Value == "n" ? IconKind.Npc : IconKind.Item, m.Groups[2].Value));
                text.Append(gap).Append(m.Groups[3].Value);
                at = m.Index + m.Length;
            }
            return text.Append(word, at, word.Length - at).ToString();
        }
    }
}
