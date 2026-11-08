using System.Globalization;

namespace Questie
{
    /// <summary>Reads the parts of a fish's entry in the game's Data/Fish text and turns them into words.</summary>
    public static class FishTimes
    {
        /// <summary>
        /// The times of day a fish bites, from the time field of Data/Fish, pairs of 24 hour clock values such as "600 2600"
        /// (up to 2am the next day). Returns an empty list when the field is blank or cannot be read.
        /// </summary>
        public static List<(int Start, int End)> ParseWindows(string? field)
        {
            var windows = new List<(int, int)>();
            if (string.IsNullOrWhiteSpace(field))
                return windows;

            var parts = field.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i + 1 < parts.Length; i += 2)
            {
                if (int.TryParse(parts[i], NumberStyles.None, CultureInfo.InvariantCulture, out var start)
                    && int.TryParse(parts[i + 1], NumberStyles.None, CultureInfo.InvariantCulture, out var end)
                    && end > start)
                    windows.Add((start, end));
            }
            return windows;
        }

        private static readonly string[] SeasonWords = { "spring", "summer", "fall", "winter" };

        /// <summary>
        /// The seasons a location rule asks for in its condition, such as "LOCATION_SEASON Here spring summer" or "SEASON winter".
        /// A condition holds clauses split by commas that all have to hold, so the seasons of every plain clause are taken together.
        /// A clause that denies a season is not understood, and a condition with one gives no seasons. Null when there are none.
        /// </summary>
        public static IReadOnlyList<int>? SeasonsInCondition(string? condition)
        {
            if (string.IsNullOrWhiteSpace(condition))
                return null;

            IReadOnlyList<int>? found = null;
            foreach (var clause in condition.Split(','))
            {
                var parts = clause.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    continue;
                if (parts[0].StartsWith('!'))
                {
                    if (parts[0].TrimStart('!') is "SEASON" or "LOCATION_SEASON")
                        return null;
                    continue;
                }
                var skip = parts[0] == "LOCATION_SEASON" ? 2 : parts[0] == "SEASON" ? 1 : -1;
                if (skip < 0)
                    continue;

                var seasons = Distinct(parts.Skip(skip));
                if (seasons is null)
                    continue;
                // clauses that all have to hold narrow each other down
                found = found is null ? seasons : found.Intersect(seasons).OrderBy(s => s).ToList();
            }
            return found;
        }

        private static IEnumerable<string> Words(string? field) =>
            string.IsNullOrWhiteSpace(field) ? Array.Empty<string>() : field.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        private static IReadOnlyList<int>? Distinct(IEnumerable<string> words)
        {
            var seasons = new SortedSet<int>();
            foreach (var word in words)
            {
                var index = Array.IndexOf(SeasonWords, word.ToLowerInvariant());
                if (index >= 0)
                    seasons.Add(index);
            }
            return seasons.Count == 0 || seasons.Count == 4 ? null : seasons.ToList();
        }

        /// <summary>Whether the windows cover the whole day, so no time needs to be mentioned.</summary>
        public static bool IsAllDay(IReadOnlyList<(int Start, int End)> windows) =>
            windows.Count == 1 && windows[0].Start <= 600 && windows[0].End >= 2600;

        /// <summary>A clock value such as 1830 as "6:30pm". Values past 2400 count as the next morning, and 2600 is "2am".</summary>
        public static string FormatTime(int clock)
        {
            var hour = clock / 100 % 24;
            var minute = clock % 100;
            var suffix = hour >= 12 ? "pm" : "am";
            var twelve = hour % 12 == 0 ? 12 : hour % 12;
            return minute == 0 ? $"{twelve}{suffix}" : $"{twelve}:{minute:00}{suffix}";
        }

        /// <summary>The windows as text, such as "6am to 7pm" or "6am to 11am, 5pm to 2am".</summary>
        public static string FormatWindows(IReadOnlyList<(int Start, int End)> windows, Func<int, string>? format = null) =>
            string.Join(", ", windows.Select(w => $"{(format ?? FormatTime)(w.Start)} to {(format ?? FormatTime)(w.End)}"));

        /// <summary>Whether a time of day falls inside any of the windows. No windows means the fish bites all day.</summary>
        public static bool IsOpen(IReadOnlyList<(int Start, int End)> windows, int clock)
        {
            if (windows.Count == 0)
                return true;
            foreach (var (start, end) in windows)
            {
                if (clock >= start && clock < end)
                    return true;
            }
            return false;
        }

        /// <summary>Whether the weather suits a fish. "sunny" wants no rain, "rainy" wants rain, and anything else suits both.</summary>
        public static bool WeatherSuits(string weather, bool raining) => weather switch
        {
            "sunny" => !raining,
            "rainy" => raining,
            _ => true,
        };
    }
}
