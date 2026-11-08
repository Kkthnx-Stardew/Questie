namespace Questie.Tests;

public class HintBuilderTests
{
    /// <summary>Writes the key and its tokens, so a test can see exactly what was asked for without depending on wording.</summary>
    private sealed class Texts : IHintTexts
    {
        public string Get(string key, params (string Name, string Value)[] tokens) =>
            key + (tokens.Length == 0 ? "" : "(" + string.Join(",", tokens.Select(t => $"{t.Name}={t.Value}")) + ")");
    }

    private sealed class Source : IHintSource
    {
        public int CarriedCount;
        public bool Here;
        public string? Place;
        public string? PlaceDisplay;
        public FishInfo? FishData;
        public RecipeInfo? RecipeData;

        public string NpcName(string npc) => npc.ToUpperInvariant();
        public string ItemName(string qualifiedItemId) => "item" + qualifiedItemId;
        public int Carried(string qualifiedItemId) => CarriedCount;
        public int ChestCount;
        public int InChests(string qualifiedItemId) => ChestCount;
        public bool IsHere(string npc) => Here;
        public string? WhereIs(string npc) => Place;
        public string? PlaceName(string internalName) => PlaceDisplay;
        public FishInfo? Fish(string qualifiedItemId) => FishData;
        public RecipeInfo? Recipe(string qualifiedItemId) => RecipeData;
        public int ClockNow { get; set; } = 1200;
        public bool RainingNow { get; set; }
        public List<ShopOffer> Offers = new();
        public IReadOnlyList<ShopOffer> ShopsFor(string qualifiedItemId) => Offers;
    }

    private static readonly string[] None = Array.Empty<string>();

    private static List<string> Build(QuestFact fact, Source? source = null, ModConfig? config = null) =>
        HintBuilder.Build(fact, config ?? new ModConfig(), source ?? new Source(), new Texts());

    [Fact]
    public void StockInChestsIsAddedToTheCarryLine()
    {
        var lines = Build(new QuestFact("1", FactKind.Gather, "Clint", "(O)390", null, 0, 25, None), new Source { CarriedCount = 0, ChestCount = 50 });
        Assert.Contains("hint.carry.chests(count=0,chests=50,total=25,item=item(O)390)", lines);
    }

    [Fact]
    public void NoChestStockKeepsTheShortCarryLine()
    {
        var lines = Build(new QuestFact("1", FactKind.Deliver, "Pierre", "(O)24", null, 0, 2, None), new Source { CarriedCount = 1, ChestCount = 0 });
        Assert.Contains("hint.carry(count=1,total=2,item=item(O)24)", lines);
        Assert.DoesNotContain(lines, l => l.StartsWith("hint.carry.chests"));
    }

    [Fact]
    public void ADeliveryShowsWhatIsCarriedAndWhereThePersonIs()
    {
        var lines = Build(new QuestFact("1", FactKind.Deliver, "Pierre", "(O)24", null, 0, 2, None), new Source { CarriedCount = 1, Place = "Pierre's General Store" });
        Assert.Equal(new[] { "hint.carry(count=1,total=2,item=item(O)24)", "hint.where(npc=PIERRE,place=Pierre's General Store)" }, lines);
    }

    [Fact]
    public void APersonInTheSamePlaceIsSaidToBeHere()
    {
        var lines = Build(new QuestFact("1", FactKind.Deliver, "Pierre", null, null, 0, 1, None), new Source { Here = true, Place = "Town" });
        Assert.Equal(new[] { "hint.here(npc=PIERRE)" }, lines);
    }

    [Fact]
    public void ASlayQuestShowsProgressThenTheReturnLineWhenDone()
    {
        var open = Build(new QuestFact("2", FactKind.Slay, "Lewis", null, "Slime", 2, 5, None));
        Assert.Equal(new[] { "hint.slain(done=2,total=5)" }, open);

        var done = Build(new QuestFact("2", FactKind.Slay, "Lewis", null, "Slime", 5, 5, None), new Source { Place = "Town" });
        Assert.Equal(new[] { "hint.slain(done=5,total=5)", "hint.return(npc=LEWIS)", "hint.where(npc=LEWIS,place=Town)" }, done);
        Assert.DoesNotContain(open, line => line.StartsWith("hint.where"));
    }

    [Fact]
    public void ASingleKillAsksForNoProgressLine()
    {
        Assert.Empty(Build(new QuestFact("2", FactKind.Slay, null, null, "Slime", 0, 1, None)));
    }

    [Fact]
    public void AFishingQuestSaysWhereWhenAndInWhatWeather()
    {
        var fish = new FishInfo(new[] { "Beach", "Ocean" }, false, "6am to 7pm", "rainy");
        var lines = Build(new QuestFact("3", FactKind.Fish, "Willy", "(O)131", null, 1, 3, None), new Source { FishData = fish });
        Assert.Equal(new[] { "hint.caught(done=1,total=3)", "hint.fish.places(places=Beach, Ocean)", "hint.fish.time(times=6am to 7pm)", "hint.fish.rainy", "hint.fish.noweather" }, lines);
    }

    [Fact]
    public void AFishThatBitesAllDayInAnyWeatherSaysOnlyWhere()
    {
        var fish = new FishInfo(new[] { "Beach" }, false, null, "both");
        var lines = Build(new QuestFact("3", FactKind.Fish, "Willy", "(O)131", null, 0, 1, None), new Source { FishData = fish });
        Assert.Equal(new[] { "hint.fish.places(places=Beach)" }, lines);
    }

    [Fact]
    public void AFishThatBitesAlmostAnywhereSaysSo()
    {
        var fish = new FishInfo(Array.Empty<string>(), true, null, "both");
        Assert.Equal(new[] { "hint.fish.everywhere" }, Build(new QuestFact("3", FactKind.Fish, null, "(O)132", null, 0, 1, None), new Source { FishData = fish }));
    }

    [Fact]
    public void ACrabPotFishSaysWhichWater()
    {
        var fish = new FishInfo(Array.Empty<string>(), false, null, "both", "ocean");
        Assert.Equal(new[] { "hint.fish.trap(water=ocean)" }, Build(new QuestFact("3", FactKind.Fish, null, "(O)715", null, 0, 1, None), new Source { FishData = fish }));
    }

    [Fact]
    public void FishInfoCanBeSwitchedOff()
    {
        var fish = new FishInfo(new[] { "Beach" }, false, null, "both");
        var lines = Build(new QuestFact("3", FactKind.Fish, "Willy", "(O)131", null, 1, 3, None), new Source { FishData = fish }, new ModConfig { ShowFishInfo = false });
        Assert.Equal(new[] { "hint.caught(done=1,total=3)" }, lines);
    }

    [Fact]
    public void ADoneFishingQuestSaysToGoBackInsteadOfWhereToFish()
    {
        var fish = new FishInfo(new[] { "Beach" }, false, null, "both");
        var lines = Build(new QuestFact("3", FactKind.Fish, "Willy", "(O)131", null, 3, 3, None), new Source { FishData = fish });
        Assert.Equal(new[] { "hint.caught(done=3,total=3)", "hint.return(npc=WILLY)" }, lines);
    }

    [Fact]
    public void ALostItemSaysWhereItLiesUntilItIsFound()
    {
        var open = new QuestFact("9", FactKind.LostItem, "Linus", "(O)789", null, 0, 1, None, "Mountain");
        Assert.Equal(new[] { "hint.lost(place=The Mountain)" }, Build(open, new Source { PlaceDisplay = "The Mountain" }));

        var found = open with { Have = 1 };
        Assert.Equal(new[] { "hint.return(npc=LINUS)", "hint.where(npc=LINUS,place=Mountain Camp)" }, Build(found, new Source { Place = "Mountain Camp" }));
    }

    [Fact]
    public void RecipesAreShownForAKnownAndAnUnknownRecipe()
    {
        var known = Build(new QuestFact("4", FactKind.Craft, null, "(BC)1", null, 0, 1, None), new Source { RecipeData = new RecipeInfo(false, true, new[] { "Wood x2" }) });
        Assert.Equal(new[] { "hint.craft.known(ingredients=Wood x2)" }, known);

        var unknown = Build(new QuestFact("4", FactKind.Craft, null, "(BC)1", null, 0, 1, None), new Source { RecipeData = new RecipeInfo(true, false, new[] { "Egg x1" }) });
        Assert.Equal(new[] { "hint.cook.unknown" }, unknown);
    }

    [Fact]
    public void AnUnknownRecipeSaysHowItIsLearned()
    {
        var friendship = new RecipeInfo(true, false, None, new RecipeUnlock(UnlockKind.Friendship, "Gus", null, 3));
        var lines = Build(new QuestFact("4", FactKind.Deliver, "Pierre", "(O)220", null, 0, 1, None), new Source { RecipeData = friendship });
        Assert.Contains("hint.cook.unknown", lines);
        Assert.Contains("hint.unlock.friendship(hearts=3,npc=Gus)", lines);

        var skill = new RecipeInfo(false, false, None, new RecipeUnlock(UnlockKind.Skill, null, "Mining", 2));
        Assert.Contains("hint.unlock.skill(skill=Mining,level=2)", Build(new QuestFact("4", FactKind.Craft, null, "(BC)13", null, 0, 1, None), new Source { RecipeData = skill }));

        var other = new RecipeInfo(false, false, None, new RecipeUnlock(UnlockKind.Other, null, null, 0));
        Assert.Contains("hint.unlock.other", Build(new QuestFact("4", FactKind.Craft, null, "(BC)13", null, 0, 1, None), new Source { RecipeData = other }));
    }

    [Fact]
    public void AKnownRecipeNeedsNoUnlockLine()
    {
        var known = new RecipeInfo(false, true, new[] { "Wood x2" }, new RecipeUnlock(UnlockKind.Other, null, null, 0));
        Assert.Equal(new[] { "hint.craft.known(ingredients=Wood x2)" }, Build(new QuestFact("4", FactKind.Craft, null, "(BC)13", null, 0, 1, None), new Source { RecipeData = known }));
    }

    [Fact]
    public void RecipesCanBeSwitchedOff()
    {
        var lines = Build(new QuestFact("4", FactKind.Craft, null, "(BC)1", null, 0, 1, None), new Source { RecipeData = new RecipeInfo(false, true, new[] { "Wood x2" }) }, new ModConfig { ShowRecipes = false });
        Assert.Empty(lines);
    }

    [Fact]
    public void SocializeListsTheNamesLeftAndCountsTheRest()
    {
        var names = Enumerable.Range(1, 11).Select(n => "n" + n).ToList();
        var lines = Build(new QuestFact("5", FactKind.Socialize, null, null, null, 2, 13, names));
        Assert.Equal("hint.greeted(done=2,total=13)", lines[0]);
        Assert.Equal("hint.left(names=N1, N2, N3, N4, N5, N6, N7, N8 hint.more(count=3))", lines[1]);
    }

    [Fact]
    public void WhereIsAPersonCanBeSwitchedOff()
    {
        var lines = Build(new QuestFact("1", FactKind.Deliver, "Pierre", "(O)24", null, 0, 1, None), new Source { Place = "Town" }, new ModConfig { ShowWhereIsNpc = false });
        Assert.Equal(new[] { "hint.carry(count=0,total=1,item=item(O)24)" }, lines);
    }

    [Fact]
    public void NothingIsAddedWhenHintsAreOff()
    {
        var lines = Build(new QuestFact("1", FactKind.Deliver, "Pierre", "(O)24", null, 0, 1, None), new Source { Place = "Town" }, new ModConfig { ShowHints = false });
        Assert.Empty(lines);
    }
}

public class HintBuilderOrderTests
{
    private sealed class Texts : IHintTexts
    {
        public string Get(string key, params (string Name, string Value)[] tokens) => key;
    }

    private sealed class Source : IHintSource
    {
        public string NpcName(string npc) => npc;
        public string ItemName(string qualifiedItemId) => qualifiedItemId;
        public int Carried(string qualifiedItemId) => 0;
        public int InChests(string qualifiedItemId) => 0;
        public bool IsHere(string npc) => false;
        public string? WhereIs(string npc) => "Town";
        public string? PlaceName(string internalName) => internalName;
        public FishInfo? Fish(string qualifiedItemId) => null;
        public RecipeInfo? Recipe(string qualifiedItemId) => null;
        public int ClockNow => 1200;
        public bool RainingNow => false;
        public IReadOnlyList<ShopOffer> ShopsFor(string qualifiedItemId) => Array.Empty<ShopOffer>();
    }

    [Fact]
    public void AnOpenKillQuestDoesNotSayWhereThePersonIs()
    {
        var lines = HintBuilder.Build(new QuestFact("2", FactKind.Slay, "Lewis", null, "Slime", 1, 5, Array.Empty<string>()), new ModConfig(), new Source(), new Texts());
        Assert.Equal(new[] { "hint.slain" }, lines);
    }

    [Fact]
    public void AnOpenGatheringQuestDoesNotSayWhereThePersonIsEither()
    {
        var lines = HintBuilder.Build(new QuestFact("2", FactKind.Gather, "Robin", "(O)388", null, 1, 5, Array.Empty<string>()), new ModConfig(), new Source(), new Texts());
        Assert.DoesNotContain("hint.where", lines);
        Assert.Contains("hint.carry", lines);
    }
}

public class HintBuilderShopAndClockTests
{
    private sealed class Texts : IHintTexts
    {
        public string Get(string key, params (string Name, string Value)[] tokens) =>
            key + (tokens.Length == 0 ? "" : "(" + string.Join(",", tokens.Select(t => $"{t.Name}={t.Value}")) + ")");
    }

    private sealed class Source : IHintSource
    {
        public int Clock = 1200;
        public bool Raining;
        public List<ShopOffer> Offers = new();
        public FishInfo? FishData;

        public string NpcName(string npc) => npc;
        public string ItemName(string qualifiedItemId) => qualifiedItemId;
        public int Carried(string qualifiedItemId) => 0;
        public int InChests(string qualifiedItemId) => 0;
        public bool IsHere(string npc) => false;
        public string? WhereIs(string npc) => null;
        public string? PlaceName(string internalName) => null;
        public FishInfo? Fish(string qualifiedItemId) => FishData;
        public RecipeInfo? Recipe(string qualifiedItemId) => null;
        public int ClockNow => Clock;
        public bool RainingNow => Raining;
        public IReadOnlyList<ShopOffer> ShopsFor(string qualifiedItemId) => Offers;
    }

    private static readonly string[] None = Array.Empty<string>();

    private static List<string> Build(QuestFact fact, Source source, ModConfig? config = null) =>
        HintBuilder.Build(fact, config ?? new ModConfig(), source, new Texts());

    [Fact]
    public void AShopThatSellsTheItemIsNamedWithItsPrice()
    {
        var source = new Source { Offers = { new ShopOffer("Pierre", 120), new ShopOffer("Robin", 0) } };
        var lines = Build(new QuestFact("1", FactKind.Deliver, null, "(O)245", null, 0, 1, None), source);
        Assert.Contains("hint.sold(shops=Pierre (120g), Robin)", lines);
    }

    [Fact]
    public void OnlyTheFirstThreeShopsAreNamed()
    {
        var source = new Source { Offers = { new ShopOffer("A", 1), new ShopOffer("B", 2), new ShopOffer("C", 3), new ShopOffer("D", 4) } };
        var lines = Build(new QuestFact("1", FactKind.Deliver, null, "(O)245", null, 0, 1, None), source);
        Assert.Contains("hint.sold(shops=A (1g), B (2g), C (3g))", lines);
    }

    [Fact]
    public void ShopsAreSkippedWhenSwitchedOffOrAGatheringQuestIsDone()
    {
        var source = new Source { Offers = { new ShopOffer("Pierre", 120) } };
        Assert.DoesNotContain(Build(new QuestFact("1", FactKind.Deliver, null, "(O)245", null, 0, 1, None), source, new ModConfig { ShowShops = false }), l => l.StartsWith("hint.sold"));
        Assert.DoesNotContain(Build(new QuestFact("2", FactKind.Gather, "Robin", "(O)388", null, 25, 25, None), source), l => l.StartsWith("hint.sold"));
    }

    [Fact]
    public void AFishWithATimeSaysWhetherItBitesNow()
    {
        var fish = new FishInfo(new[] { "Beach" }, false, "6am to 7pm", "both", null, new[] { (600, 1900) });
        var fact = new QuestFact("3", FactKind.Fish, "Willy", "(O)131", null, 0, 1, None);
        Assert.Contains("hint.fish.now", Build(fact, new Source { FishData = fish, Clock = 1200 }));
        Assert.Contains("hint.fish.notime", Build(fact, new Source { FishData = fish, Clock = 2100 }));
    }

    [Fact]
    public void AFishThatWantsRainSaysSoWhenItIsDry()
    {
        var fish = new FishInfo(new[] { "Town" }, false, null, "rainy");
        var fact = new QuestFact("3", FactKind.Fish, "Willy", "(O)138", null, 0, 1, None);
        Assert.Contains("hint.fish.noweather", Build(fact, new Source { FishData = fish, Raining = false }));
        Assert.Contains("hint.fish.now", Build(fact, new Source { FishData = fish, Raining = true }));
    }

    [Fact]
    public void AFishWithNoPreferenceAndACrabPotFishGetNoNowLine()
    {
        var plain = new FishInfo(new[] { "Beach" }, false, null, "both");
        var fact = new QuestFact("3", FactKind.Fish, "Willy", "(O)131", null, 0, 1, None);
        Assert.DoesNotContain(Build(fact, new Source { FishData = plain }), l => l.StartsWith("hint.fish.now") || l.StartsWith("hint.fish.no"));

        var trap = new FishInfo(Array.Empty<string>(), false, "6am to 7pm", "rainy", "ocean", new[] { (600, 1900) });
        Assert.DoesNotContain(Build(fact, new Source { FishData = trap, Clock = 2100 }), l => l.StartsWith("hint.fish.no"));
    }
}
