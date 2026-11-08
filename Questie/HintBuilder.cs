namespace Questie
{
    /// <summary>Gives the builder its words. Keys are the ones in i18n/default.json, and tokens fill the {{name}} places in the text.</summary>
    public interface IHintTexts
    {
        string Get(string key, params (string Name, string Value)[] tokens);
    }

    /// <summary>Where, when and in what weather a fish bites.</summary>
    /// <param name="Places">Display names of the places it can be caught, each with its seasons when only some apply.</param>
    /// <param name="Everywhere">Whether it can be caught almost anywhere, which the game's default fishing list says.</param>
    /// <param name="Time">The times of day as text, or null when it bites all day.</param>
    /// <param name="Weather">"sunny", "rainy" or "both".</param>
    /// <param name="TrapWater">For a fish that is caught in a crab pot, "freshwater" or "ocean". Null for a fish that is caught with a rod.</param>
    /// <param name="Windows">The clock windows the fish bites in, used to say whether it bites now. Null or empty means all day.</param>
    /// <param name="Seasons">The seasons it bites in, as 0 spring to 3 winter. Null when it bites in every season.</param>
    public sealed record FishInfo(IReadOnlyList<string> Places, bool Everywhere, string? Time, string Weather, string? TrapWater = null, IReadOnlyList<(int Start, int End)>? Windows = null, IReadOnlyList<int>? Seasons = null);

    /// <summary>What it takes to have a building built.</summary>
    /// <param name="Builder">The person who builds it, or null when the data names none.</param>
    /// <param name="Gold">The gold it costs, 0 or less when it is free.</param>
    /// <param name="Materials">The items it takes, as qualified ids with how many of each.</param>
    public sealed record BuildingInfo(string? Builder, int Gold, IReadOnlyList<(string ItemId, int Amount)> Materials);

    /// <summary>A shop that sells an item. The price is 0 or less when the shop data does not state one. Sometimes is true when the shop only stocks it under a condition, such as a season.</summary>
    public sealed record ShopOffer(string Seller, int Price, bool Sometimes = false);

    /// <summary>Why a recipe is not known yet.</summary>
    public enum UnlockKind
    {
        /// <summary>Learned from a villager at a number of hearts.</summary>
        Friendship,

        /// <summary>Learned at a skill level.</summary>
        Skill,

        /// <summary>Learned some other way, such as from a shop, a letter or a show.</summary>
        Other,
    }

    /// <summary>How a recipe is learned.</summary>
    /// <param name="Npc">For Friendship, the villager's display name.</param>
    /// <param name="Skill">For Skill, the skill's display name.</param>
    /// <param name="Amount">Hearts or the skill level.</param>
    public sealed record RecipeUnlock(UnlockKind Kind, string? Npc, string? Skill, int Amount);

    /// <summary>How an item is made.</summary>
    /// <param name="Cooking">True for a cooking recipe, false for a crafting recipe.</param>
    /// <param name="Known">Whether the farmer already knows the recipe.</param>
    /// <param name="Ingredients">Each ingredient as text, such as "Copper Bar x2".</param>
    /// <param name="Unlock">When the recipe is not known, how to learn it, if the game says.</param>
    public sealed record RecipeInfo(bool Cooking, bool Known, IReadOnlyList<string> Ingredients, RecipeUnlock? Unlock = null);

    /// <summary>What the builder asks the game. Kept as an interface so the lines can be tested without the game.</summary>
    public interface IHintSource
    {
        string NpcName(string npc);
        string ItemName(string qualifiedItemId);
        int Carried(string qualifiedItemId);

        /// <summary>How many of the item lie in chests and the fridge, not counting what the farmer carries.</summary>
        int InChests(string qualifiedItemId);

        /// <summary>Whether the person is in the same place as the farmer right now.</summary>
        bool IsHere(string npc);

        /// <summary>The display name of the place a person is in right now, or null when that is not known.</summary>
        string? WhereIs(string npc);

        /// <summary>The display name of a place, given its internal name, or null.</summary>
        string? PlaceName(string internalName);

        FishInfo? Fish(string qualifiedItemId);

        /// <summary>The time of day on the game clock, such as 1830. Past 2400 counts as the small hours.</summary>
        int ClockNow { get; }

        /// <summary>Whether it is raining where the farmer is.</summary>
        bool RainingNow { get; }

        /// <summary>The cost and builder of a building type, or null when the type is not known.</summary>
        BuildingInfo? Building(string buildingType);

        /// <summary>How deep the farmer has been, in the mines (up to the bottom at 120) or in the Skull Cavern (counted from its first level).</summary>
        int DeepestLevel(string area);

        /// <summary>The season it is now, as 0 spring to 3 winter.</summary>
        int SeasonNow { get; }

        /// <summary>The name of a season, as 0 spring to 3 winter, in the player's language.</summary>
        string SeasonName(int season);

        /// <summary>The shops that sell an item for gold, best known first.</summary>
        IReadOnlyList<ShopOffer> ShopsFor(string qualifiedItemId);
        RecipeInfo? Recipe(string qualifiedItemId);
    }

    /// <summary>Builds the lines added to the end of a quest's description in the quest log.</summary>
    public static class HintBuilder
    {
        /// <summary>The most names listed for a Socialize quest before the rest are counted.</summary>
        public const int MaxNames = 8;

        public static List<string> Build(QuestFact fact, ModConfig config, IHintSource source, IHintTexts text)
        {
            var lines = new List<string>();
            if (!config.ShowHints)
                return lines;

            switch (fact.Kind)
            {
                case FactKind.Deliver:
                    AddCarry(lines, fact, source, text);
                    AddShops(lines, fact, config, source, text);
                    AddRecipe(lines, fact, config, source, text);
                    AddWhere(lines, fact.Npc, config, source, text);
                    break;

                case FactKind.Slay:
                    AddProgress(lines, "hint.slain", fact, text);
                    AddHandIn(lines, fact, config, source, text);
                    break;

                case FactKind.Fish:
                    AddProgress(lines, "hint.caught", fact, text);
                    if (fact.CountDone)
                        AddHandIn(lines, fact, config, source, text);
                    else
                        AddFish(lines, fact, config, source, text);
                    break;

                case FactKind.Gather:
                    AddProgress(lines, "hint.collected", fact, text);
                    if (fact.CountDone)
                        AddHandIn(lines, fact, config, source, text);
                    else
                    {
                        AddCarry(lines, fact, source, text);
                        AddShops(lines, fact, config, source, text);
                        AddRecipe(lines, fact, config, source, text);
                    }
                    break;

                case FactKind.Socialize:
                    AddProgress(lines, "hint.greeted", fact, text);
                    AddNames(lines, fact, source, text);
                    break;

                case FactKind.LostItem:
                    if (fact.CountDone)
                        AddHandIn(lines, fact, config, source, text);
                    else if (fact.Place is not null && source.PlaceName(fact.Place) is { Length: > 0 } place)
                        lines.Add(text.Get("hint.lost", ("place", place)));
                    break;

                case FactKind.Craft:
                    AddRecipe(lines, fact, config, source, text);
                    break;

                case FactKind.Bring:
                    AddCarry(lines, fact, source, text);
                    AddFish(lines, fact, config, source, text);
                    AddShops(lines, fact, config, source, text);
                    AddRecipe(lines, fact, config, source, text);
                    lines.Add(text.Get("hint.bring." + fact.Id));
                    break;

                case FactKind.Harvest:
                    AddCarry(lines, fact, source, text);
                    AddShops(lines, fact, config, source, text);
                    break;

                case FactKind.Build:
                    AddBuilding(lines, fact, source, text);
                    break;

                case FactKind.Depth:
                    if (fact.Place is "mines" or "skull")
                        lines.Add(text.Get("hint.depth." + fact.Place, ("have", fact.Have.ToString()), ("need", fact.Need.ToString())));
                    break;

                case FactKind.Go:
                    if (fact.Place is not null && source.PlaceName(fact.Place) is { Length: > 0 } where)
                        lines.Add(text.Get("hint.go", ("place", where)));
                    break;
            }
            return lines;
        }

        private static void AddBuilding(List<string> lines, QuestFact fact, IHintSource source, IHintTexts text)
        {
            if (fact.Place is null || source.Building(fact.Place) is not { } building)
                return;

            var cost = new List<string>();
            if (building.Gold > 0)
                cost.Add(text.Get("hint.build.gold", ("gold", building.Gold.ToString())));
            foreach (var (itemId, amount) in building.Materials)
                cost.Add(amount + " " + source.ItemName(itemId));
            if (cost.Count == 0)
                return;

            var list = JoinNames(cost, text);
            lines.Add(building.Builder is { Length: > 0 } builder
                ? text.Get("hint.build.by", ("builder", source.NpcName(builder)), ("cost", list))
                : text.Get("hint.build", ("cost", list)));
        }

        private static void AddProgress(List<string> lines, string key, QuestFact fact, IHintTexts text)
        {
            if (fact.Need > 1)
                lines.Add(text.Get(key, ("done", fact.Have.ToString()), ("total", fact.Need.ToString())));
        }

        private static void AddCarry(List<string> lines, QuestFact fact, IHintSource source, IHintTexts text)
        {
            if (fact.ItemId is null)
                return;
            var count = source.Carried(fact.ItemId).ToString();
            var total = Math.Max(1, fact.Need).ToString();
            var item = source.ItemName(fact.ItemId);
            var chests = source.InChests(fact.ItemId);
            lines.Add(chests > 0
                ? text.Get("hint.carry.chests", ("count", count), ("chests", chests.ToString()), ("total", total), ("item", item))
                : text.Get("hint.carry", ("count", count), ("total", total), ("item", item)));
        }

        private static void AddHandIn(List<string> lines, QuestFact fact, ModConfig config, IHintSource source, IHintTexts text)
        {
            // the person only matters once the counted part is done, so before that there is nothing to go back to
            if (string.IsNullOrEmpty(fact.Npc) || fact.Npc == "null" || !fact.CountDone)
                return;
            lines.Add(text.Get("hint.return", ("npc", source.NpcName(fact.Npc))));
            AddWhere(lines, fact.Npc, config, source, text);
        }

        private static void AddWhere(List<string> lines, string? npc, ModConfig config, IHintSource source, IHintTexts text)
        {
            if (!config.ShowWhereIsNpc || string.IsNullOrEmpty(npc) || npc == "null")
                return;
            if (source.IsHere(npc))
            {
                lines.Add(text.Get("hint.here", ("npc", source.NpcName(npc))));
                return;
            }
            var place = source.WhereIs(npc);
            if (!string.IsNullOrEmpty(place))
                lines.Add(text.Get("hint.where", ("npc", source.NpcName(npc)), ("place", place)));
        }

        private static void AddFish(List<string> lines, QuestFact fact, ModConfig config, IHintSource source, IHintTexts text)
        {
            if (!config.ShowFishInfo || fact.ItemId is null || source.Fish(fact.ItemId) is not { } fish)
                return;
            if (fish.TrapWater is not null)
                lines.Add(text.Get("hint.fish.trap", ("water", fish.TrapWater)));
            else if (fish.Everywhere && fish.Places.Count == 0)
                lines.Add(text.Get("hint.fish.everywhere"));
            else if (fish.Places.Count > 0)
                lines.Add(text.Get("hint.fish.places", ("places", string.Join(", ", fish.Places))));
            if (fish.TrapWater is null && fish.Seasons is { Count: > 0 } seasons)
                lines.Add(text.Get("hint.fish.seasons", ("seasons", JoinNames(seasons.Select(source.SeasonName).ToList(), text))));
            if (!string.IsNullOrEmpty(fish.Time))
                lines.Add(text.Get("hint.fish.time", ("times", fish.Time)));
            if (fish.Weather is "sunny" or "rainy")
                lines.Add(text.Get(fish.Weather == "sunny" ? "hint.fish.sunny" : "hint.fish.rainy"));

            // Whether it bites right now is only worth a line for a fish that has a time or a weather it cares about.
            var picky = !string.IsNullOrEmpty(fish.Time) || fish.Weather is "sunny" or "rainy";
            if (fish.TrapWater is null && fish.Seasons is { Count: > 0 } open && !open.Contains(source.SeasonNow))
            {
                // out of season, so the time and the weather do not matter yet
                lines.Add(text.Get("hint.fish.noseason"));
            }
            else if (fish.TrapWater is null && picky)
            {
                var timeOk = FishTimes.IsOpen(fish.Windows ?? Array.Empty<(int, int)>(), source.ClockNow);
                var weatherOk = FishTimes.WeatherSuits(fish.Weather, source.RainingNow);
                lines.Add(text.Get(timeOk && weatherOk ? "hint.fish.now" : !timeOk ? "hint.fish.notime" : "hint.fish.noweather"));
            }
        }

        /// <summary>Names as a list in words, "spring", "spring and summer", "spring, summer and fall".</summary>
        private static string JoinNames(IReadOnlyList<string> names, IHintTexts text) =>
            names.Count <= 1 ? string.Concat(names) : string.Join(", ", names.Take(names.Count - 1)) + text.Get("hint.and") + names[^1];

        /// <summary>The most shops named for one item.</summary>
        public const int MaxShops = 3;

        private static string Describe(ShopOffer offer, IHintTexts text)
        {
            var name = offer.Price > 0 ? $"{offer.Seller} ({offer.Price}g)" : offer.Seller;
            return offer.Sometimes ? text.Get("hint.sold.sometimes", ("shop", name)) : name;
        }

        private static void AddShops(List<string> lines, QuestFact fact, ModConfig config, IHintSource source, IHintTexts text)
        {
            if (!config.ShowShops || fact.ItemId is null)
                return;
            var offers = source.ShopsFor(fact.ItemId);
            if (offers.Count == 0)
                return;
            var names = offers.Take(MaxShops).Select(o => Describe(o, text));
            lines.Add(text.Get("hint.sold", ("shops", string.Join(", ", names))));
        }

        private static void AddRecipe(List<string> lines, QuestFact fact, ModConfig config, IHintSource source, IHintTexts text)
        {
            if (!config.ShowRecipes || fact.ItemId is null || source.Recipe(fact.ItemId) is not { } recipe)
                return;
            var kind = recipe.Cooking ? "cook" : "craft";
            if (recipe.Known)
            {
                lines.Add(text.Get($"hint.{kind}.known", ("ingredients", string.Join(", ", recipe.Ingredients))));
                return;
            }

            lines.Add(text.Get($"hint.{kind}.unknown"));
            if (recipe.Unlock is { } unlock)
            {
                lines.Add(unlock.Kind switch
                {
                    UnlockKind.Friendship => text.Get("hint.unlock.friendship", ("hearts", unlock.Amount.ToString()), ("npc", unlock.Npc ?? "")),
                    UnlockKind.Skill => text.Get("hint.unlock.skill", ("skill", unlock.Skill ?? ""), ("level", unlock.Amount.ToString())),
                    _ => text.Get("hint.unlock.other"),
                });
            }
        }

        private static void AddNames(List<string> lines, QuestFact fact, IHintSource source, IHintTexts text)
        {
            if (fact.Remaining.Count == 0)
                return;
            var names = fact.Remaining.Take(MaxNames).Select(source.NpcName).ToList();
            var list = string.Join(", ", names);
            if (fact.Remaining.Count > MaxNames)
                list += " " + text.Get("hint.more", ("count", (fact.Remaining.Count - MaxNames).ToString()));
            lines.Add(text.Get("hint.left", ("names", list)));
        }
    }
}
