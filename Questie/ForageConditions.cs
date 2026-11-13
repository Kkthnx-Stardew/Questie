using System.Text;

namespace Questie
{
    /// <summary>Reads the seasons out of a location forage entry's game state query.</summary>
    public static class ForageConditions
    {
        private static readonly int[] EverySeason = { 0, 1, 2, 3 };

        /// <summary>
        /// The seasons a condition can allow, or null when it puts no limit on seasons. An empty list means the condition can never
        /// match. Conditions that are not about seasons, such as a year, a letter or the weather, are ignored.
        /// </summary>
        public static IReadOnlyList<int>? Seasons(string? condition)
        {
            if (string.IsNullOrWhiteSpace(condition))
                return null;
            return ParseQuery(condition);
        }

        private static IReadOnlyList<int>? ParseQuery(string text)
        {
            IReadOnlyList<int>? allowed = null;
            foreach (var condition in SplitConditions(text))
            {
                allowed = Intersect(allowed, ParseCondition(condition));
                if (allowed is { Count: 0 })
                    break;
            }
            return allowed;
        }

        private static IReadOnlyList<int>? ParseCondition(string text)
        {
            var args = SplitArguments(text);
            if (args.Count == 0)
                return null;

            var keyword = args[0];
            var negated = keyword.StartsWith('!');
            if (negated)
                keyword = keyword[1..];

            return keyword.ToUpperInvariant() switch
            {
                "SEASON" => SeasonSet(args.Skip(1), negated),
                // the first argument of LOCATION_SEASON is the place it applies to
                "LOCATION_SEASON" => SeasonSet(args.Skip(2), negated),
                "SEASON_DAY" => SeasonSet(args.Skip(1), negated),
                "DATE_RANGE" => DateRange(args.Skip(1), negated),
                "ANY" => AnyOf(args.Skip(1), negated),
                "TRUE" => negated ? Array.Empty<int>() : null,
                "FALSE" => negated ? null : Array.Empty<int>(),
                _ => null,
            };
        }

        private static IReadOnlyList<int>? SeasonSet(IEnumerable<string> values, bool negated)
        {
            var seasons = new HashSet<int>();
            foreach (var value in values)
            {
                if (TrySeason(value, out var season))
                    seasons.Add(season);
            }
            if (seasons.Count == 0)
                return null; // a malformed season condition says nothing this can use
            return negated ? Complement(seasons) : Sorted(seasons);
        }

        private static IReadOnlyList<int>? DateRange(IEnumerable<string> values, bool negated)
        {
            var args = values.ToList();
            if (args.Count < 3 || !TrySeason(args[0], out var first) || !int.TryParse(args[2], out var firstYear))
                return null;
            // an omitted max year is unlimited, so the range can reach any season in a later year
            if (args.Count < 6 || !int.TryParse(args[5], out var lastYear) || lastYear < firstYear)
                return null;

            // DATE_RANGE minSeason minDay minYear [maxSeason maxDay maxYear], with winter as the default last season
            var last = args.Count > 3 && TrySeason(args[3], out var lastSeason) ? lastSeason : 3;
            var seasons = new HashSet<int>();
            if (lastYear == firstYear)
            {
                if (last < first)
                    return null;
                for (var season = first; season <= last; season++)
                    seasons.Add(season);
            }
            else if (lastYear == firstYear + 1)
            {
                for (var season = first; season < 4; season++)
                    seasons.Add(season);
                for (var season = 0; season <= last; season++)
                    seasons.Add(season);
            }
            else
            {
                // more than one whole year in between means any season can be in range
                return null;
            }
            return negated ? Complement(seasons) : Sorted(seasons);
        }

        private static IReadOnlyList<int>? AnyOf(IEnumerable<string> values, bool negated)
        {
            var alternatives = values.ToList();
            if (alternatives.Count == 0)
                return null;

            var seasons = new HashSet<int>();
            var anyUnrestricted = false;
            foreach (var alternative in alternatives)
            {
                var allowed = ParseQuery(alternative);
                if (allowed is null)
                {
                    anyUnrestricted = true;
                    continue;
                }
                seasons.UnionWith(allowed);
            }

            if (seasons.Count == 0)
            {
                // a positive ANY of impossible queries is impossible; otherwise no alternative limits the seasons
                if (negated || anyUnrestricted)
                    return null;
                return Array.Empty<int>();
            }
            // a positive ANY with one unrestricted alternative can happen in any season
            if (!negated && anyUnrestricted)
                return null;
            // a negative ANY excludes only the alternatives that name seasons
            return negated ? Complement(seasons) : Sorted(seasons);
        }

        private static IReadOnlyList<int>? Intersect(IReadOnlyList<int>? left, IReadOnlyList<int>? right)
        {
            if (left is null)
                return right;
            if (right is null)
                return left;
            var allowed = new HashSet<int>(right);
            return left.Where(allowed.Contains).OrderBy(season => season).ToList();
        }

        private static List<int> Complement(IEnumerable<int> seasons)
        {
            var excluded = new HashSet<int>(seasons);
            return EverySeason.Where(season => !excluded.Contains(season)).ToList();
        }

        private static List<int> Sorted(IEnumerable<int> seasons) => seasons.OrderBy(season => season).ToList();

        private static bool TrySeason(string value, out int season)
        {
            switch (value.ToLowerInvariant())
            {
                case "spring": season = 0; return true;
                case "summer": season = 1; return true;
                case "fall": season = 2; return true;
                case "winter": season = 3; return true;
                default: season = 0; return false;
            }
        }

        private static List<string> SplitConditions(string text)
        {
            var conditions = new List<string>();
            var current = new StringBuilder();
            var quoted = false;
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '\\' && i + 1 < text.Length)
                {
                    current.Append(c).Append(text[++i]);
                    continue;
                }
                if (c == '"')
                {
                    quoted = !quoted;
                    current.Append(c);
                    continue;
                }
                if (c == ',' && !quoted)
                {
                    Add(current, conditions);
                    continue;
                }
                current.Append(c);
            }
            Add(current, conditions);
            return conditions;
        }

        private static List<string> SplitArguments(string text)
        {
            var args = new List<string>();
            var current = new StringBuilder();
            var quoted = false;
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '\\' && i + 1 < text.Length)
                {
                    current.Append(text[++i]);
                    continue;
                }
                if (c == '"')
                {
                    quoted = !quoted;
                    continue;
                }
                if (char.IsWhiteSpace(c) && !quoted)
                {
                    if (current.Length > 0)
                    {
                        args.Add(current.ToString());
                        current.Clear();
                    }
                    continue;
                }
                current.Append(c);
            }
            if (current.Length > 0)
                args.Add(current.ToString());
            return args;
        }

        private static void Add(StringBuilder current, List<string> values)
        {
            var value = current.ToString().Trim();
            if (value.Length > 0)
                values.Add(value);
            current.Clear();
        }
    }
}
