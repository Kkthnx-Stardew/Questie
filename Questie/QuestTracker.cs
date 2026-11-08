using StardewValley;

namespace Questie
{
    /// <summary>
    /// One player's quests as facts, and the marker plan built from them. Each split screen player has their own, because the
    /// quests are theirs. It is refreshed when something changes and a few times a second, and read every frame by the markers.
    /// </summary>
    internal sealed class QuestTracker
    {
        private readonly List<QuestFact> _facts = new();

        public MarkerPlan Plan { get; } = new();

        /// <summary>The qualified ids of the items the quests want, for the marks on item icons.</summary>
        public HashSet<string> WantedItems { get; } = new(StringComparer.Ordinal);

        public IReadOnlyList<QuestFact> Facts => _facts;

        private readonly List<GameOrderWant> _orders = new();

        /// <summary>The open objectives of the special orders that ask for items or monsters.</summary>
        public IReadOnlyList<GameOrderWant> Orders => _orders;

        /// <summary>Whether a special order asks for items the farmer could be carrying.</summary>
        public bool HasOrderItems { get; private set; }

        /// <summary>The order objectives an item fits.</summary>
        public IEnumerable<GameOrderWant> OrdersFor(Item item) => _orders.Where(o => o.Kind != OrderWantKind.Slay && o.Accepts(item));

        /// <summary>What was found out about each item since the orders were last read, so a frame does not match tags again for every slot.</summary>
        private readonly Dictionary<Item, bool> _fits = new(ReferenceEqualityComparer.Instance);

        /// <summary>Whether a special order asks for the item.</summary>
        public bool OrderWants(Item item)
        {
            if (_fits.TryGetValue(item, out var known))
                return known;
            var wanted = false;
            for (var i = 0; i < _orders.Count && !wanted; i++)
                wanted = _orders[i].Kind != OrderWantKind.Slay && _orders[i].Accepts(item);
            _fits[item] = wanted;
            return wanted;
        }

        public void Refresh(ModConfig config)
        {
            var player = Game1.player;
            if (player is null)
            {
                Clear();
                return;
            }

            QuestReader.Read(player, _facts);
            ItemNeeds.Collect(_facts, WantedItems);
            OrderReader.Read(player, _orders);
            _fits.Clear();
            HasOrderItems = _orders.Any(o => o.Kind != OrderWantKind.Slay);
            Plan.Rebuild(_facts, config, id => player.Items.CountId(id), _orders);
        }

        public void Clear()
        {
            _facts.Clear();
            _orders.Clear();
            _fits.Clear();
            HasOrderItems = false;
            WantedItems.Clear();
            Plan.Rebuild(_facts, new ModConfig { EnableMarkers = false }, _ => 0);
        }
    }
}
