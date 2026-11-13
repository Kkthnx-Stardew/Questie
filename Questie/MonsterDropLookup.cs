using System.Globalization;
using StardewValley;
using StardewValley.Monsters;

namespace Questie
{
    /// <summary>Finds which monsters drop an item, from the game's monster data and its monster code.</summary>
    internal sealed class MonsterDropLookup
    {
        private Dictionary<string, DropInfo>? _byItem;

        /// <summary>Forgets the index. Called when the monster data or the language changes.</summary>
        public void Reset() => _byItem = null;

        /// <summary>Which monsters drop the item, or null when none do.</summary>
        public DropInfo? Find(string qualifiedItemId)
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
                    // a monster data problem must not take the rest of the quest's notes with it
                    _byItem = new Dictionary<string, DropInfo>(StringComparer.Ordinal);
                }
            }
            return _byItem.TryGetValue(qualifiedItemId, out var info) ? info : null;
        }

        private static Dictionary<string, DropInfo> Build()
        {
            var byItem = new Dictionary<string, Builder>(StringComparer.Ordinal);
            foreach (var (monsterId, raw) in DataLoader.Monsters(Game1.content))
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(raw))
                        continue;
                    var fields = raw.Split('/');
                    if (fields.Length <= 14)
                        continue;
                    var monster = fields[14].Trim();
                    if (monster.Length == 0)
                        monster = monsterId;
                    var places = MonsterPlaces.For(monsterId, monster);

                    // field 6 is the item id and chance of each possible drop, in pairs; a repeated item is a separate roll
                    var row = new Dictionary<string, double>(StringComparer.Ordinal);
                    var drops = fields[6].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (var i = 0; i + 1 < drops.Length; i += 2)
                    {
                        var itemId = drops[i].Trim();
                        // a negative id names a category, not one item, so there is nothing to look up
                        if (itemId.Length == 0 || itemId.StartsWith('-'))
                            continue;
                        if (!double.TryParse(drops[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var chance) || chance <= 0)
                            continue;

                        var qualified = ItemRegistry.QualifyItemId(itemId) ?? "(O)" + itemId;
                        chance = Math.Min(chance, 1);
                        // the chance of at least one when the same item is rolled more than once
                        row[qualified] = row.TryGetValue(qualified, out var previous)
                            ? 1 - (1 - previous) * (1 - chance)
                            : chance;
                    }

                    foreach (var (qualified, chance) in row)
                        Add(byItem, qualified, chance, monster, places, combine: false);
                }
                catch
                {
                    // one monster from another mod that does not look the way this expects must not stop the rest
                }
            }

            // a few drops live in the game's monster code instead of Data/Monsters; they are added after the data so a
            // monster that has both is still one entry, with the chance of either roll producing the item
            foreach (var extra in MonsterExtraDrops.All)
            {
                try
                {
                    var display = Monster.GetDisplayName(extra.Monster);
                    if (string.IsNullOrWhiteSpace(display))
                        display = extra.Monster;
                    Add(byItem, extra.ItemId, extra.Chance, display, MonsterPlaces.For(extra.Monster, display), combine: true);
                }
                catch
                {
                    // an extra drop that cannot be resolved must not stop the rest
                }
            }

            var result = new Dictionary<string, DropInfo>(StringComparer.Ordinal);
            foreach (var (itemId, builder) in byItem)
            {
                var monsters = builder.Monsters
                    .OrderByDescending(pair => pair.Value.Chance)
                    .ThenBy(pair => pair.Key, StringComparer.CurrentCultureIgnoreCase)
                    .Select(pair => pair.Value)
                    .ToList();
                result[itemId] = new DropInfo(monsters);
            }
            return result;
        }

        /// <summary>
        /// Adds one roll to the index. A data row with the same display name as another data row keeps the better chance,
        /// while a code-added extra is combined with the data chance because both can drop on the same kill.
        /// </summary>
        private static void Add(Dictionary<string, Builder> byItem, string qualified, double chance, string monster, IReadOnlyList<string> places, bool combine)
        {
            if (!byItem.TryGetValue(qualified, out var builder))
            {
                builder = new Builder();
                byItem[qualified] = builder;
            }

            chance = Math.Clamp(chance, 0, 1);
            if (builder.Monsters.TryGetValue(monster, out var previous))
            {
                if (!combine && chance <= previous.Chance)
                    return;
                if (combine)
                    chance = 1 - (1 - previous.Chance) * (1 - chance);
                if (places.Count == 0)
                    places = previous.Places;
            }
            builder.Monsters[monster] = new MonsterDrop(monster, chance, places);
        }

        private sealed class Builder
        {
            public Dictionary<string, MonsterDrop> Monsters { get; } = new(StringComparer.CurrentCultureIgnoreCase);
        }
    }
}
