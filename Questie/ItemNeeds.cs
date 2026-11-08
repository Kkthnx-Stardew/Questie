namespace Questie
{
    /// <summary>
    /// Which items a player's quests still want, for the small marker on an item's icon and the line in its tooltip. This file has no
    /// game dependencies so the tests can compile it.
    /// </summary>
    public static class ItemNeeds
    {
        /// <summary>The most quests named in one tooltip.</summary>
        public const int MaxQuests = 2;

        /// <summary>
        /// Whether the quest wants this item to be held or found. A delivery and a found lost item are handed over from the backpack
        /// until they are given. A fish or a resource only matters until enough has been caught or gathered. A crafted item does not
        /// exist until it is made, so it is not marked.
        /// </summary>
        public static bool Wants(QuestFact fact) => fact.ItemId is not null && fact.Kind switch
        {
            FactKind.Deliver or FactKind.Bring => true,
            FactKind.LostItem => fact.Have > 0,
            FactKind.Fish or FactKind.Gather => !fact.CountDone,
            _ => false,
        };

        /// <summary>The set of qualified item ids the quests want, rebuilt when the quests change.</summary>
        public static void Collect(IEnumerable<QuestFact> facts, HashSet<string> into)
        {
            into.Clear();
            foreach (var fact in facts)
            {
                if (Wants(fact))
                    into.Add(fact.ItemId!);
            }
        }

        /// <summary>The tooltip lines for one item, one for each quest that wants it, up to two.</summary>
        public static IReadOnlyList<string> Notes(string qualifiedItemId, IEnumerable<QuestFact> facts, IHintSource source, IHintTexts texts)
        {
            var notes = new List<string>();
            foreach (var fact in facts)
            {
                if (notes.Count >= MaxQuests)
                    break;
                if (fact.ItemId != qualifiedItemId || !Wants(fact))
                    continue;

                var quest = string.IsNullOrWhiteSpace(fact.Title) ? source.ItemName(qualifiedItemId) : fact.Title;
                var npc = fact.Npc is null ? null : source.NpcName(fact.Npc);
                switch (fact.Kind)
                {
                    case FactKind.Deliver when npc is not null:
                        notes.Add(texts.Get("note.deliver", ("quest", quest), ("count", fact.Need.ToString()), ("npc", npc)));
                        break;
                    case FactKind.Bring:
                        notes.Add(texts.Get("note.bring", ("quest", quest), ("count", fact.Need.ToString())));
                        break;
                    case FactKind.LostItem when npc is not null:
                        notes.Add(texts.Get("note.lost", ("quest", quest), ("npc", npc)));
                        break;
                    default:
                        notes.Add(texts.Get("note.progress", ("quest", quest), ("have", fact.Have.ToString()), ("need", fact.Need.ToString())));
                        break;
                }
            }
            return notes;
        }
    }
}
