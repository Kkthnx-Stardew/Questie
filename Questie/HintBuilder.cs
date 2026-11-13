using System.Globalization;

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

    /// <summary>How an item is grown, from the game's crop data.</summary>
    /// <param name="SeedName">The display name of the seed to plant.</param>
    /// <param name="Seasons">The seasons it grows in, as 0 spring to 3 winter. Empty when the data names none.</param>
    /// <param name="Days">The days from planting to the first harvest, 0 when the data does not say.</param>
    /// <param name="RegrowDays">The days between harvests after the first, 0 or less when it does not regrow.</param>
    public sealed record GrowInfo(string SeedName, IReadOnlyList<int> Seasons, int Days, int RegrowDays);

    /// <summary>One place an item can be foraged, with the seasons that apply there.</summary>
    /// <param name="Name">The display name of the place.</param>
    /// <param name="Seasons">The seasons it can be foraged there, as 0 spring to 3 winter. All four when it is found there all year.</param>
    public sealed record ForagePlace(string Name, IReadOnlyList<int> Seasons);

    /// <summary>Where an item can be foraged, from the game's location data.</summary>
    /// <param name="Places">The places it can be foraged in, each with only the seasons that apply there.</param>
    public sealed record ForageInfo(IReadOnlyList<ForagePlace> Places);

    /// <summary>One monster that can drop an item, where it can be found and how often a kill drops at least one.</summary>
    /// <param name="Name">The display name of the monster.</param>
    /// <param name="Chance">The chance on one kill, from 0 to 1.</param>
    /// <param name="Places">The display names of the places it can be found in, empty when they are not known.</param>
    public sealed record MonsterDrop(string Name, double Chance, IReadOnlyList<string> Places);

    /// <summary>Which monsters drop an item, from the game's monster data.</summary>
    /// <param name="Monsters">The monsters that can drop it, each with its chance.</param>
    public sealed record DropInfo(IReadOnlyList<MonsterDrop> Monsters);

    /// <summary>What it takes to have a building built.</summary>
    /// <param name="Builder">The person who builds it, or null when the data names none.</param>
    /// <param name="Gold">The gold it costs, 0 or less when it is free.</param>
    /// <param name="Materials">The items it takes, as qualified ids with how many of each.</param>
    public sealed record BuildingInfo(string? Builder, int Gold, IReadOnlyList<(string ItemId, int Amount)> Materials);

    /// <summary>A shop that sells an item. The price is 0 or less when the shop data does not state one. Sometimes is true when the shop only stocks it under a condition, such as a season.</summary>
    public sealed record ShopOffer(string Seller, int Price, bool Sometimes = false);

    /// <summary>One machine that makes an item out of another item, such as a keg making pale ale out of hops.</summary>
    /// <param name="Machine">The display name of the machine.</param>
    /// <param name="InputItemId">The qualified id of the item that goes in.</param>
    public sealed record MadeWay(string Machine, string InputItemId);

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

        /// <summary>How the item is grown, or null when it is not a crop.</summary>
        GrowInfo? Grow(string qualifiedItemId);

        /// <summary>How the item a tag names is grown, or null when the tag names no crop.</summary>
        GrowInfo? GrowForTag(string tag);

        /// <summary>Where and in which seasons the item can be foraged, or null when it is not forage.</summary>
        ForageInfo? Forage(string qualifiedItemId);

        /// <summary>Which monsters drop the item, or null when none do.</summary>
        DropInfo? Drop(string qualifiedItemId);

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

        /// <summary>The places a kind of monster can be found in, or none when they are not known.</summary>
        IReadOnlyList<string> WhereMonster(string monster) => Array.Empty<string>();

        /// <summary>The machines that make the item out of another item, such as a keg making pale ale out of hops.</summary>
        IReadOnlyList<MadeWay> MadeIn(string qualifiedItemId) => Array.Empty<MadeWay>();
    }

    /// <summary>Builds the lines added to the end of a quest's description in the quest log.</summary>
    public static class HintBuilder
    {
        /// <summary>The most names listed for a Socialize quest before the rest are counted.</summary>
        public const int MaxNames = 6;

        public static List<string> Build(QuestFact fact, ModConfig config, IHintSource source, IHintTexts text)
        {
            var lines = new List<string>();
            if (!config.ShowHints)
                return lines;

            switch (fact.Kind)
            {
                case FactKind.Deliver:
                    AddCarry(lines, fact, source, text);
                    AddGrow(lines, fact, source, text);
                    AddForage(lines, fact, source, text);
                    AddDrop(lines, fact, source, text);
                    AddFish(lines, fact, config, source, text);
                    AddShops(lines, fact, config, source, text);
                    AddRecipe(lines, fact, config, source, text);
                    AddWhere(lines, fact.Npc, config, source, text);
                    break;

                case FactKind.Slay:
                    AddMonsterPlaces(lines, fact, source, text);
                    AddHandIn(lines, fact, config, source, text);
                    break;

                case FactKind.Fish:
                    if (fact.CountDone)
                        AddHandIn(lines, fact, config, source, text);
                    else
                        AddFish(lines, fact, config, source, text);
                    break;

                case FactKind.Gather:
                    if (fact.CountDone)
                        AddHandIn(lines, fact, config, source, text);
                    else
                    {
                        AddCarry(lines, fact, source, text);
                        AddGrow(lines, fact, source, text);
                        AddForage(lines, fact, source, text);
                        AddDrop(lines, fact, source, text);
                        AddFish(lines, fact, config, source, text);
                        AddShops(lines, fact, config, source, text);
                        AddRecipe(lines, fact, config, source, text);
                    }
                    break;

                case FactKind.Socialize:
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
                    if (fact.Trade && fact.ItemId is not null && fact.Npc is not null)
                    {
                        // a quest that passes an item from person to person, so the next trade is the help it needs
                        lines.Add(fact.Gives is not null
                            ? text.Get("hint.trade", ("item", source.ItemName(fact.ItemId)), ("npc", source.NpcName(fact.Npc)), ("next", source.ItemName(fact.Gives)))
                            : text.Get("hint.trade.finish", ("item", source.ItemName(fact.ItemId)), ("npc", source.NpcName(fact.Npc))));
                        AddWhere(lines, fact.Npc, config, source, text);
                        break;
                    }
                    AddGrow(lines, fact, source, text);
                    AddForage(lines, fact, source, text);
                    AddDrop(lines, fact, source, text);
                    AddFish(lines, fact, config, source, text);
                    AddShops(lines, fact, config, source, text);
                    AddRecipe(lines, fact, config, source, text);
                    lines.Add(text.Get("hint.bring." + fact.Id));
                    break;

                case FactKind.Harvest:
                    AddCarry(lines, fact, source, text);
                    AddGrow(lines, fact, source, text);
                    AddForage(lines, fact, source, text);
                    AddDrop(lines, fact, source, text);
                    AddShops(lines, fact, config, source, text);
                    break;

                case FactKind.StoryItem:
                    AddStoryItem(lines, fact, config, source, text);
                    break;

                case FactKind.Story:
                    lines.Add(text.Get("hint.story." + fact.Id));
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

        /// <summary>
        /// An item a plain story quest names only in its objective text. A quest that names one item gets the full help for it, and one
        /// that names several gets one short line for each, so a list such as five fish stays readable.
        /// </summary>
        private static void AddStoryItem(List<string> lines, QuestFact fact, ModConfig config, IHintSource source, IHintTexts text)
        {
            if (fact.ItemId is null)
                return;
            if (fact.Brief)
            {
                AddBriefItem(lines, fact, config, source, text);
                return;
            }
            // the text says which item, not how many, so it says what is carried and never a number that was guessed
            lines.Add(text.Get("hint.story.carry", ("count", source.Carried(fact.ItemId).ToString()), ("item", source.ItemName(fact.ItemId))));
            AddGrow(lines, fact, source, text);
            AddForage(lines, fact, source, text);
            AddDrop(lines, fact, source, text);
            AddFish(lines, fact, config, source, text);
            AddShops(lines, fact, config, source, text);
            AddRecipe(lines, fact, config, source, text);
        }

        /// <summary>One short line for an item of a quest that names several, saying where a fish bites, what to plant, where an item is foraged, which monsters drop it or how another item is got.</summary>
        private static void AddBriefItem(List<string> lines, QuestFact fact, ModConfig config, IHintSource source, IHintTexts text)
        {
            if (fact.ItemId is null)
                return;
            if (config.ShowFishInfo && source.Fish(fact.ItemId) is { } fish)
            {
                var line = fish.TrapWater is not null
                    ? text.Get("hint.story.fish.trap", ("item", source.ItemName(fact.ItemId)), ("water", fish.TrapWater))
                    : fish.Everywhere && fish.Places.Count == 0
                        ? text.Get("hint.story.fish.everywhere", ("item", source.ItemName(fact.ItemId)))
                        : fish.Places.Count > 0
                            ? text.Get("hint.story.fish", ("item", source.ItemName(fact.ItemId)), ("places", string.Join(", ", fish.Places)))
                            : "";
                // the seasons are a second sentence of the same line, so the fish gets one picture and not two
                if (fish.TrapWater is null && fish.Seasons is { Count: > 0 } seasons)
                {
                    var seasonLine = text.Get("hint.fish.seasons", ("seasons", JoinNames(seasons.Select(source.SeasonName).ToList(), text)));
                    line = line.Length > 0 ? line + " " + seasonLine : seasonLine;
                }
                if (line.Length > 0)
                    lines.Add(line);
                return;
            }
            if (source.Grow(fact.ItemId) is not null)
            {
                AddGrow(lines, fact, source, text, brief: true);
                return;
            }
            if (source.Forage(fact.ItemId) is { Places.Count: > 0 })
            {
                AddForage(lines, fact, source, text);
                return;
            }
            if (source.Drop(fact.ItemId) is { Monsters.Count: > 0 })
            {
                AddDrop(lines, fact, source, text);
                return;
            }
            AddShops(lines, fact, config, source, text);
            AddRecipe(lines, fact, config, source, text);
        }

        /// <summary>What to plant and how long it takes, for an item that is a crop.</summary>
        private static void AddGrow(List<string> lines, QuestFact fact, IHintSource source, IHintTexts text, bool brief = false)
        {
            if (fact.ItemId is null || source.Grow(fact.ItemId) is not { } grow)
                return;
            lines.AddRange(GrowLines(grow, source, text, brief));
        }

        /// <summary>What to plant and how long it takes, for a crop. The quest log and the special order journal word these the same way.</summary>
        public static List<string> GrowLines(GrowInfo grow, IHintSource source, IHintTexts text, bool brief = false)
        {
            var lines = new List<string>();
            var seasons = grow.Seasons.Count is > 0 and < 4 ? JoinNames(grow.Seasons.Select(source.SeasonName).ToList(), text) : null;
            var line = seasons is not null
                ? text.Get("hint.grow", ("seed", grow.SeedName), ("seasons", seasons))
                : text.Get("hint.grow.any", ("seed", grow.SeedName));
            if (!brief)
            {
                // what to plant and how long it takes is one thought, so it is one line and not three short ones
                if (grow.Days > 0)
                    line += " " + (grow.Days == 1 ? text.Get("hint.grow.ready.one") : text.Get("hint.grow.ready", ("days", grow.Days.ToString())));
                if (grow.RegrowDays > 0)
                    line += " " + (grow.RegrowDays == 1 ? text.Get("hint.grow.regrow.one") : text.Get("hint.grow.regrow", ("days", grow.RegrowDays.ToString())));
            }
            lines.Add(line);
            return lines;
        }

        /// <summary>The most places named for one foraged item.</summary>
        public const int MaxForagePlaces = 3;

        /// <summary>Where and in which seasons an item can be foraged, with each place keeping its own seasons.</summary>
        private static void AddForage(List<string> lines, QuestFact fact, IHintSource source, IHintTexts text)
        {
            if (fact.ItemId is null || source.Forage(fact.ItemId) is not { Places.Count: > 0 } forage)
                return;

            var shown = forage.Places.Take(MaxForagePlaces).ToList();
            var everyPlaceAllYear = shown.All(place => place.Seasons.Count is 0 or 4);
            var places = shown.Select(place => DescribeForagePlace(place, source, text, everyPlaceAllYear)).ToList();
            var list = forage.Places.Count > MaxForagePlaces
                ? string.Join(", ", places) + " " + text.Get("hint.more", ("count", (forage.Places.Count - MaxForagePlaces).ToString()))
                : JoinNames(places, text);
            lines.Add(everyPlaceAllYear
                ? text.Get("hint.forage.any", ("places", list))
                : text.Get("hint.forage", ("places", list)));
        }

        /// <summary>A place with only the seasons that apply there, or "all year" when the other places give seasons.</summary>
        private static string DescribeForagePlace(ForagePlace place, IHintSource source, IHintTexts text, bool everyPlaceAllYear)
        {
            if (place.Seasons.Count is > 0 and < 4)
            {
                var seasons = JoinNames(place.Seasons.Select(source.SeasonName).ToList(), text);
                return text.Get("hint.forage.place", ("place", place.Name), ("seasons", seasons));
            }
            return everyPlaceAllYear ? place.Name : text.Get("hint.forage.place.any", ("place", place.Name));
        }

        /// <summary>The most monsters named for one dropped item.</summary>
        public const int MaxDropMonsters = 3;

        /// <summary>The most places named for one monster.</summary>
        public const int MaxDropPlaces = 3;

        /// <summary>Which monsters drop an item.</summary>
        private static void AddDrop(List<string> lines, QuestFact fact, IHintSource source, IHintTexts text)
        {
            if (fact.ItemId is null || source.Drop(fact.ItemId) is not { Monsters.Count: > 0 } drop)
                return;

            var monsters = DescribeDrops(drop.Monsters.Take(MaxDropMonsters).ToList(), text);
            var list = drop.Monsters.Count > MaxDropMonsters
                ? string.Join(", ", monsters) + " " + text.Get("hint.more", ("count", (drop.Monsters.Count - MaxDropMonsters).ToString()))
                : JoinNames(monsters, text);
            lines.Add(text.Get("hint.drop", ("monsters", list)));
        }

        /// <summary>
        /// Monsters with the same chance in the same places are named together, "Shadow Brute and Shadow Guy (4% in The Mines)", so
        /// the chance and the places are said once and not for every one of them.
        /// </summary>
        private static List<string> DescribeDrops(IReadOnlyList<MonsterDrop> monsters, IHintTexts text)
        {
            var described = new List<string>();
            var used = new bool[monsters.Count];
            for (var i = 0; i < monsters.Count; i++)
            {
                if (used[i])
                    continue;
                var names = new List<string> { monsters[i].Name };
                for (var j = i + 1; j < monsters.Count; j++)
                {
                    if (!used[j] && monsters[j].Chance == monsters[i].Chance && monsters[j].Places.SequenceEqual(monsters[i].Places))
                    {
                        used[j] = true;
                        names.Add(monsters[j].Name);
                    }
                }
                described.Add(DescribeDrop(monsters[i] with { Name = JoinNames(names, text) }, text));
            }
            return described;
        }

        /// <summary>A monster with its chance as a percentage and where it can be found, such as "Bat (94% in The Mines and Skull Cavern)".</summary>
        private static string DescribeDrop(MonsterDrop monster, IHintTexts text)
        {
            var percent = (Math.Clamp(monster.Chance, 0, 1) * 100).ToString("0.###", CultureInfo.InvariantCulture);
            if (monster.Places.Count == 0)
                return text.Get("hint.drop.monster", ("monster", monster.Name), ("chance", percent));

            var shown = monster.Places.Take(MaxDropPlaces).ToList();
            var places = monster.Places.Count > MaxDropPlaces
                ? string.Join(", ", shown) + " " + text.Get("hint.more", ("count", (monster.Places.Count - MaxDropPlaces).ToString()))
                : JoinNames(shown, text);
            return text.Get("hint.drop.monster.places", ("monster", monster.Name), ("chance", percent), ("places", places));
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
            // where the person is rides on the same line as going back to them, so it is one thing to read and not two
            if (config.ShowWhereIsNpc)
            {
                if (source.IsHere(fact.Npc))
                {
                    lines.Add(text.Get("hint.return.here", ("npc", source.NpcName(fact.Npc))));
                    return;
                }
                var place = source.WhereIs(fact.Npc);
                if (!string.IsNullOrEmpty(place))
                {
                    lines.Add(text.Get("hint.return.where", ("npc", source.NpcName(fact.Npc)), ("place", place)));
                    return;
                }
            }
            lines.Add(text.Get("hint.return", ("npc", source.NpcName(fact.Npc))));
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
            // the hours and the weather are one thing to know, so they share a line
            if (!string.IsNullOrEmpty(fish.Time))
            {
                lines.Add(fish.Weather is "sunny" or "rainy"
                    ? text.Get(fish.Weather == "sunny" ? "hint.fish.time.sunny" : "hint.fish.time.rainy", ("times", fish.Time))
                    : text.Get("hint.fish.time", ("times", fish.Time)));
            }
            else if (fish.Weather is "sunny" or "rainy")
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

        /// <summary>How many times the cheapest price a shop may ask and still be named.</summary>
        public const int MaxPriceRatio = 10;

        private static string Describe(ShopOffer offer, IHintTexts text)
        {
            var name = offer.Price > 0 ? text.Get("hint.price", ("shop", offer.Seller), ("price", offer.Price.ToString())) : offer.Seller;
            return offer.Sometimes ? text.Get("hint.sold.sometimes", ("shop", name)) : name;
        }

        private static void AddShops(List<string> lines, QuestFact fact, ModConfig config, IHintSource source, IHintTexts text)
        {
            if (!config.ShowShops || fact.ItemId is null)
                return;
            var offers = source.ShopsFor(fact.ItemId);
            // a shop that asks over ten times what the cheapest one does is a way to lose gold and not a place to go
            var cheapest = offers.Where(o => o.Price > 0).Select(o => o.Price).DefaultIfEmpty(0).Min();
            var worthIt = offers.Where(o => cheapest == 0 || o.Price <= 0 || o.Price <= cheapest * MaxPriceRatio).ToList();
            if (worthIt.Count == 0)
                return;
            var names = worthIt.Take(MaxShops).Select(o => Describe(o, text));
            lines.Add(text.Get("hint.sold", ("shops", string.Join(", ", names))));
        }

        /// <summary>How the item is crafted or cooked, and which machine makes it out of what.</summary>
        private static void AddRecipe(List<string> lines, QuestFact fact, ModConfig config, IHintSource source, IHintTexts text)
        {
            AddCrafting(lines, fact, config, source, text);
            AddMade(lines, fact, config, source, text);
        }

        /// <summary>The most machines named for one item.</summary>
        public const int MaxMachines = 2;

        /// <summary>The most things named that go into one machine.</summary>
        public const int MaxInputs = 3;

        /// <summary>Names as a list in words that offers a choice, "Milk", "Milk or Large Milk".</summary>
        private static string JoinOr(IReadOnlyList<string> names, IHintTexts text) =>
            names.Count <= 1 ? string.Concat(names) : string.Join(", ", names.Take(names.Count - 1)) + text.Get("hint.or") + names[^1];

        /// <summary>"Made with the Keg from Hops." for an item a machine makes out of another item.</summary>
        private static void AddMade(List<string> lines, QuestFact fact, ModConfig config, IHintSource source, IHintTexts text)
        {
            if (!config.ShowRecipes || fact.ItemId is null)
                return;
            var ways = source.MadeIn(fact.ItemId);
            if (ways.Count == 0)
                return;
            // a machine that takes several things in is one sentence, and the sentences share a line so two machines are not two bullets
            var sentences = ways.GroupBy(way => way.Machine).Take(MaxMachines)
                .Select(machine => text.Get("hint.made", ("machine", machine.Key), ("item", JoinOr(machine.Take(MaxInputs).Select(way => source.ItemName(way.InputItemId)).ToList(), text))));
            lines.Add(string.Join(" ", sentences));
        }

        /// <summary>Where a kill quest's monsters can be found, until the kills are done.</summary>
        private static void AddMonsterPlaces(List<string> lines, QuestFact fact, IHintSource source, IHintTexts text)
        {
            // a quest for several kinds at once, such as any slime, has no one place
            if (fact.CountDone || string.IsNullOrWhiteSpace(fact.Monster) || fact.Monster.Contains('|'))
                return;
            var places = source.WhereMonster(fact.Monster);
            if (places.Count == 0)
                return;
            var list = places.Count > MaxDropPlaces
                ? string.Join(", ", places.Take(MaxDropPlaces)) + " " + text.Get("hint.more", ("count", (places.Count - MaxDropPlaces).ToString()))
                : JoinNames(places, text);
            lines.Add(text.Get("hint.slay.where", ("places", list)));
        }

        private static void AddCrafting(List<string> lines, QuestFact fact, ModConfig config, IHintSource source, IHintTexts text)
        {
            if (!config.ShowRecipes || fact.ItemId is null || source.Recipe(fact.ItemId) is not { } recipe)
                return;
            var kind = recipe.Cooking ? "cook" : "craft";
            if (recipe.Known)
            {
                lines.Add(text.Get($"hint.{kind}.known", ("ingredients", string.Join(", ", recipe.Ingredients))));
                return;
            }

            // not knowing it and how to learn it are one line, and a way the game does not spell out adds nothing
            var unknown = text.Get($"hint.{kind}.unknown");
            if (recipe.Unlock is { } unlock)
            {
                var how = unlock.Kind switch
                {
                    UnlockKind.Friendship => text.Get("hint.unlock.friendship", ("hearts", unlock.Amount.ToString()), ("npc", unlock.Npc ?? "")),
                    UnlockKind.Skill => text.Get("hint.unlock.skill", ("skill", unlock.Skill ?? ""), ("level", unlock.Amount.ToString())),
                    _ => "",
                };
                if (how.Length > 0)
                    unknown += " " + how;
            }
            lines.Add(unknown);
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
