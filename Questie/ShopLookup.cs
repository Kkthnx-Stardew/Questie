using StardewValley;

namespace Questie
{
    /// <summary>Finds which shops sell an item for gold, from the game's own shop data.</summary>
    internal sealed class ShopLookup
    {
        private static readonly IReadOnlyList<ShopOffer> None = Array.Empty<ShopOffer>();

        /// <summary>The shops for each item, by qualified id. Built the first time it is needed.</summary>
        private Dictionary<string, List<ShopOffer>>? _byItem;

        /// <summary>Forgets the index. Called when the shop data or the language changes.</summary>
        public void Reset() => _byItem = null;

        public IReadOnlyList<ShopOffer> Find(string qualifiedItemId)
        {
            _byItem ??= Build();
            return _byItem.TryGetValue(qualifiedItemId, out var offers) ? offers : None;
        }

        private static Dictionary<string, List<ShopOffer>> Build()
        {
            var index = new Dictionary<string, List<ShopOffer>>(StringComparer.Ordinal);
            foreach (var (shopId, shop) in DataLoader.Shops(Game1.content))
            {
                // only shops that take gold, and only items listed by id (a list can also hold item queries)
                if (shop.Currency != 0 || shop.Items is null)
                    continue;

                var seller = Seller(shopId, shop);
                foreach (var item in shop.Items)
                {
                    if (item.TradeItemId is not null || string.IsNullOrWhiteSpace(item.ItemId))
                        continue;
                    var qualified = ItemRegistry.QualifyItemId(item.ItemId);
                    if (qualified is null || !ItemRegistry.Exists(qualified))
                        continue;

                    if (!index.TryGetValue(qualified, out var offers))
                        index[qualified] = offers = new List<ShopOffer>();
                    if (offers.All(o => o.Seller != seller))
                        offers.Add(new ShopOffer(seller, item.Price > 0 ? item.Price : 0, !string.IsNullOrWhiteSpace(item.Condition)));
                }
            }
            return index;
        }

        /// <summary>The person who runs the shop, or the shop's id when it has no named owner.</summary>
        private static string Seller(string shopId, StardewValley.GameData.Shops.ShopData shop)
        {
            if (shop.Owners is not null)
            {
                foreach (var owner in shop.Owners)
                {
                    var name = owner.Name;
                    if (!string.IsNullOrWhiteSpace(name) && name is not ("AnyOrNone" or "Any" or "None"))
                        return NPC.GetDisplayName(name);
                }
            }
            return NameText.Humanize(shopId);
        }
    }
}
