using StardewValley;
using StardewValley.GameData.Locations;
using StardewValley.TokenizableStrings;

namespace Questie
{
    /// <summary>Works out where, when and in what weather a fish bites, from the game's own fish and location data.</summary>
    internal sealed class FishLookup
    {
        /// <summary>The most places named for one fish before the rest are counted.</summary>
        private const int MaxPlaces = 6;

        private readonly Dictionary<string, FishInfo?> _cache = new(StringComparer.Ordinal);
        private readonly Func<string, string> _more;
        private readonly Func<string, string, string> _range;

        /// <param name="more">Builds the "and 3 more" text for the places that did not fit.</param>
        /// <param name="range">Builds "6am to 1pm" from the two times.</param>
        public FishLookup(Func<string, string> more, Func<string, string, string> range)
        {
            _more = more;
            _range = range;
        }

        /// <summary>Forgets what was worked out. Called when the data or the language changes.</summary>
        public void Reset() => _cache.Clear();

        public FishInfo? Find(string qualifiedItemId)
        {
            if (_cache.TryGetValue(qualifiedItemId, out var cached))
                return cached;
            var info = Build(qualifiedItemId);
            _cache[qualifiedItemId] = info;
            return info;
        }

        private FishInfo? Build(string qualifiedItemId)
        {
            var data = ItemRegistry.GetData(qualifiedItemId);
            if (data is null || data.Category != StardewValley.Object.FishCategory)
                return null;
            if (!DataLoader.Fish(Game1.content).TryGetValue(data.ItemId, out var text))
                return null;
            var fields = text.Split('/');

            // a crab pot fish has its own layout: "name/trap/chance/.../water"
            if (fields.Length > 4 && fields[1] == "trap")
                return new FishInfo(Array.Empty<string>(), false, null, "both", fields[4]);

            var windows = FishTimes.ParseWindows(fields.Length > 5 ? fields[5] : null);
            var time = windows.Count == 0 || FishTimes.IsAllDay(windows) ? null : FishTimes.FormatWindows(windows, Game1.getTimeOfDayString, _range);
            var weather = fields.Length > 7 ? fields[7] : "both";

            var rules = RulesFor(qualifiedItemId, out var defaultRule);
            var everywhere = defaultRule is not null;

            // The game catches a fish by the rules of the place alone and never by the seasons in the fish's own entry, which a fish
            // can be caught outside of, as on Ginger Island. So the seasons are the ones the rules allow.
            var seasons = Union(everywhere ? rules.Select(r => r.Rule).Append(defaultRule!.Value) : rules.Select(r => r.Rule));
            var places = everywhere ? new List<string>() : Places(rules, seasons);
            return new FishInfo(places, everywhere, time, weather, null, windows, seasons);
        }

        /// <summary>What one place's rules say about a fish.</summary>
        /// <param name="AllYear">Whether any rule holds in every season.</param>
        /// <param name="Seasons">The seasons the other rules name, as 0 spring to 3 winter.</param>
        private readonly record struct Rule(bool AllYear, IReadOnlyCollection<int> Seasons);

        /// <summary>Locations that only hold a festival's or a minigame's fish, which are not places to go fishing.</summary>
        private static readonly HashSet<string> NotPlaces = new(StringComparer.Ordinal) { "Temp", "fishingGame" };

        private static readonly int[] WholeYear = { 0, 1, 2, 3 };

        /// <summary>The rules that name the fish in each place, and in the default list every place falls back on.</summary>
        private static List<(string Name, LocationData Data, Rule Rule)> RulesFor(string qualifiedItemId, out Rule? defaultRule)
        {
            defaultRule = null;
            var rules = new List<(string, LocationData, Rule)>();
            foreach (var (name, location) in Game1.locationData)
            {
                if (location.Fish is null || NotPlaces.Contains(name))
                    continue;
                if (RuleIn(location, qualifiedItemId) is not { } rule)
                    continue;
                if (name == "Default")
                    defaultRule = rule;
                else
                    rules.Add((name, location, rule));
            }
            return rules;
        }

        private static Rule? RuleIn(LocationData location, string qualifiedItemId)
        {
            var allYear = false;
            var seasons = new SortedSet<int>();
            var found = false;
            foreach (var spawn in location.Fish)
            {
                // a rule that only holds on festival days is not a place the fish lives in
                if (!Matches(spawn, qualifiedItemId) || spawn.Condition?.Contains("FESTIVAL", StringComparison.OrdinalIgnoreCase) == true)
                    continue;
                found = true;

                // a rule names its season in its own field or in its condition, and a rule with neither holds all year
                var named = spawn.Season is { } season ? new[] { (int)season } : FishTimes.SeasonsInCondition(spawn.Condition)?.ToArray();
                if (named is null)
                    allYear = true;
                else
                    seasons.UnionWith(named);
            }
            return found ? new Rule(allYear, seasons) : null;
        }

        /// <summary>The seasons the rules allow together, or null when they allow every season, or none is named.</summary>
        private static IReadOnlyList<int>? Union(IEnumerable<Rule> rules)
        {
            var union = new SortedSet<int>();
            foreach (var rule in rules)
            {
                if (rule.AllYear)
                    return null;
                union.UnionWith(rule.Seasons);
            }
            return union.Count is 0 or 4 ? null : union.ToList();
        }

        private List<string> Places(List<(string Name, LocationData Data, Rule Rule)> rules, IReadOnlyList<int>? fishSeasons)
        {
            var places = new List<string>();
            foreach (var (name, location, rule) in rules)
            {
                var display = DisplayName(name, location);
                // a place's seasons are only worth saying when they differ from the fish's own, which is said once on its own line
                var differs = !rule.AllYear && rule.Seasons.Count is > 0 and < 4 && !rule.Seasons.SequenceEqual(fishSeasons ?? WholeYear);
                places.Add(differs
                    ? $"{display} ({string.Join(", ", rule.Seasons.Select(s => Utility.getSeasonNameFromNumber(s).ToLowerInvariant()))})"
                    : display);
            }

            places = places.Distinct(StringComparer.Ordinal).ToList();
            places.Sort(StringComparer.CurrentCultureIgnoreCase);
            if (places.Count > MaxPlaces)
            {
                var rest = places.Count - MaxPlaces;
                places = places.Take(MaxPlaces).ToList();
                places.Add(_more(rest.ToString()));
            }
            return places;
        }

        private static bool Matches(SpawnFishData spawn, string qualifiedItemId) =>
            spawn.ItemId is not null && (spawn.ItemId == qualifiedItemId || ItemRegistry.QualifyItemId(spawn.ItemId) == qualifiedItemId);

        private static string DisplayName(string internalName, LocationData data)
        {
            var named = string.IsNullOrWhiteSpace(data.DisplayName) ? null : TokenParser.ParseText(data.DisplayName);
            // a name whose text the game cannot find comes back as "(no translation:key)", which is no name to show
            if (!string.IsNullOrWhiteSpace(named) && !named.StartsWith("(no translation:", StringComparison.Ordinal))
                return named;

            // a place with no display name in its data may still have one on the live location, for example the Ginger Island areas
            var live = Game1.getLocationFromName(internalName)?.GetDisplayName();
            if (!string.IsNullOrWhiteSpace(live) && live != internalName)
                return live;

            // nothing readable anywhere, so split the internal name into words, such as IslandSouthEast into Island South East
            return NameText.Humanize(internalName);
        }
    }
}
