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
        private readonly ShopLookup _shops;
        private readonly ChestLookup _chests;

        public GameHintSource(ITranslationHelper i18n, FishLookup fish, RecipeLookup recipes, ShopLookup shops, ChestLookup chests)
        {
            _i18n = i18n;
            _fish = fish;
            _recipes = recipes;
            _shops = shops;
            _chests = chests;
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

        // Names carry a marker so the quest log can put the portrait or the item picture in front of them. Anything else strips it.
        public string NpcName(string npc) => IconText.Mark(IconKind.Npc, npc, NPC.GetDisplayName(npc));

        public string ItemName(string qualifiedItemId) => IconText.Mark(IconKind.Item, qualifiedItemId, ItemRegistry.GetDataOrErrorItem(qualifiedItemId).DisplayName);

        public int Carried(string qualifiedItemId) => Game1.player.Items.CountId(qualifiedItemId);

        public int InChests(string qualifiedItemId) => _chests.Count(qualifiedItemId);

        public bool IsHere(string npc) => Game1.getCharacterFromName(npc) is { } person && ReferenceEquals(person.currentLocation, Game1.currentLocation);

        public string? WhereIs(string npc) => Game1.getCharacterFromName(npc)?.currentLocation is { } place ? PlaceDisplay(place) : null;

        public string? PlaceName(string internalName) => Game1.getLocationFromName(internalName) is { } place ? PlaceDisplay(place) : null;

        public FishInfo? Fish(string qualifiedItemId) => _fish.Find(qualifiedItemId);

        public RecipeInfo? Recipe(string qualifiedItemId) => _recipes.Find(qualifiedItemId);

        public int ClockNow => Game1.timeOfDay;

        public bool RainingNow => Game1.currentLocation is { } place && Game1.IsRainingHere(place);

        public IReadOnlyList<ShopOffer> ShopsFor(string qualifiedItemId) => _shops.Find(qualifiedItemId);

        private static string? PlaceDisplay(GameLocation place)
        {
            var name = place.GetDisplayName();
            return string.IsNullOrWhiteSpace(name) ? place.Name : name;
        }
    }
}
