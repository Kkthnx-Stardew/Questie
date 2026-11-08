using StardewValley;

namespace Questie
{
    /// <summary>Finds an item from the name tag the game gives it, such as item_hardwood, which special orders use to say what they accept.</summary>
    internal sealed class ItemTagIndex
    {
        private Dictionary<string, string>? _byTag;

        /// <summary>Forgets the index. Called when the item data changes.</summary>
        public void Reset() => _byTag = null;

        /// <summary>The qualified id of the item with the tag, or null when none has it. The first item found wins when two share a name.</summary>
        public string? Find(string tag)
        {
            _byTag ??= Build();
            return _byTag.TryGetValue(tag, out var id) ? id : null;
        }

        private static Dictionary<string, string> Build()
        {
            var index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (id, data) in Game1.objectData)
            {
                if (!string.IsNullOrWhiteSpace(data.Name))
                    index.TryAdd("item_" + ItemContextTagManager.SanitizeContextTag(data.Name), "(O)" + id);
            }
            foreach (var (id, data) in Game1.bigCraftableData)
            {
                if (!string.IsNullOrWhiteSpace(data.Name))
                    index.TryAdd("item_" + ItemContextTagManager.SanitizeContextTag(data.Name), "(BC)" + id);
            }
            return index;
        }
    }
}
