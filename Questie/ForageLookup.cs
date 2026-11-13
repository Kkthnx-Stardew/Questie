using StardewValley;
using StardewValley.GameData.Locations;
using StardewValley.Internal;

namespace Questie
{
    /// <summary>Finds where and in which seasons an item can be foraged, from the game's own location data.</summary>
    internal sealed class ForageLookup
    {
        private static readonly int[] EverySeason = { 0, 1, 2, 3 };

        private Dictionary<string, ForageInfo>? _byItem;

        /// <summary>Forgets the index. Called when the location data or the language changes.</summary>
        public void Reset() => _byItem = null;

        /// <summary>Where and when the item can be foraged, or null when it is not forage.</summary>
        public ForageInfo? Find(string qualifiedItemId)
        {
            if (string.IsNullOrWhiteSpace(qualifiedItemId))
                return null;
            if (_byItem is null)
            {
                try
                {
                    _byItem = Build();
                }
                catch
                {
                    // a location data problem must not take the rest of the quest's notes with it
                    _byItem = new Dictionary<string, ForageInfo>(StringComparer.Ordinal);
                }
            }
            return _byItem.TryGetValue(qualifiedItemId, out var info) ? info : null;
        }

        private static Dictionary<string, ForageInfo> Build()
        {
            var byItem = new Dictionary<string, Builder>(StringComparer.Ordinal);
            // Queries like RANDOM_ITEMS use a random order while enumerating. A private random keeps the hint index
            // from changing the save's random state, and the order does not matter because every result is indexed.
            var random = new Random(0);
            Utility.ForEachLocation(location =>
            {
                try
                {
                    if (location.GetData() is not { Forage: { Count: > 0 } forage })
                        return true;
                    var place = PlaceName(location);
                    if (place is null)
                        return true;

                    foreach (var entry in forage)
                    {
                        try
                        {
                            if (entry is null || entry.Chance <= 0)
                                continue;
                            var itemIds = ForageEntryItems.Resolve(
                                entry.ItemId,
                                ItemRegistry.QualifyItemId,
                                query => ResolveItemQuery(query, entry.PerItemCondition, location, random));
                            if (itemIds.Count == 0)
                                continue;

                            IReadOnlyList<int>? seasons = entry.Season is { } entrySeason
                                ? new[] { (int)entrySeason }
                                : ForageConditions.Seasons(entry.Condition);
                            // a condition that can never match names no season at all
                            if (seasons is { Count: 0 })
                                continue;

                            foreach (var itemId in itemIds)
                            {
                                if (!byItem.TryGetValue(itemId, out var builder))
                                {
                                    builder = new Builder();
                                    byItem[itemId] = builder;
                                }
                                if (!builder.Places.TryGetValue(place, out var placeSeasons))
                                {
                                    placeSeasons = new HashSet<int>();
                                    builder.Places[place] = placeSeasons;
                                }
                                foreach (var season in seasons ?? EverySeason)
                                    placeSeasons.Add(season);
                            }
                        }
                        catch
                        {
                            // one forage entry from another mod that does not look the way this expects must not stop the rest
                        }
                    }
                }
                catch
                {
                    // one location must not stop the rest
                }
                return true;
            });

            var result = new Dictionary<string, ForageInfo>(StringComparer.Ordinal);
            foreach (var (itemId, builder) in byItem)
            {
                var places = builder.Places
                    .OrderBy(pair => pair.Key, StringComparer.CurrentCultureIgnoreCase)
                    .Select(pair => new ForagePlace(pair.Key, pair.Value.OrderBy(season => season).ToList()))
                    .ToList();
                result[itemId] = new ForageInfo(places);
            }
            return result;
        }

        /// <summary>The objects an item query can produce, so each one gets the entry's place and seasons.</summary>
        private static IReadOnlyList<string> ResolveItemQuery(string query, string? perItemCondition, GameLocation location, Random random)
        {
            var results = ItemQueryResolver.TryResolve(
                query,
                CreateQueryContext(location, random),
                ItemQuerySearchMode.AllOfTypeItem,
                perItemCondition: perItemCondition,
                logError: (_, _) => { });
            var itemIds = new List<string>();
            foreach (var result in results)
            {
                // forage only spawns objects; a query that produces another kind of item cannot be this entry
                if (result.Item is StardewValley.Object item
                    && !string.IsNullOrWhiteSpace(item.QualifiedItemId)
                    && ItemRegistry.Exists(item.QualifiedItemId))
                    itemIds.Add(item.QualifiedItemId);
            }
            return itemIds;
        }

        /// <summary>
        /// A context that keeps the query's random order away from the save. The constructor gained a source phrase in
        /// Stardew Valley 1.6.14, so the three-argument form is used on the older game versions this mod supports.
        /// </summary>
        private static ItemQueryContext CreateQueryContext(GameLocation location, Random random)
        {
            var type = typeof(ItemQueryContext);
            var withSource = type.GetConstructor(new[] { typeof(GameLocation), typeof(Farmer), typeof(Random), typeof(string) });
            if (withSource is not null)
                return (ItemQueryContext)withSource.Invoke(new object?[] { location, Game1.player, random, null });

            var withoutSource = type.GetConstructor(new[] { typeof(GameLocation), typeof(Farmer), typeof(Random) });
            if (withoutSource is not null)
                return (ItemQueryContext)withoutSource.Invoke(new object?[] { location, Game1.player, random });

            return new ItemQueryContext();
        }

        private static string? PlaceName(GameLocation location)
        {
            var name = location.GetDisplayName();
            if (!string.IsNullOrWhiteSpace(name) && !name.StartsWith("(no translation:", StringComparison.Ordinal))
                return name;
            return string.IsNullOrWhiteSpace(location.Name) ? null : location.Name;
        }

        private sealed class Builder
        {
            public Dictionary<string, HashSet<int>> Places { get; } = new(StringComparer.CurrentCultureIgnoreCase);
        }
    }
}
