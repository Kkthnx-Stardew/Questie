namespace Questie
{
    /// <summary>What a special order objective asks the farmer to do with items or monsters.</summary>
    public enum OrderWantKind
    {
        /// <summary>Hand items to a villager.</summary>
        Deliver,

        /// <summary>Put items in a donation box.</summary>
        Donate,

        /// <summary>Put items in the shipping bin.</summary>
        Ship,

        /// <summary>Kill monsters.</summary>
        Slay,
    }

    /// <summary>
    /// One unfinished objective of a special order that asks for items or monsters. Special orders ask for items by tag, such as "any
    /// vegetable", so there is no single item id to match, and the game side says which items fit. This file has no game dependencies
    /// so the tests can compile it.
    /// </summary>
    public class OrderWant
    {
        /// <summary>The order's key, which tells its objectives apart from another order's.</summary>
        public string OrderKey { get; init; } = "";

        /// <summary>The order's name as the journal shows it.</summary>
        public string Order { get; init; } = "";

        public OrderWantKind Kind { get; init; }

        /// <summary>The objective's own wording, such as "Deliver a Ruby to Emily".</summary>
        public string Text { get; init; } = "";

        /// <summary>For a delivery, the internal name of the villager to hand the items to.</summary>
        public string? Npc { get; init; }

        /// <summary>For a donation, the internal name of the place the box stands in.</summary>
        public string? Place { get; init; }

        /// <summary>For a kill objective, the name parts a monster must contain.</summary>
        public IReadOnlyList<string> Monsters { get; init; } = Array.Empty<string>();

        /// <summary>How far the objective has come, and how far it goes. For an objective counted in gold, these are gold.</summary>
        public int Have { get; init; }

        public int Need { get; init; }

        /// <summary>Whether the count is the value of what was shipped and not a number of items.</summary>
        public bool ByValue { get; init; }

        /// <summary>How many fitting items the farmer carries in all.</summary>
        public int Carried { get; set; }

        /// <summary>The most of one kind the farmer carries in a single stack.</summary>
        public int LargestStack { get; set; }

        /// <summary>The qualified id of the fitting item in that biggest stack, or null when none is carried.</summary>
        public string? HeldItemId { get; set; }

        /// <summary>
        /// The one item tag the objective requires, when its rule asks for a single item, such as the id_o_192 tag the Crop Order
        /// uses. Null when it takes a category, several alternatives, or only excludes things.
        /// </summary>
        public string? ItemTag { get; init; }

        /// <summary>How many are still needed.</summary>
        public int Left => Math.Max(0, Need - Have);

        /// <summary>
        /// Whether the farmer carries enough to hand over now. The game takes a delivery only when one stack covers everything that is
        /// left, and takes a donation or shipment a little at a time.
        /// </summary>
        public bool CanHandOver => Kind == OrderWantKind.Deliver ? LargestStack >= Left && Left > 0 : Carried > 0;
    }

    /// <summary>Reads the tags an order accepts, so a plain tag list can say whether it names one item.</summary>
    internal static class OrderTag
    {
        /// <summary>
        /// The one tag that every possible accepted item must have, or null when the rule names no single item. A tag list is
        /// alternatives, a clause is tags that must all match, and a clause can offer alternatives with / between them. A category, a
        /// colour, an alternative or a negation names no single item. A required tag beside a category is still just one item tag.
        /// </summary>
        public static string? SingleItemTag(IEnumerable<string> tagSets)
        {
            string? found = null;
            foreach (var tagSet in tagSets)
            {
                string? setTag = null;
                foreach (var clause in (tagSet ?? "").Split(','))
                {
                    var tags = clause.Split('/').Select(tag => tag.Trim()).Where(tag => tag.Length > 0).ToList();
                    // one of several tags in a clause is an alternative, so the item is not fixed
                    if (tags.Count != 1)
                        return null;

                    var tag = tags[0];
                    // an excluded tag says what the item is not, not what it is
                    if (tag.StartsWith('!'))
                        return null;
                    if (!NamesOneItem(tag))
                        continue;
                    if (setTag is not null && !setTag.Equals(tag, StringComparison.OrdinalIgnoreCase))
                        return null;
                    setTag = tag;
                }

                // a list with no item tag can take other things, so it fixes no crop
                if (setTag is null)
                    return null;
                if (found is not null && !found.Equals(setTag, StringComparison.OrdinalIgnoreCase))
                    return null;
                found = setTag;
            }
            return found;
        }

        private static bool NamesOneItem(string tag) =>
            tag.StartsWith("id_", StringComparison.OrdinalIgnoreCase) ||
            tag.StartsWith("crop_", StringComparison.OrdinalIgnoreCase) ||
            (tag.StartsWith("item_", StringComparison.OrdinalIgnoreCase) &&
             !tag.StartsWith("item_type_", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>The lines about special orders, for the journal and for an item's tooltip. This file has no game dependencies so the tests can compile it.</summary>
    public static class OrderNotes
    {
        /// <summary>
        /// The lines added to the end of an order's description in the journal. An order with several objectives that ask for items
        /// names each one in front of what is carried for it, and a line that would be the same for two objectives is said once.
        /// </summary>
        public static List<string> Journal(IEnumerable<OrderWant> wants, ModConfig config, IHintSource source, IHintTexts text)
        {
            var lines = new List<string>();
            if (!config.ShowHints)
                return lines;

            var items = wants.Where(w => w.Kind != OrderWantKind.Slay).ToList();
            var several = items.Count > 1;
            var said = new HashSet<string>(StringComparer.Ordinal);
            void Say(string line)
            {
                if (said.Add(line))
                    lines.Add(line);
            }

            // what is carried for each objective first, then how to grow it, then what to do with it
            if (several)
            {
                // the game lists every objective on the page just below, so a line for each one with nothing carried says nothing new
                var counted = items.Where(w => !w.ByValue && w.Left > 0).ToList();
                var carrying = counted.Where(w => w.Carried > 0).ToList();
                foreach (var want in carrying)
                    AddCarry(lines, want, true, text);
                if (counted.Count > 0 && carrying.Count == 0)
                    lines.Add(text.Get("hint.order.carry.none"));
            }
            else
            {
                foreach (var want in items)
                    AddCarry(lines, want, false, text);
            }
            foreach (var want in items)
            {
                if (want.ItemTag is not { Length: > 0 } tag || source.GrowForTag(tag) is not { } grow)
                    continue;
                foreach (var line in HintBuilder.GrowLines(grow, source, text))
                    Say(line);
            }
            foreach (var want in items)
            {
                switch (want.Kind)
                {
                    case OrderWantKind.Deliver:
                        if (want.Left > 1)
                            Say(text.Get("hint.order.onestack"));
                        if (!string.IsNullOrEmpty(want.Npc))
                        {
                            Say(text.Get("hint.order.deliver", ("npc", source.NpcName(want.Npc))));
                            AddWhere(Say, want.Npc, config, source, text);
                        }
                        break;

                    case OrderWantKind.Donate:
                        if (want.Place is not null && source.PlaceName(want.Place) is { Length: > 0 } place)
                            Say(text.Get("hint.order.donate", ("place", place)));
                        break;

                    case OrderWantKind.Ship:
                        Say(text.Get("hint.order.ship"));
                        break;
                }
            }
            return lines;
        }

        /// <summary>The line in the tooltip of an item an order wants.</summary>
        public static string? Tooltip(OrderWant want, IHintSource source, IHintTexts text) => want.Kind switch
        {
            OrderWantKind.Deliver when !string.IsNullOrEmpty(want.Npc) => text.Get("note.order.deliver", ("order", want.Order), ("npc", source.NpcName(want.Npc))),
            OrderWantKind.Donate => text.Get("note.order.donate", ("order", want.Order)),
            OrderWantKind.Ship => text.Get("note.order.ship", ("order", want.Order)),
            _ => null,
        };

        private static void AddCarry(List<string> lines, OrderWant want, bool named, IHintTexts text)
        {
            // an objective counted in gold has no number of items to compare with
            if (want.ByValue || want.Left <= 0)
                return;
            if (named && want.Text.Length > 0)
                lines.Add(text.Get("hint.order.carry.each", ("what", want.Text.TrimEnd('.', ' ')), ("count", want.Carried.ToString())));
            else
                lines.Add(text.Get("hint.order.carry", ("count", want.Carried.ToString()), ("left", want.Left.ToString())));
        }

        private static void AddWhere(Action<string> say, string npc, ModConfig config, IHintSource source, IHintTexts text)
        {
            if (!config.ShowWhereIsNpc)
                return;
            if (source.IsHere(npc))
            {
                say(text.Get("hint.here", ("npc", source.NpcName(npc))));
                return;
            }
            if (source.WhereIs(npc) is { Length: > 0 } place)
                say(text.Get("hint.where", ("npc", source.NpcName(npc)), ("place", place)));
        }
    }
}
