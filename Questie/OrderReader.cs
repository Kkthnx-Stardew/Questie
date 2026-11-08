using StardewValley;
using StardewValley.SpecialOrders;
using StardewValley.SpecialOrders.Objectives;

namespace Questie
{
    /// <summary>An order objective together with the game's own rule for which items fit it.</summary>
    internal sealed class GameOrderWant : OrderWant
    {
        /// <summary>Whether an item fits the objective. Always false for a kill objective.</summary>
        public Func<Item, bool> Accepts { get; init; } = _ => false;
    }

    /// <summary>Copies the unfinished objectives of the special orders that ask for items or monsters out of the game.</summary>
    internal static class OrderReader
    {
        /// <summary>Replaces the list with the objectives that are still open, and counts what the farmer carries for each.</summary>
        public static void Read(Farmer player, List<GameOrderWant> into)
        {
            into.Clear();
            if (player.team?.specialOrders is not { } orders)
                return;

            foreach (var order in orders)
            {
                if (order.questState.Value != SpecialOrderStatus.InProgress)
                    continue;

                var key = order.questKey.Value ?? "";
                var name = order.GetName();
                var texts = order.GetObjectiveDescriptions();
                for (var i = 0; i < order.objectives.Count; i++)
                {
                    var objective = order.objectives[i];
                    if (objective.IsComplete() || ToWant(objective, key, name, i < texts.Count ? texts[i] : "") is not { } want)
                        continue;
                    // a donation that is full and only waits for the box to be closed needs nothing more
                    if (want.Need > 0 && want.Left == 0)
                        continue;
                    if (want.Kind != OrderWantKind.Slay)
                        Count(player, want);
                    into.Add(want);
                }
            }
        }

        private static GameOrderWant? ToWant(OrderObjective objective, string key, string name, string text)
        {
            var have = objective.GetCount();
            var need = objective.GetMaxCount();
            switch (objective)
            {
                case DeliverObjective deliver:
                    return new GameOrderWant { OrderKey = key, Order = name, Text = text, Kind = OrderWantKind.Deliver, Npc = Clean(deliver.targetName.Value), Have = have, Need = need, Accepts = Matcher(deliver.acceptableContextTagSets) };

                case DonateObjective donate:
                    return new GameOrderWant { OrderKey = key, Order = name, Text = text, Kind = OrderWantKind.Donate, Place = Clean(donate.GetDropboxLocationName()), Have = have, Need = need, Accepts = donate.IsValidItem };

                case ShipObjective ship:
                    return new GameOrderWant { OrderKey = key, Order = name, Text = text, Kind = OrderWantKind.Ship, Have = have, Need = need, ByValue = ship.useShipmentValue.Value, Accepts = Matcher(ship.acceptableContextTagSets) };

                case SlayObjective slay:
                    return new GameOrderWant { OrderKey = key, Order = name, Text = text, Kind = OrderWantKind.Slay, Monsters = slay.targetNames.Where(n => !string.IsNullOrWhiteSpace(n)).ToList(), Have = have, Need = need };

                default:
                    // collecting counts what is picked up and not what is held, and the rest ask for no item the farmer can carry
                    return null;
            }
        }

        private static void Count(Farmer player, GameOrderWant want)
        {
            foreach (var item in player.Items)
            {
                if (item is null || !want.Accepts(item))
                    continue;
                want.Carried += item.Stack;
                if (item.Stack > want.LargestStack)
                {
                    want.LargestStack = item.Stack;
                    want.HeldItemId = item.QualifiedItemId;
                }
            }
        }

        /// <summary>
        /// The game's rule for the tag lists of a delivery and a shipment. Each list is alternatives to the others, a list is clauses that
        /// all have to hold, and a clause is tags of which one has to match, written with / between them.
        /// </summary>
        private static Func<Item, bool> Matcher(IEnumerable<string> tagSets)
        {
            var sets = tagSets.Select(set => set.Split(',').Select(clause => clause.Split('/')).ToArray()).ToArray();
            return item =>
            {
                if (sets.Length == 0)
                    return false;
                var tags = item.GetContextTags();
                foreach (var clauses in sets)
                {
                    var all = true;
                    foreach (var clause in clauses)
                    {
                        if (!ItemContextTagManager.DoAnyTagsMatch(clause, tags))
                        {
                            all = false;
                            break;
                        }
                    }
                    if (all)
                        return true;
                }
                return false;
            };
        }

        private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) || value == "null" ? null : value;
    }
}
