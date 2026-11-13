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
        public string? Place;
        public string NpcName(string npc) => npc;
        public string ItemName(string qualifiedItemId) => qualifiedItemId;
        public int Carried(string qualifiedItemId) => Carry;
        public int InChests(string qualifiedItemId) => 0;
        public bool IsHere(string npc) => false;
        public string? WhereIs(string npc) => Place;
        public string? PlaceName(string internalName) => internalName;
        public FishInfo? Fish(string qualifiedItemId) => null;
        public GrowInfo? Grow(string qualifiedItemId) => null;
        public GrowInfo? GrowForTag(string tag) => null;
        public ForageInfo? Forage(string qualifiedItemId) => null;
        public DropInfo? Drop(string qualifiedItemId) => null;
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
    public void EveryStoryQuestIsWellFormed()
    {
        foreach (var id in StoryQuests.Ids)
        {
            Assert.True(StoryQuests.TryGet(id, out var quest));
            if (quest.ItemId is null && quest.Area is null)
                continue;
            Assert.True(quest.ItemId is not null ^ quest.Area is not null, id);
            if (quest.ItemId is not null)
                Assert.StartsWith("(O)", quest.ItemId);
            else
                Assert.True(quest.Level > 0 && quest.Area is "mines" or "skull", id);
        }
    }

    [Fact]
    public void EveryStoryQuestHasItsTextInTheTranslations()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Questie", "i18n", "default.json");
        var keys = System.Text.Json.JsonDocument.Parse(File.ReadAllText(path)).RootElement.EnumerateObject().Select(p => p.Name).ToHashSet();
        foreach (var id in StoryQuests.Ids)
        {
            StoryQuests.TryGet(id, out var quest);
            if (quest.ItemId is not null && quest.Chain is null)
                Assert.Contains("hint.bring." + id, keys);
            else if (quest.ItemId is null && quest.Area is null)
                Assert.Contains("hint.story." + id, keys);
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
    public void AWordsOnlyStoryQuestShowsItsLine()
    {
        var lines = Build(new QuestFact("31", FactKind.Story, null, null, null, 0, 1, None));
        Assert.Equal(new[] { "hint.story.31" }, lines);
    }

    [Fact]
    public void TheGiantStumpAsksForAHundredHardwood()
    {
        Assert.True(StoryQuests.TryGet("134", out var quest));
        Assert.Equal("(O)709", quest.ItemId);
        Assert.Equal(100, quest.Count);
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

    [Fact]
    public void ThePiratesWifeTradesOneKeepsakeForTheNext()
    {
        Assert.True(StoryQuests.TryGet("130", out var quest));
        Assert.NotNull(quest.Chain);
        Assert.Equal(7, quest.Chain!.Count);
        Assert.Equal("(O)864", quest.Chain[0].ItemId);
        Assert.Equal("Kent", quest.Chain[0].Npc);
        Assert.Equal("(O)865", quest.Chain[0].Gives);
        Assert.Equal("(O)870", quest.Chain[^1].ItemId);
        Assert.Equal("Birdie", quest.Chain[^1].Npc);
        Assert.Null(quest.Chain[^1].Gives);
        // every trade asks for what the one before it gave
        for (var i = 1; i < quest.Chain.Count; i++)
            Assert.Equal(quest.Chain[i - 1].Gives, quest.Chain[i].ItemId);
    }

    [Fact]
    public void ATradeSaysWhoWantsTheItemAndWhatComesBack()
    {
        var lines = Build(new QuestFact("130", FactKind.Bring, "Kent", "(O)864", null, 0, 1, None, null, null, "(O)865", true), new Source { Carry = 1 });
        Assert.Equal(new[] { "hint.carry(count=1,total=1,item=(O)864)", "hint.trade(item=(O)864,npc=Kent,next=(O)865)" }, lines);
    }

    [Fact]
    public void TheLastTradeSaysToTakeTheKeepsakeBack()
    {
        var lines = Build(new QuestFact("130", FactKind.Bring, "Birdie", "(O)870", null, 0, 1, None, null, null, null, true));
        Assert.Equal(new[] { "hint.carry(count=0,total=1,item=(O)870)", "hint.trade.finish(item=(O)870,npc=Birdie)" }, lines);
    }

    [Fact]
    public void ATradeAlsoSaysWhereThePersonIs()
    {
        var lines = Build(new QuestFact("130", FactKind.Bring, "Gus", "(O)865", null, 0, 1, None, null, null, "(O)866", true), new Source { Carry = 1, Place = "The Saloon" });
        Assert.Contains("hint.where(npc=Gus,place=The Saloon)", lines);
    }
}
