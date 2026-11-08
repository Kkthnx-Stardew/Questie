namespace Questie
{
    /// <summary>What a story quest of the game's plain kind asks for, when its data says so only in words.</summary>
    /// <param name="ItemId">The qualified item to bring, or null for a quest about depth.</param>
    /// <param name="Count">How many of the item.</param>
    /// <param name="Area">For a quest about depth, "mines" or "skull".</param>
    /// <param name="Level">For a quest about depth, the level to reach.</param>
    public sealed record StoryQuest(string? ItemId, int Count, string? Area, int Level);

    /// <summary>
    /// The game's story quests of the plain kind, such as "Bring a Cave Carrot into Marnie's shop", hold no item in their data, only
    /// the words of their objective, so the items and levels are listed here, each read from the quest's own objective text in the
    /// game's data. A quest is only matched by its id, and only when it is of the plain quest class. This file has no game
    /// dependencies so the tests can compile it.
    /// </summary>
    public static class StoryQuests
    {
        private static readonly Dictionary<string, StoryQuest> Table = new(StringComparer.Ordinal)
        {
            ["2"] = new("(O)394", 1, null, 0),   // The Mysterious Qi, "Leave a rainbow shell in the box at the train platform"
            ["3"] = new("(O)284", 10, null, 0),  // The Mysterious Qi, "Place 10 beets inside Mayor Lewis' fridge"
            ["10"] = new("(O)378", 1, null, 0),  // Copper Ore, "Bring a piece of copper ore to Clint's blacksmith shop"
            ["21"] = new("(O)78", 1, null, 0),   // Marnie's Request, "Bring a Cave Carrot into Marnie's shop"
            ["22"] = new("(O)136", 1, null, 0),  // Fish Casserole, "Enter Jodi's house with a largemouth bass at 7:00 PM"
            ["29"] = new("(O)724", 1, null, 0),  // Strange Note, "Bring may-pal serrup to the seecrit wuds"
            ["127"] = new("(O)220", 1, null, 0), // Haley's Cake-Walk, "Enter Town on a sunny morning with a chocolate cake"

            ["14"] = new(null, 0, "mines", 5),   // Explore The Mine, "Reach level 5 in the mines"
            ["17"] = new(null, 0, "mines", 40),  // Deeper In The Mine, "Reach level 40 in the mines"
            ["18"] = new(null, 0, "mines", 120), // To The Bottom?, "Reach the bottom of the mine"
            ["20"] = new(null, 0, "skull", 25),  // Qi's Challenge, "Reach level 25 in the Skull Cavern"
            ["30"] = new(null, 0, "skull", 100), // Cryptic Note, "Reach level 100 in the Skull Cavern"
        };

        public static bool TryGet(string id, out StoryQuest quest) => Table.TryGetValue(id, out quest!);

        /// <summary>The ids the table knows, for the tests.</summary>
        public static IEnumerable<string> Ids => Table.Keys;
    }
}
