namespace Questie
{
    /// <summary>The kinds of quest this mod knows how to help with.</summary>
    public enum FactKind
    {
        /// <summary>Bring an item to a person.</summary>
        Deliver,

        /// <summary>Kill monsters of one kind.</summary>
        Slay,

        /// <summary>Catch a number of one fish.</summary>
        Fish,

        /// <summary>Gather a number of one resource.</summary>
        Gather,

        /// <summary>Greet a list of villagers.</summary>
        Socialize,

        /// <summary>Find a lost item and give it back.</summary>
        LostItem,

        /// <summary>Craft one item.</summary>
        Craft,

        /// <summary>Go to a place.</summary>
        Go,
    }

    /// <summary>
    /// What the mod needs to know about one quest, copied out of the game's quest objects so the rules that use it can be tested
    /// without the game. Names are internal names, and item ids are qualified, such as (O)24.
    /// </summary>
    /// <param name="Id">The quest's id. Empty for a daily quest that has none.</param>
    /// <param name="Kind">What the quest asks for.</param>
    /// <param name="Npc">The person to hand in to, or null when the quest has none.</param>
    /// <param name="ItemId">The item involved, or null.</param>
    /// <param name="Monster">The name part a monster must contain, or null.</param>
    /// <param name="Have">How much of the quest is done: caught, killed, gathered or greeted so far. For a lost item, 1 once it is found.</param>
    /// <param name="Need">How much it asks for in all.</param>
    /// <param name="Remaining">For Socialize, the villagers still to greet. Empty otherwise.</param>
    /// <param name="Place">For a lost item that is not found yet, the internal name of the place it lies in, or null.</param>
    /// <param name="Title">The quest's title as the quest log shows it, or null.</param>
    public sealed record QuestFact(
        string Id,
        FactKind Kind,
        string? Npc,
        string? ItemId,
        string? Monster,
        int Have,
        int Need,
        IReadOnlyList<string> Remaining,
        string? Place = null,
        string? Title = null)
    {
        /// <summary>Whether the counted part of the quest is finished, so only the hand-in is left.</summary>
        public bool CountDone => Need > 0 && Have >= Need;
    }
}
