using StardewValley;
using StardewValley.Quests;

namespace Questie
{
    /// <summary>Copies what the mod needs out of the game's quest objects into <see cref="QuestFact"/> records.</summary>
    internal static class QuestReader
    {
        private static readonly string[] None = Array.Empty<string>();

        /// <summary>The id of the quest that asks for any slime. The game counts every slime-like monster for it.</summary>
        private const string SlimeQuestId = "15";

        /// <summary>The Skull Cavern's levels count on from the mines' last level, so its first level is this plus one in the deepest level the farmer has reached.</summary>
        private const int SkullCavernStart = 120;

        private const string SlimeKinds = "Slime|Jelly|Sludge";

        /// <summary>Replaces the list with a fact for each unfinished quest of a kind the mod knows.</summary>
        public static void Read(Farmer player, List<QuestFact> into)
        {
            into.Clear();
            var log = player.questLog;
            for (var i = 0; i < log.Count; i++)
            {
                if (ToFact(log[i]) is { } fact)
                    into.Add(fact);
            }
        }

        /// <summary>The fact for one quest, or null when it is finished or not a kind the mod knows.</summary>
        public static QuestFact? ToFact(Quest quest) => Build(quest) is { } fact ? fact with { Title = quest.questTitle } : null;

        private static QuestFact? Build(Quest quest)
        {
            if (quest.completed.Value)
                return null;
            var id = quest.id.Value ?? "";

            switch (quest)
            {
                case ItemDeliveryQuest delivery:
                    return new QuestFact(id, FactKind.Deliver, Clean(delivery.target.Value), Qualify(delivery.ItemId.Value), null, 0, Math.Max(1, delivery.number.Value), None);

                case SlayMonsterQuest slay:
                    var kinds = id == SlimeQuestId ? SlimeKinds : Clean(slay.monsterName.Value);
                    return new QuestFact(id, FactKind.Slay, Clean(slay.target.Value), null, kinds, slay.numberKilled.Value, slay.numberToKill.Value, None);

                case FishingQuest fishing:
                    return new QuestFact(id, FactKind.Fish, Clean(fishing.target.Value), Qualify(fishing.ItemId.Value), null, fishing.numberFished.Value, fishing.numberToFish.Value, None);

                case ResourceCollectionQuest resource:
                    return new QuestFact(id, FactKind.Gather, Clean(resource.target.Value), Qualify(resource.ItemId.Value), null, resource.numberCollected.Value, resource.number.Value, None);

                case SocializeQuest socialize:
                    var left = new List<string>(socialize.whoToGreet);
                    return new QuestFact(id, FactKind.Socialize, null, null, null, Math.Max(0, socialize.total.Value - left.Count), socialize.total.Value, left);

                case LostItemQuest lost:
                    return new QuestFact(id, FactKind.LostItem, Clean(lost.npcName.Value), Qualify(lost.ItemId.Value), null, lost.itemFound.Value ? 1 : 0, 1, None, lost.itemFound.Value ? null : Clean(lost.locationOfItem.Value));

                case SecretLostItemQuest secret:
                    return new QuestFact(id, FactKind.LostItem, Clean(secret.npcName.Value), Qualify(secret.ItemId.Value), null, secret.itemFound.Value ? 1 : 0, 1, None);

                case CraftingQuest crafting:
                    return new QuestFact(id, FactKind.Craft, null, Qualify(crafting.ItemId.Value), null, 0, 1, None);

                case ItemHarvestQuest harvest:
                    // the item can also be a whole category, written as a negative number, which has no picture or name to show
                    var harvested = Qualify(harvest.ItemId.Value);
                    return new QuestFact(id, FactKind.Harvest, null, harvested is { Length: > 0 } && !harvested.StartsWith('-') ? harvested : null, null, 0, Math.Max(1, harvest.Number.Value), None);

                case HaveBuildingQuest building:
                    return new QuestFact(id, FactKind.Build, null, null, null, 0, 1, None, Clean(building.buildingType.Value));

                case GoSomewhereQuest go:
                    return new QuestFact(id, FactKind.Go, null, null, null, 0, 1, None, Clean(go.whereToGo.Value));

                default:
                    // a story quest of the plain kind, known by its id
                    if (quest.GetType() == typeof(Quest) && StoryQuests.TryGet(id, out var story))
                    {
                        if (story.ItemId is not null)
                            return new QuestFact(id, FactKind.Bring, null, story.ItemId, null, 0, story.Count, None);
                        var level = Game1.player.deepestMineLevel;
                        var reached = story.Area == "skull" ? Math.Max(0, level - SkullCavernStart) : Math.Min(level, SkullCavernStart);
                        return new QuestFact(id, FactKind.Depth, null, null, null, Math.Min(reached, story.Level), story.Level, None, story.Area);
                    }
                    return null;
            }
        }

        /// <summary>The game writes "null" in a text field to mean nothing.</summary>
        private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) || value == "null" ? null : value;

        private static string? Qualify(string? itemId) => string.IsNullOrWhiteSpace(itemId) ? null : ItemRegistry.QualifyItemId(itemId) ?? itemId;
    }
}
