namespace Questie
{
    /// <summary>One trade of a story quest that passes an item from person to person.</summary>
    /// <param name="ItemId">The qualified item the farmer gives.</param>
    /// <param name="Npc">The internal name of the person to give it to.</param>
    /// <param name="Gives">The qualified item that person gives back, or null when the trade finishes the quest.</param>
    public sealed record StoryStep(string ItemId, string Npc, string? Gives);

    /// <summary>What a story quest of the game's plain kind asks for, when its data says so only in words.</summary>
    /// <param name="ItemId">The qualified item to bring, or null for a quest about depth.</param>
    /// <param name="Count">How many of the item.</param>
    /// <param name="Area">For a quest about depth, "mines" or "skull".</param>
    /// <param name="Level">For a quest about depth, the level to reach.</param>
    public sealed record StoryQuest(string? ItemId, int Count, string? Area, int Level)
    {
        /// <summary>For a quest that trades one item for another, the trades in order, or null for a quest that asks for one item.</summary>
        public IReadOnlyList<StoryStep>? Chain { get; init; }
    }

    /// <summary>
    /// The game's story quests of the plain kind, such as "Bring a Cave Carrot into Marnie's shop", hold no item in their data, only
    /// the words of their objective, so the items and levels are listed here, each read from the quest's own objective text in the
    /// game's data. A quest that needs words alone has neither an item nor a level, and its help is the hint.story.&lt;id&gt; translation.
    /// A quest is only matched by its id, and only when it is of the plain quest class. This file has no game dependencies so the
    /// tests can compile it.
    /// </summary>
    public static class StoryQuests
    {
        private static readonly Dictionary<string, StoryQuest> Table = new(StringComparer.Ordinal)
        {
            ["2"] = new("(O)394", 1, null, 0),   // The Mysterious Qi, "Leave a rainbow shell in the box at the train platform"
            ["3"] = new("(O)284", 10, null, 0),  // The Mysterious Qi, "Place 10 beets inside Mayor Lewis' fridge"
            ["4"] = new("(O)768", 1, null, 0),   // The Mysterious Qi, "Give the sand dragon his final meal"
            ["10"] = new("(O)378", 1, null, 0),  // Copper Ore, "Bring a piece of copper ore to Clint's blacksmith shop"
            ["21"] = new("(O)78", 1, null, 0),   // Marnie's Request, "Bring a Cave Carrot into Marnie's shop"
            ["22"] = new("(O)136", 1, null, 0),  // Fish Casserole, "Enter Jodi's house with a largemouth bass at 7:00 PM"
            ["27"] = new("(O)308", 1, null, 0),  // Goblin Problem, the goblin wants a Void Mayonnaise to let you pass
            ["29"] = new("(O)724", 1, null, 0),  // Strange Note, "Bring may-pal serrup to the seecrit wuds"
            ["127"] = new("(O)220", 1, null, 0), // Haley's Cake-Walk, "Enter Town on a sunny morning with a chocolate cake"
            ["134"] = new("(O)709", 100, null, 0), // The Giant Stump, "Fix up the giant stump" with 100 hardwood
            ["130"] = new("(O)864", 1, null, 0)  // The Pirate's Wife, "Find a keepsake that belonged to Birdie's husband"
            {
                // the quest data names no item, only the keepsake, so the trades are listed from the quest's own solution
                Chain = new[]
                {
                    new StoryStep("(O)864", "Kent", "(O)865"),   // War Memento for Gourmet Tomato Salt
                    new StoryStep("(O)865", "Gus", "(O)866"),    // Gourmet Tomato Salt for Stardew Valley Rose
                    new StoryStep("(O)866", "Sandy", "(O)867"),  // Stardew Valley Rose for Advanced TV Remote
                    new StoryStep("(O)867", "George", "(O)868"), // Advanced TV Remote for Arctic Shard
                    new StoryStep("(O)868", "Wizard", "(O)869"), // Arctic Shard for Wriggling Worm
                    new StoryStep("(O)869", "Willy", "(O)870"),  // Wriggling Worm for Pirate's Locket
                    new StoryStep("(O)870", "Birdie", null),     // Pirate's Locket back to Birdie
                },
            },

            ["14"] = new(null, 0, "mines", 5),   // Explore The Mine, "Reach level 5 in the mines"
            ["17"] = new(null, 0, "mines", 40),  // Deeper In The Mine, "Reach level 40 in the mines"
            ["18"] = new(null, 0, "mines", 120), // To The Bottom?, "Reach the bottom of the mine"
            ["20"] = new(null, 0, "skull", 25),  // Qi's Challenge, "Reach level 25 in the Skull Cavern"
            ["30"] = new(null, 0, "skull", 100), // Cryptic Note, "Reach level 100 in the Skull Cavern"

            // quests helped by words alone, whose hint is the hint.story.<id> translation
            ["5"] = new(null, 0, null, 0),  // The Mysterious Qi, "Inspect the lumber pile beside your house"
            ["13"] = new(null, 0, null, 0), // To The Beach, "Visit the beach south of town before 5:00pm"
            ["19"] = new(null, 0, null, 0), // The Skull Key, "Discover the purpose of the Skull Key"
            ["24"] = new(null, 0, null, 0), // Archaeology, "Donate an artifact or a mineral to the museum"
            ["25"] = new(null, 0, null, 0), // How To Win Friends, "Give someone a gift"
            ["26"] = new(null, 0, null, 0), // "Rat Problem", "Investigate the Community Center"
            ["28"] = new(null, 0, null, 0), // Dark Talisman, "Enter the sewer and ask Krobus about the dark talisman"
            ["31"] = new(null, 0, null, 0), // A Winter Mystery, "Find the shadowy figure"
        };

        public static bool TryGet(string id, out StoryQuest quest) => Table.TryGetValue(id, out quest!);

        /// <summary>The ids the table knows, for the tests.</summary>
        public static IEnumerable<string> Ids => Table.Keys;
    }
}
