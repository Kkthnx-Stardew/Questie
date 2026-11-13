using StardewValley;
using StardewValley.Quests;

namespace Questie
{
    /// <summary>
    /// Finds the items a plain story quest names only in its objective text, such as the five fish an expansion's elixir quest asks
    /// for. The names are the game's own, so items added by another mod are found as well and a translated game is read in its own
    /// language. A longer name is matched before a shorter one inside it, so "Small Manta Ray" is not read as "Manta Ray". Simple
    /// plurals are read too, so "void souls" finds the "Void Soul" it names.
    /// </summary>
    internal static class QuestTextItems
    {
        /// <summary>The most items named by one quest, so a quest that mentions many things does not fill the journal.</summary>
        private const int MaxItems = 8;

        /// <summary>Names shorter than this are too easy to read out of a sentence by accident.</summary>
        private const int MinNameLength = 3;

        private static List<(string Name, string Id)>? _names;
        private static readonly Dictionary<string, string[]> Cache = new(StringComparer.Ordinal);

        /// <summary>Forgets the names and what was found. Called when the item data or the language changes.</summary>
        public static void Reset()
        {
            _names = null;
            Cache.Clear();
        }

        /// <summary>Builds the names now, while a save loads, so the first journal opening does not stop for them.</summary>
        public static void WarmUp() => _ = Names();

        /// <summary>The qualified ids of the items the quest's objective text names, in the order they are named.</summary>
        public static IReadOnlyList<string> Find(Quest quest)
        {
            string text;
            try
            {
                text = string.Join("\n", quest.GetObjectiveDescriptions());
            }
            catch
            {
                // a quest from another mod whose text cannot be read simply gets no notes
                return Array.Empty<string>();
            }
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<string>();
            if (Cache.TryGetValue(text, out var cached))
                return cached;

            var found = Scan(text);
            Cache[text] = found;
            return found;
        }

        private static string[] Scan(string text)
        {
            var found = new List<string>();
            var work = text;
            foreach (var (name, id) in Names())
            {
                if (found.Count >= MaxItems)
                    break;
                if (found.Contains(id))
                    continue;
                var at = TextMatch.IndexOfName(work, name, out var length);
                if (at < 0)
                    continue;
                found.Add(id);
                // blank the match so a shorter name inside it cannot be found again
                work = work.Remove(at, length).Insert(at, new string(' ', length));
            }
            return found.ToArray();
        }

        /// <summary>The display names of the items the game knows, longest first, each with its qualified id.</summary>
        private static List<(string Name, string Id)> Names()
        {
            _names ??= BuildNames();
            return _names;
        }

        private static List<(string Name, string Id)> BuildNames()
        {
            var names = new List<(string, string)>();
            var seen = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
            void Add(string qualifiedId)
            {
                try
                {
                    var name = ItemRegistry.GetData(qualifiedId)?.DisplayName?.Trim();
                    // a name the game cannot translate comes back as a marker, which is no name to look for
                    if (string.IsNullOrWhiteSpace(name) || name.Length < MinNameLength || name.StartsWith("(no translation:", StringComparison.Ordinal))
                        return;
                    if (seen.Add(name))
                        names.Add((name, qualifiedId));
                }
                catch
                {
                    // one bad item from another mod must not stop the rest from being read
                }
            }

            foreach (var id in Game1.objectData.Keys)
                Add("(O)" + id);
            foreach (var id in Game1.bigCraftableData.Keys)
                Add("(BC)" + id);

            names.Sort((a, b) => b.Item1.Length.CompareTo(a.Item1.Length));
            return names;
        }
    }
}
