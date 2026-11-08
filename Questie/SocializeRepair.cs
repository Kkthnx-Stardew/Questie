namespace Questie
{
    /// <summary>The changes a Socialize quest needs to match the villagers who exist now.</summary>
    /// <param name="Remove">Villagers in the quest who no longer exist as sociable people.</param>
    /// <param name="Add">Sociable villagers who are not in the quest and have not been met.</param>
    public sealed record SocializeChanges(IReadOnlyList<string> Remove, IReadOnlyList<string> Add)
    {
        public bool Any => Remove.Count > 0 || Add.Count > 0;

        /// <summary>How much the quest's total changes by: each villager added counts, each one removed no longer does.</summary>
        public int TotalChange => Add.Count - Remove.Count;
    }

    /// <summary>
    /// A Socialize quest such as Introductions is written down with the villagers who exist when it starts. When a mod later
    /// removes one of them, nobody can be greeted any more and the quest can never be finished, and when a mod adds one, the quest
    /// never asks for them. This works out what to change.
    /// </summary>
    public static class SocializeRepair
    {
        /// <param name="toGreet">The villagers the quest still lists.</param>
        /// <param name="sociable">Every villager who counts for a meeting quest, by the game's own rule.</param>
        /// <param name="needGreeting">The sociable villagers the game puts on the list of people to greet. Others are known from the start.</param>
        /// <param name="alreadyMet">Whether the farmer has already met a villager.</param>
        public static SocializeChanges Plan(IReadOnlyCollection<string> toGreet, IReadOnlySet<string> sociable, IEnumerable<string> needGreeting, Func<string, bool> alreadyMet)
        {
            var remove = toGreet.Where(name => !sociable.Contains(name)).ToList();
            var listed = new HashSet<string>(toGreet, StringComparer.Ordinal);
            var add = needGreeting.Where(name => !listed.Contains(name) && !alreadyMet(name)).Distinct(StringComparer.Ordinal).ToList();
            return new SocializeChanges(remove, add);
        }
    }
}
