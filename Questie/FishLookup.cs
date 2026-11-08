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

        /// <param name="more">Builds the "and 3 more" text for the places that did not fit.</param>
        public FishLookup(Func<string, string> more)
        {
            _more = more;
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
            var time = windows.Count == 0 || FishTimes.IsAllDay(windows) ? null : FishTimes.FormatWindows(windows, Game1.getTimeOfDayString);
            var weather = fields.Length > 7 ? fields[7] : "both";

            var everywhere = FishesAnywhere(qualifiedItemId);
            var places = everywhere ? new List<string>() : Places(qualifiedItemId);
            return new FishInfo(places, everywhere, time, weather);
        }

        /// <summary>Whether the game's default list, which every place falls back on, includes the fish.</summary>
        private static bool FishesAnywhere(string qualifiedItemId)
        {
            if (!Game1.locationData.TryGetValue("Default", out var data) || data.Fish is null)
                return false;
            foreach (var spawn in data.Fish)
            {
                if (Matches(spawn, qualifiedItemId))
                    return true;
            }
            return false;
        }

        private List<string> Places(string qualifiedItemId)
        {
            var places = new List<string>();
            foreach (var (name, location) in Game1.locationData)
            {
                if (name == "Default" || location.Fish is null)
                    continue;

                var allYear = false;
                var seasons = new List<Season>();
                var found = false;
                foreach (var spawn in location.Fish)
                {
                    if (!Matches(spawn, qualifiedItemId))
                        continue;
                    found = true;
                    if (spawn.Season is { } season)
                    {
                        if (!seasons.Contains(season))
                            seasons.Add(season);
                    }
                    else
                        allYear = true;
                }
                if (!found)
                    continue;

                var display = DisplayName(name, location);
                places.Add(allYear || seasons.Count == 0 || seasons.Count == 4
                    ? display
                    : $"{display} ({string.Join(", ", seasons.OrderBy(s => (int)s).Select(s => Utility.getSeasonNameFromNumber((int)s).ToLowerInvariant()))})");
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
            if (!string.IsNullOrWhiteSpace(named))
                return named;

            // a place with no display name in its data may still have one on the live location, for example the Ginger Island areas
            var live = Game1.getLocationFromName(internalName)?.GetDisplayName();
            if (!string.IsNullOrWhiteSpace(live) && live != internalName)
                return live;

            // nothing readable anywhere, so split the internal name into words, such as IslandSouthEast into Island South East
            return System.Text.RegularExpressions.Regex.Replace(internalName, "(?<=[a-z])(?=[A-Z])", " ").Replace('_', ' ');
        }
    }
}
