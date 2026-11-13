using System.Text.RegularExpressions;

namespace Questie
{
    /// <summary>Turns an internal name into something readable when the game's data gives no display name. This file has no game dependencies so the tests can compile it.</summary>
    public static class NameText
    {
        // a break at a lower case letter followed by a capital, and after a run of capitals such as RSV before Heaps
        private static readonly Regex WordBreaks = new("(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", RegexOptions.Compiled);

        /// <summary>"IslandSouthEast" as "Island South East", "Author.CentralStation_GiftShop" as "Central Station Gift Shop", "RSVHeapsStore" as "RSV Heaps Store" and "Custom_Claire_WarpRoom" as "Custom Claire Warp Room".</summary>
        public static string Humanize(string internalName) => WordBreaks.Replace(WithoutAuthor(internalName), " ").Replace('_', ' ');

        /// <summary>A mod writes its ids as "Author.ModName_Thing". The author is not part of what the player calls it.</summary>
        private static string WithoutAuthor(string internalName)
        {
            var dot = internalName.IndexOf('.');
            return dot > 0 && dot < internalName.Length - 1 && internalName.IndexOfAny(new[] { ' ', '_' }, 0, dot) < 0 ? internalName[(dot + 1)..] : internalName;
        }
    }
}
