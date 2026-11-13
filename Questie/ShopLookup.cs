using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Internal;

namespace Questie
{
    /// <summary>Finds which shops sell an item for gold, from the game's own shop data.</summary>
    internal sealed class ShopLookup
    {
        private static readonly IReadOnlyList<ShopOffer> None = Array.Empty<ShopOffer>();

        /// <summary>One shop's entry for an item, kept until a price is asked for.</summary>
        private readonly record struct Stock(string Seller, bool Sometimes, ShopData Shop, ShopItemData Entry);

        /// <summary>The shops for each item, by qualified id. Built the first time it is needed.</summary>
        private Dictionary<string, List<Stock>>? _byItem;

        /// <summary>The offers of the items asked about so far, with their prices worked out.</summary>
        private readonly Dictionary<string, IReadOnlyList<ShopOffer>> _offers = new(StringComparer.Ordinal);

        /// <summary>Forgets the index. Called when the shop data or the language changes.</summary>
        public void Reset()
        {
            _byItem = null;
            _offers.Clear();
        }

        public IReadOnlyList<ShopOffer> Find(string qualifiedItemId)
        {
            _byItem ??= Build();
            if (_offers.TryGetValue(qualifiedItemId, out var known))
                return known;
            if (!_byItem.TryGetValue(qualifiedItemId, out var stock))
                return None;

            // a shop that always stocks the item is named before one that only does at times, and the cheapest comes first
            // so a shop that asks far more than the rest is the one left off when only a few are named
            var offers = stock.Select(s => new ShopOffer(s.Seller, PriceOf(s, qualifiedItemId), s.Sometimes))
                .OrderBy(o => o.Sometimes).ThenBy(o => o.Price > 0 ? o.Price : int.MaxValue).ToList();
            _offers[qualifiedItemId] = offers;
            return offers;
        }

        /// <summary>
        /// The price the shop asks, worked out the way the game works it out when it stocks the shop, with the item's own price or the
        /// default one, the profit margin and the price modifiers. 0 when it cannot be worked out.
        /// </summary>
        private static int PriceOf(Stock stock, string qualifiedItemId)
        {
            try
            {
                var item = ItemRegistry.Create(qualifiedItemId);
                float price = ShopBuilder.GetBasePrice(new ItemQueryResult(item), stock.Shop, stock.Entry, item, false, stock.Entry.UseObjectDataPrice);
                var random = Utility.CreateDaySaveRandom();
                if (!stock.Entry.IgnoreShopPriceModifiers)
                    price = Utility.ApplyQuantityModifiers(price, stock.Shop.PriceModifiers, stock.Shop.PriceModifierMode, null, null, item, null, random);
                price = Utility.ApplyQuantityModifiers(price, stock.Entry.PriceModifiers, stock.Entry.PriceModifierMode, null, null, item, null, random);
                return Math.Max(0, (int)price);
            }
            catch (Exception)
            {
                // an item or a modifier from another mod that cannot be priced is listed without a price, which is true
                return 0;
            }
        }

        private static Dictionary<string, List<Stock>> Build()
        {
            var index = new Dictionary<string, List<Stock>>(StringComparer.Ordinal);
            foreach (var (shopId, shop) in DataLoader.Shops(Game1.content))
            {
                // only shops that take gold, and only items listed by id (a list can also hold item queries)
                if (shop.Currency != 0 || shop.Items is null)
                    continue;

                // a festival's stalls are open for a day and never a place to go and buy, and they carry their festival's name
                if (shopId.Contains("Festival", StringComparison.OrdinalIgnoreCase))
                    continue;

                var seller = Seller(shopId, shop);
                if (seller is null)
                    continue;
                foreach (var item in shop.Items)
                {
                    if (item.TradeItemId is not null || string.IsNullOrWhiteSpace(item.ItemId))
                        continue;
                    var qualified = ItemRegistry.QualifyItemId(item.ItemId);
                    if (qualified is null || !ItemRegistry.Exists(qualified))
                        continue;

                    if (!index.TryGetValue(qualified, out var stock))
                        index[qualified] = stock = new List<Stock>();
                    if (stock.All(o => o.Seller != seller))
                        stock.Add(new Stock(seller, !string.IsNullOrWhiteSpace(item.Condition), shop, item));
                }
            }
            return index;
        }

        /// <summary>
        /// The person who runs the shop, or the shop's id made readable when it has no named owner. Another mod's shop with no owner has
        /// an id such as "Mod.Author_VendorTwo", which is not a name a player knows, so it is left out and null is returned.
        /// </summary>
        private static string? Seller(string shopId, ShopData shop)
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
            return shopId.IndexOfAny(new[] { '_', '.' }) >= 0 ? null : NameText.Humanize(shopId);
        }
    }
}
