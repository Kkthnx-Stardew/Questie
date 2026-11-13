using System.Collections.Generic;

namespace Questie
{
    /// <summary>
    /// A table of the extra drops the game adds in monster code rather than in Data/Monsters, so an item can name the
    /// monster that drops it. The game has no data field for these, so each entry names the monster, the item and the
    /// chance on one kill.
    /// </summary>
    internal static class MonsterExtraDrops
    {
        /// <summary>One item a monster can drop in addition to the drops in its data row.</summary>
        /// <param name="Monster">The monster's data id or display name.</param>
        /// <param name="ItemId">The qualified id of the item.</param>
        /// <param name="Chance">The chance on one kill, from 0 to 1.</param>
        internal sealed record Drop(string Monster, string ItemId, double Chance);

        private static readonly Drop[] Known =
        {
            // Ghost.getExtraDropItems: only while the Wizard's special order is active and before its letter is received.
            // The item is only asked for then, so the hint is still the useful one when it appears.
            new("Ghost", "(O)875", 0.095),
            new("Carbon Ghost", "(O)875", 0.095),
            new("Putrid Ghost", "(O)875", 0.095),

            // GreenSlime.getExtraDropItems: a prismatic slime returns the jelly instead of its colour drops.
            new("Prismatic Slime", "(O)876", 1),

            // Serpent.getExtraDropItems and Mummy.getExtraDropItems.
            new("Serpent", "(O)485", 0.002),
            new("Royal Serpent", "(O)485", 0.002),
            new("Mummy", "(O)485", 0.002),

            // Skeleton.getExtraDropItems.
            new("Skeleton", "(W)5", 0.04),
            new("Skeleton Mage", "(W)5", 0.04),
            new("Skeleton Warrior", "(W)5", 0.04),

            // DinoMonster.getExtraDropItems: an egg, or one of three bones when that roll fails.
            new("Pepper Rex", "(O)107", 0.10),
            new("Pepper Rex", "(O)580", 0.30),
            new("Pepper Rex", "(O)583", 0.30),
            new("Pepper Rex", "(O)584", 0.30),

            // GreenSlime.getExtraDropItems for a tiger slime: each branch is tried after the one before it fails.
            new("Tiger Slime", "(O)831", 0.10),
            new("Tiger Slime", "(O)829", 0.09),
            new("Tiger Slime", "(O)833", 0.0162),
            new("Tiger Slime", "(O)835", 0.0047628),

            // Bat.getExtraDropItems for a haunted skull: one of the eleven results of its rare table.
            new("Haunted Skull", "(O)279", 0.0129909),

            // RockGolem.getExtraDropItems for an iridium golem.
            new("Iridium Golem", "(O)386", 0.20),
            new("Iridium Golem", "(O)527", 0.001),
        };

        private static readonly Dictionary<string, Drop[]> ByMonster = BuildTable();

        /// <summary>Every extra drop, for merging into the item-to-monsters index.</summary>
        public static IReadOnlyList<Drop> All => Known;

        /// <summary>The extra drops of a monster, or an empty list when it has none.</summary>
        public static IReadOnlyList<Drop> For(string? monsterId, string? displayName)
        {
            var name = displayName?.Trim();
            if (!string.IsNullOrEmpty(name) && ByMonster.TryGetValue(name, out var byName))
                return byName;

            var id = monsterId?.Trim();
            if (!string.IsNullOrEmpty(id) && ByMonster.TryGetValue(id, out var byId))
                return byId;

            return Array.Empty<Drop>();
        }

        private static Dictionary<string, Drop[]> BuildTable()
        {
            var table = new Dictionary<string, Drop[]>(StringComparer.OrdinalIgnoreCase);
            foreach (var drop in Known)
            {
                if (table.TryGetValue(drop.Monster, out var previous))
                {
                    var grown = new Drop[previous.Length + 1];
                    previous.CopyTo(grown, 0);
                    grown[^1] = drop;
                    table[drop.Monster] = grown;
                }
                else
                {
                    table[drop.Monster] = new[] { drop };
                }
            }
            return table;
        }
    }
}
