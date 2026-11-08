using StardewModdingAPI;
using StardewValley;

namespace Questie
{
    /// <summary>Answers the hint builder's questions from the game and turns its text keys into the player's language.</summary>
    internal sealed class GameHintSource : IHintSource, IHintTexts
    {
        private readonly ITranslationHelper _i18n;
        private readonly FishLookup _fish;
        private readonly RecipeLookup _recipes;

        public GameHintSource(ITranslationHelper i18n, FishLookup fish, RecipeLookup recipes)
        {
            _i18n = i18n;
            _fish = fish;
            _recipes = recipes;
        }

        // ---------------------------------------------------------------- words

        public string Get(string key, params (string Name, string Value)[] tokens)
        {
            if (tokens.Length == 0)
                return _i18n.Get(key);
            var values = new Dictionary<string, string>(tokens.Length);
            foreach (var (name, value) in tokens)
                values[name] = value;
            return _i18n.Get(key, values);
        }

        // ---------------------------------------------------------------- the game

        public string NpcName(string npc) => NPC.GetDisplayName(npc);

        public string ItemName(string qualifiedItemId) => ItemRegistry.GetDataOrErrorItem(qualifiedItemId).DisplayName;

        public int Carried(string qualifiedItemId) => Game1.player.Items.CountId(qualifiedItemId);

        public bool IsHere(string npc) => Game1.getCharacterFromName(npc) is { } person && ReferenceEquals(person.currentLocation, Game1.currentLocation);

        public string? WhereIs(string npc) => Game1.getCharacterFromName(npc)?.currentLocation is { } place ? PlaceDisplay(place) : null;

        public string? PlaceName(string internalName) => Game1.getLocationFromName(internalName) is { } place ? PlaceDisplay(place) : null;

        public FishInfo? Fish(string qualifiedItemId) => _fish.Find(qualifiedItemId);

        public RecipeInfo? Recipe(string qualifiedItemId) => _recipes.Find(qualifiedItemId);

        private static string? PlaceDisplay(GameLocation place)
        {
            var name = place.GetDisplayName();
            return string.IsNullOrWhiteSpace(name) ? place.Name : name;
        }
    }
}
