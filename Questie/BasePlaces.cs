namespace Questie
{
    /// <summary>
    /// The places of the base game. A list of places where a fish bites or a plant is foraged is cut to a few, and with an expansion
    /// installed the places in alphabetical order are mostly the expansion's, so the base game's places come first. This file has no
    /// game dependencies so the tests can compile it.
    /// </summary>
    public static class BasePlaces
    {
        private static readonly HashSet<string> Names = new(StringComparer.Ordinal)
        {
            "Farm", "FarmCave", "BusStop", "Town", "Beach", "Mountain", "Forest", "Backwoods", "Woods", "Desert", "Sewer", "BugLand",
            "WitchSwamp", "UndergroundMine", "Mine", "Railroad", "Submarine", "Club", "IslandSouth", "IslandSouthEast", "IslandSouthEastCave",
            "IslandEast", "IslandWest", "IslandNorth", "IslandNorthCave1", "IslandFarmCave", "IslandHut", "Caldera",
        };

        /// <summary>Whether the internal name is one of the base game's places.</summary>
        public static bool Contains(string internalName) => Names.Contains(internalName);
    }
}
