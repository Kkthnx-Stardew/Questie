using System.Collections.Generic;

namespace Questie
{
    /// <summary>
    /// The places the game's own monsters can be found in. The game does not keep this in its monster data: the mines, the
    /// Skull Cavern, the Quarry Mine, the Volcano Dungeon and the named locations each choose their monsters in code. A
    /// monster added by another mod has no place here, and is shown without one.
    /// </summary>
    internal static class MonsterPlaces
    {
        private static readonly string[] NoPlaces = Array.Empty<string>();

        /// <summary>Data/Monsters ids, plus the English names of variants that share another monster's data row.</summary>
        private static readonly Dictionary<string, string[]> KnownPlaces = new(StringComparer.OrdinalIgnoreCase)
        {
            // slimes
            ["Green Slime"] = Places("The Mines", "Secret Woods", "Skull Cavern", "Quarry Mine", "The Farm", "Slime Hutch"),
            ["Big Slime"] = Places("The Mines", "Skull Cavern", "Quarry Mine", "Slime Hutch"),
            ["BigSlime"] = Places("The Mines", "Skull Cavern", "Quarry Mine", "Slime Hutch"),
            ["Frost Jelly"] = Places("The Mines", "Secret Woods", "The Farm", "Slime Hutch"),
            ["Sludge"] = Places("The Mines", "Skull Cavern", "Slime Hutch"),
            ["Tiger Slime"] = Places("Ginger Island", "Volcano Dungeon"),

            // the mines and their dangerous floors
            ["Bat"] = Places("The Mines", "Skull Cavern"),
            ["Frost Bat"] = Places("The Mines"),
            ["Lava Bat"] = Places("The Mines", "Skull Cavern"),
            ["Iridium Bat"] = Places("Skull Cavern"),
            ["Bug"] = Places("The Mines", "Skull Cavern"),
            ["Fly"] = Places("The Mines", "Mutant Bug Lair"),
            ["Grub"] = Places("The Mines", "Mutant Bug Lair"),
            ["Duggy"] = Places("The Mines"),
            ["Dust Spirit"] = Places("The Mines"),
            ["Rock Crab"] = Places("The Mines"),
            ["Stone Golem"] = Places("The Mines"),
            ["Skeleton"] = Places("The Mines"),
            ["Skeleton Mage"] = Places("The Mines"),
            ["Skeleton Warrior"] = Places("The Mines"),
            ["Ghost"] = Places("The Mines"),
            ["Carbon Ghost"] = Places("Skull Cavern", "The Mines"),
            ["Putrid Ghost"] = Places("The Mines"),
            ["Squid Kid"] = Places("The Mines"),
            ["Blue Squid"] = Places("The Mines"),
            ["Metal Head"] = Places("The Mines"),
            ["Shadow Guy"] = Places("The Mines"),
            ["Shadow Brute"] = Places("The Mines"),
            ["Shadow Shaman"] = Places("The Mines"),
            ["Shadow Sniper"] = Places("The Mines"),
            ["Spider"] = Places("The Mines"),
            ["Spiker"] = Places("The Mines"),
            ["Lava Crab"] = Places("The Mines"),

            // the Skull Cavern
            ["Iridium Crab"] = Places("Skull Cavern"),
            ["Mummy"] = Places("Skull Cavern"),
            ["Pepper Rex"] = Places("Skull Cavern"),
            ["Serpent"] = Places("Skull Cavern"),
            ["Royal Serpent"] = Places("Skull Cavern"),

            // the Volcano Dungeon
            ["Dwarvish Sentry"] = Places("Volcano Dungeon"),
            ["False Magma Cap"] = Places("Volcano Dungeon"),
            ["Hot Head"] = Places("Volcano Dungeon"),
            ["Lava Lurk"] = Places("Volcano Dungeon"),
            ["Magma Duggy"] = Places("Volcano Dungeon"),
            ["Magma Sparker"] = Places("Volcano Dungeon"),
            ["Magma Sprite"] = Places("Volcano Dungeon"),

            // the farm
            ["Wilderness Golem"] = Places("The Farm"),

            // Fixed names used by variants that share another monster's data row. These are checked first, so a
            // Haunted Skull is not listed as an ordinary Bat.
            ["Haunted Skull"] = Places("Quarry Mine", "Skull Cavern", "The Mines"),
            ["Armored Bug"] = Places("Skull Cavern"),
            ["Mutant Fly"] = Places("Mutant Bug Lair"),
            ["Mutant Grub"] = Places("Mutant Bug Lair"),
            ["Copper Slime"] = Places("Quarry Mine"),
            ["Iron Slime"] = Places("Quarry Mine"),
            ["Purple Slime"] = Places("Skull Cavern", "Slime Hutch"),
            ["Red Slime"] = Places("The Mines", "Slime Hutch"),
            ["Blue Slime"] = Places("The Mines", "Secret Woods", "The Farm", "Slime Hutch"),
            ["Prismatic Slime"] = Places("The Mines", "Secret Woods", "Skull Cavern", "Quarry Mine", "The Farm", "Slime Hutch"),
            ["Iridium Golem"] = Places("The Farm"),
            ["Truffle Crab"] = Places("The Farm"),
            ["Stick Bug"] = Places("The Mines"),
            ["Dust Sprite"] = Places("The Mines"),
            ["Cave Fly"] = Places("The Mines"),
        };

        /// <summary>The places a monster can be found in, or none when the game's data does not describe it.</summary>
        public static IReadOnlyList<string> For(string? monsterId, string? displayName)
        {
            var name = displayName?.Trim();
            if (!string.IsNullOrEmpty(name) && KnownPlaces.TryGetValue(name, out var byName))
                return byName;

            var id = monsterId?.Trim();
            if (!string.IsNullOrEmpty(id) && KnownPlaces.TryGetValue(id, out var byId))
                return byId;

            return NoPlaces;
        }

        private static string[] Places(params string[] places) => places;
    }
}
