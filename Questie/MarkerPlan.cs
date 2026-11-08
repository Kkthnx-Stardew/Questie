namespace Questie
{
    /// <summary>What a marker over a person or monster means, which decides its colour.</summary>
    public enum MarkKind
    {
        None = 0,

        /// <summary>Someone to give something to, or go back to, to finish a quest.</summary>
        HandIn = 1,

        /// <summary>Someone a meeting quest still needs you to greet.</summary>
        Greet = 2,
    }

    /// <summary>A marker over a person: what it means, and for a delivery the qualified id of the item to show beside it.</summary>
    public readonly record struct Mark(MarkKind Kind, string? ItemId);

    /// <summary>
    /// Who and what to mark right now. Built from the quests each time they or the backpack change, and read every frame by the
    /// drawing code, so the per-frame work is a lookup and nothing is worked out again.
    /// </summary>
    public sealed class MarkerPlan
    {
        private readonly Dictionary<string, Mark> _npcs = new(StringComparer.Ordinal);
        private readonly List<string> _monsters = new();

        /// <summary>Internal names of the people to mark.</summary>
        public IReadOnlyCollection<string> Npcs => _npcs.Keys;

        /// <summary>Name parts of the monsters to mark. A monster whose name contains one of them is marked.</summary>
        public IReadOnlyList<string> Monsters => _monsters;

        public bool IsEmpty => _npcs.Count == 0 && _monsters.Count == 0;

        public bool HasMonsters => _monsters.Count > 0;

        public bool HasNpcs => _npcs.Count > 0;

        public MarkKind NpcMark(string name) => _npcs.TryGetValue(name, out var mark) ? mark.Kind : MarkKind.None;

        /// <summary>The marker for a person, with the item to show beside it for a delivery.</summary>
        public bool TryGetMark(string name, out Mark mark) => _npcs.TryGetValue(name, out mark);

        public bool MarksNpc(string name) => _npcs.ContainsKey(name);

        public bool MarksMonster(string name)
        {
            for (var i = 0; i < _monsters.Count; i++)
            {
                if (name.Contains(_monsters[i], StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        /// <summary>Replaces the plan with the one the given quests and settings ask for.</summary>
        /// <param name="carried">How many of an item (a qualified id) the farmer carries.</param>
        public void Rebuild(IReadOnlyList<QuestFact> facts, ModConfig config, Func<string, int> carried)
        {
            _npcs.Clear();
            _monsters.Clear();
            if (!config.EnableMarkers)
                return;

            foreach (var fact in facts)
            {
                switch (fact.Kind)
                {
                    case FactKind.Deliver:
                        if (config.MarkDeliveries && HoldsEnough(fact, config, carried, Math.Max(1, fact.Need)))
                            Add(fact.Npc, MarkKind.HandIn, fact.ItemId);
                        break;

                    case FactKind.Slay:
                        if (fact.CountDone)
                            AddTurnIn(fact, config);
                        else if (config.MarkMonsters && !string.IsNullOrEmpty(fact.Monster))
                        {
                            // one quest can name several kinds, separated by | (the slime quest counts every slime)
                            foreach (var kind in fact.Monster.Split('|', StringSplitOptions.RemoveEmptyEntries))
                            {
                                if (!_monsters.Contains(kind))
                                    _monsters.Add(kind);
                            }
                        }
                        break;

                    case FactKind.Fish:
                    case FactKind.Gather:
                        if (fact.CountDone)
                            AddTurnIn(fact, config);
                        break;

                    case FactKind.LostItem:
                        // the person only needs visiting once the item has been found, and while it is in the backpack
                        if (fact.CountDone && config.MarkTurnIn && HoldsEnough(fact, config, carried, 1))
                            Add(fact.Npc, MarkKind.HandIn, fact.ItemId);
                        break;

                    case FactKind.Socialize:
                        if (config.MarkSocialize)
                        {
                            foreach (var name in fact.Remaining)
                                Add(name, MarkKind.Greet);
                        }
                        break;
                }
            }
        }

        private static bool HoldsEnough(QuestFact fact, ModConfig config, Func<string, int> carried, int count) =>
            fact.Npc is not null && (!config.OnlyWhenCarried || (fact.ItemId is not null && carried(fact.ItemId) >= count));

        private void AddTurnIn(QuestFact fact, ModConfig config)
        {
            if (config.MarkTurnIn)
                Add(fact.Npc, MarkKind.HandIn, null);
        }

        /// <summary>Adds a person. A hand-in outranks a greeting when the same person is both.</summary>
        private void Add(string? npc, MarkKind kind, string? itemId = null)
        {
            if (string.IsNullOrEmpty(npc) || npc == "null")
                return;
            if (_npcs.TryGetValue(npc, out var existing) && existing.Kind == MarkKind.HandIn && (kind != MarkKind.HandIn || existing.ItemId is not null))
                return; // a hand-in already marks them, and the first delivery's item is the one shown
            _npcs[npc] = new Mark(kind, itemId);
        }
    }
}
