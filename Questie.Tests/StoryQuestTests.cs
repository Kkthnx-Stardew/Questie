namespace Questie.Tests;

public class StoryQuestTests
{
    private sealed class Texts : IHintTexts
    {
        public string Get(string key, params (string Name, string Value)[] tokens) =>
            key + (tokens.Length == 0 ? "" : "(" + string.Join(",", tokens.Select(t => $"{t.Name}={t.Value}")) + ")");
    }

    private sealed class Source : IHintSource
    {
        public BuildingInfo? BuildingData;
        public int Carry;
        public string NpcName(string npc) => npc;
        public string ItemName(string qualifiedItemId) => qualifiedItemId;
        public int Carried(string qualifiedItemId) => Carry;
        public int InChests(string qualifiedItemId) => 0;
        public bool IsHere(string npc) => false;
        public string? WhereIs(string npc) => null;
        public string? PlaceName(string internalName) => internalName;
        public FishInfo? Fish(string qualifiedItemId) => null;
        public RecipeInfo? Recipe(string qualifiedItemId) => null;
        public int ClockNow => 1200;
        public bool RainingNow => false;
        public BuildingInfo? Building(string buildingType) => BuildingData;
        public int DeepestLevel(string area) => 0;
        public int SeasonNow => 0;
        public string SeasonName(int season) => "s" + season;
        public IReadOnlyList<ShopOffer> ShopsFor(string qualifiedItemId) => Array.Empty<ShopOffer>();
    }

    private static readonly string[] None = Array.Empty<string>();

    private static List<string> Build(QuestFact fact, Source? source = null) => HintBuilder.Build(fact, new ModConfig(), source ?? new Source(), new Texts());

    [Fact]
    public void EveryStoryQuestHasAnItemOrALevelAndABringTextWhenItNeedsOne()
    {
        foreach (var id in StoryQuests.Ids)
        {
            Assert.True(StoryQuests.TryGet(id, out var quest));
            Assert.True(quest.ItemId is not null ^ quest.Area is not null, id);
            if (quest.ItemId is not null)
                Assert.StartsWith("(O)", quest.ItemId);
            else
                Assert.True(quest.Level > 0 && quest.Area is "mines" or "skull", id);
        }
    }

    [Fact]
    public void EveryBringQuestHasItsTextInTheTranslations()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Questie", "i18n", "default.json");
        var keys = System.Text.Json.JsonDocument.Parse(File.ReadAllText(path)).RootElement.EnumerateObject().Select(p => p.Name).ToHashSet();
        foreach (var id in StoryQuests.Ids)
        {
            StoryQuests.TryGet(id, out var quest);
            if (quest.ItemId is not null)
                Assert.Contains("hint.bring." + id, keys);
        }
    }

    [Fact]
    public void AnUnknownQuestIsNotAStoryQuest()
    {
        Assert.False(StoryQuests.TryGet("99999", out _));
    }

    [Fact]
    public void ABringQuestShowsWhatYouCarryAndWhereToTakeIt()
    {
        var lines = Build(new QuestFact("21", FactKind.Bring, null, "(O)78", null, 0, 1, None), new Source { Carry = 1 });
        Assert.Equal(new[] { "hint.carry(count=1,total=1,item=(O)78)", "hint.bring.21" }, lines);
    }

    [Fact]
    public void ADepthQuestSaysHowFarYouHaveGone()
    {
        var lines = Build(new QuestFact("14", FactKind.Depth, null, null, null, 3, 5, None, "mines"));
        Assert.Equal(new[] { "hint.depth.mines(have=3,need=5)" }, lines);
    }

    [Fact]
    public void ABuildQuestSaysWhoBuildsItAndWhatItCosts()
    {
        var source = new Source { BuildingData = new BuildingInfo("Robin", 4000, new[] { ("(O)388", 100), ("(O)390", 50) }) };
        var lines = Build(new QuestFact("7", FactKind.Build, null, null, null, 0, 1, None, "Coop"), source);
        Assert.Equal(new[] { "hint.build.by(builder=Robin,cost=hint.build.gold(gold=4000), 100 (O)388hint.and50 (O)390)" }, lines);
    }

    [Fact]
    public void AFreeBuildingWithNoBuilderSaysNothing()
    {
        var source = new Source { BuildingData = new BuildingInfo(null, 0, Array.Empty<(string, int)>()) };
        Assert.Empty(Build(new QuestFact("7", FactKind.Build, null, null, null, 0, 1, None, "Coop"), source));
    }

    [Fact]
    public void AHarvestQuestShowsWhatYouCarry()
    {
        var lines = Build(new QuestFact("6", FactKind.Harvest, null, "(O)24", null, 0, 1, None), new Source { Carry = 0 });
        Assert.Equal(new[] { "hint.carry(count=0,total=1,item=(O)24)" }, lines);
    }

    [Fact]
    public void ABringItemIsWantedInTheBackpack()
    {
        Assert.True(ItemNeeds.Wants(new QuestFact("21", FactKind.Bring, null, "(O)78", null, 0, 1, None)));
        Assert.False(ItemNeeds.Wants(new QuestFact("6", FactKind.Harvest, null, "(O)24", null, 0, 1, None)));
    }
}
