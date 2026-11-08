using StardewValley;
using StardewValley.Inventories;
using StardewValley.Objects;

namespace Questie
{
    /// <summary>Counts what lies in the chests and the fridge, for a quest that asks for an item. Counted again at most once a second, and only when a quest note asks.</summary>
    internal sealed class ChestLookup
    {
        private const int RefreshTicks = 60;
        private const int Never = int.MinValue / 2;

        private readonly Dictionary<string, int> _counts = new(StringComparer.Ordinal);
        private int _tick = Never;

        /// <summary>Forgets the counts. Called when the game's data or the language changes.</summary>
        public void Reset() => _tick = Never;

        public int Count(string qualifiedItemId)
        {
            if (Game1.ticks - _tick >= RefreshTicks || Game1.ticks < _tick)
                Rebuild();
            return _counts.TryGetValue(qualifiedItemId, out var count) ? count : 0;
        }

        private void Rebuild()
        {
            _tick = Game1.ticks;
            _counts.Clear();

            // Junimo chests share one inventory, so each inventory is counted once however many chests show it
            var seen = new HashSet<IInventory>(ReferenceEqualityComparer.Instance);
            Utility.ForEachLocation(location =>
            {
                foreach (var obj in location.objects.Values)
                {
                    if (obj is Chest chest && Counts(chest))
                        Add(chest, seen);
                }
                if (location.GetFridge() is { } fridge)
                    Add(fridge, seen);
                return true;
            });
        }

        /// <summary>A mini shipping bin holds what is about to be sold, and a loader what is about to be used up, so neither is stock to count.</summary>
        private static bool Counts(Chest chest) => chest.SpecialChestType is Chest.SpecialChestTypes.None or Chest.SpecialChestTypes.JunimoChest or Chest.SpecialChestTypes.BigChest;

        private void Add(Chest chest, HashSet<IInventory> seen)
        {
            var items = chest.GetItemsForPlayer();
            if (!seen.Add(items))
                return;
            foreach (var item in items)
            {
                if (item is not null)
                    _counts[item.QualifiedItemId] = _counts.GetValueOrDefault(item.QualifiedItemId) + item.Stack;
            }
        }
    }
}
