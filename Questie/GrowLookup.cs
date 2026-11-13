using StardewValley;

namespace Questie
{
    /// <summary>Finds how an item is grown, from the game's own crop data.</summary>
    internal sealed class GrowLookup
    {
        private Dictionary<string, GrowInfo>? _byHarvest;
        private Dictionary<string, GrowInfo>? _byTag;

        /// <summary>Forgets the index. Called when the crop data or the language changes.</summary>
        public void Reset()
        {
            _byHarvest = null;
            _byTag = null;
        }

        /// <summary>How the item is grown, or null when it is not a crop.</summary>
        public GrowInfo? Find(string qualifiedItemId)
        {
            if (string.IsNullOrWhiteSpace(qualifiedItemId))
                return null;
            EnsureHarvestIndex();
            return _byHarvest!.TryGetValue(qualifiedItemId, out var info) ? info : null;
        }

        /// <summary>How the item a tag names is grown, or null when the tag names no single crop.</summary>
        public GrowInfo? FindByTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return null;
            EnsureHarvestIndex();
            EnsureTagIndex();
            return _byTag!.TryGetValue(tag.Trim(), out var info) ? info : null;
        }

        private void EnsureHarvestIndex()
        {
            if (_byHarvest is not null)
                return;
            try
            {
                _byHarvest = BuildHarvestIndex();
            }
            catch
            {
                // a crop data problem must not take the rest of the quest's notes with it
                _byHarvest = new Dictionary<string, GrowInfo>(StringComparer.Ordinal);
            }
        }

        private void EnsureTagIndex()
        {
            if (_byTag is not null)
                return;
            try
            {
                _byTag = BuildTagIndex(_byHarvest!);
            }
            catch
            {
                _byTag = new Dictionary<string, GrowInfo>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static Dictionary<string, GrowInfo> BuildHarvestIndex()
        {
            var byHarvest = new Dictionary<string, GrowInfo>(StringComparer.Ordinal);
            foreach (var (seedId, crop) in Game1.cropData)
            {
                try
                {
                    if (crop?.HarvestItemId is not { Length: > 0 } harvestId)
                        continue;
                    var seedName = SeedName(seedId)?.Trim();
                    if (string.IsNullOrWhiteSpace(seedName))
                        continue;

                    var qualifiedHarvest = ItemRegistry.QualifyItemId(harvestId) ?? "(O)" + harvestId;
                    var seasons = crop.Seasons is { Count: > 0 } ? crop.Seasons.Select(season => (int)season).ToList() : new List<int>();
                    var days = crop.DaysInPhase is { Count: > 0 } ? crop.DaysInPhase.Sum() : 0;
                    // a harvest can come from more than one seed; the first the game lists is enough to point the farmer at
                    byHarvest.TryAdd(qualifiedHarvest, new GrowInfo(seedName, seasons, days, crop.RegrowDays));
                }
                catch
                {
                    // one crop from another mod that does not look the way this expects must not stop the rest
                }
            }
            return byHarvest;
        }

        /// <summary>
        /// The tags that plainly name a crop's harvest item, such as id_o_192 for a potato, item_potato or a crop_ tag. A tag shared
        /// by two crops names neither one, so it is left out.
        /// </summary>
        private static Dictionary<string, GrowInfo> BuildTagIndex(Dictionary<string, GrowInfo> byHarvest)
        {
            var byTag = new Dictionary<string, GrowInfo>(StringComparer.OrdinalIgnoreCase);
            var sharedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (harvestId, info) in byHarvest)
            {
                try
                {
                    foreach (var tag in ItemContextTagManager.GetBaseContextTags(harvestId))
                    {
                        if (!NamesOneItem(tag))
                            continue;
                        if (byTag.TryGetValue(tag, out var other))
                        {
                            if (!ReferenceEquals(other, info))
                                sharedTags.Add(tag);
                        }
                        else
                            byTag[tag] = info;
                    }
                }
                catch
                {
                    // one crop from another mod that does not look the way this expects must not stop the rest
                }
            }

            foreach (var tag in sharedTags)
                byTag.Remove(tag);
            return byTag;
        }

        /// <summary>Whether the tag is an id, internal name or crop tag and so plainly names one item rather than a category or a colour.</summary>
        private static bool NamesOneItem(string tag) =>
            tag.StartsWith("id_", StringComparison.OrdinalIgnoreCase) ||
            tag.StartsWith("crop_", StringComparison.OrdinalIgnoreCase) ||
            (tag.StartsWith("item_", StringComparison.OrdinalIgnoreCase) &&
             !tag.StartsWith("item_type_", StringComparison.OrdinalIgnoreCase));

        /// <summary>The display name of a seed, which is an object except for a few placeable seeds such as the tea sapling.</summary>
        private static string? SeedName(string seedId)
        {
            var objectData = ItemRegistry.GetData("(O)" + seedId);
            if (objectData is not null && LooksLikeSeed(objectData.DisplayName))
                return objectData.DisplayName;
            var bigData = ItemRegistry.GetData("(BC)" + seedId);
            if (bigData is not null && LooksLikeSeed(bigData.DisplayName))
                return bigData.DisplayName;
            return (objectData ?? bigData)?.DisplayName;
        }

        private static bool LooksLikeSeed(string? name) =>
            !string.IsNullOrWhiteSpace(name) &&
            (name.Contains("Seed", StringComparison.OrdinalIgnoreCase) || name.Contains("Sapling", StringComparison.OrdinalIgnoreCase)
             || name.Contains("Bean", StringComparison.OrdinalIgnoreCase) || name.Contains("Start", StringComparison.OrdinalIgnoreCase)
             || name.Contains("Shoot", StringComparison.OrdinalIgnoreCase) || name.Contains("Tuber", StringComparison.OrdinalIgnoreCase));
    }
}
