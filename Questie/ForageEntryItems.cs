namespace Questie
{
    /// <summary>Finds every item a forage entry can produce, including the results of an item query.</summary>
    public static class ForageEntryItems
    {
        /// <summary>
        /// The qualified item IDs of a forage entry. A plain item ID is returned once; an item query is resolved to
        /// every item it can produce, so each of them gets the same place and seasons in the hint.
        /// </summary>
        public static IReadOnlyList<string> Resolve(
            string? itemId,
            Func<string, string?> qualify,
            Func<string, IReadOnlyList<string>> resolveQuery)
        {
            var id = itemId?.Trim();
            if (string.IsNullOrWhiteSpace(id))
                return Array.Empty<string>();

            var plainId = qualify(id);
            if (!string.IsNullOrWhiteSpace(plainId))
                return new[] { plainId };

            IReadOnlyList<string>? resolved;
            try
            {
                resolved = resolveQuery(id);
            }
            catch
            {
                return Array.Empty<string>();
            }
            if (resolved is null)
                return Array.Empty<string>();

            var ids = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in resolved)
            {
                var resolvedId = value?.Trim();
                if (!string.IsNullOrWhiteSpace(resolvedId) && seen.Add(resolvedId))
                    ids.Add(resolvedId);
            }
            return ids;
        }
    }
}
