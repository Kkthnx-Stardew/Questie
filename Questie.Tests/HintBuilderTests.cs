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
        public GrowInfo? GrowData;
        public ForageInfo? ForageData;
        public DropInfo? DropData;
        public RecipeInfo? RecipeData;
        public string[] MonsterAt = Array.Empty<string>();
        public List<MadeWay> Made = new();
        public IReadOnlyList<string> WhereMonster(string monster) => MonsterAt;
        public IReadOnlyList<MadeWay> MadeIn(string qualifiedItemId) => Made;

        public string NpcName(string npc) => npc.ToUpperInvariant();
        public string ItemName(string qualifiedItemId) => "item" + qualifiedItemId;
        public int Carried(string qualifiedItemId) => CarriedCount;
        public int ChestCount;
        public int InChests(string qualifiedItemId) => ChestCount;
        public bool IsHere(string npc) => Here;
        public string? WhereIs(string npc) => Place;
        public string? PlaceName(string internalName) => PlaceDisplay;
        public FishInfo? Fish(string qualifiedItemId) => FishData;
        public GrowInfo? Grow(string qualifiedItemId) => GrowData;
        public GrowInfo? GrowForTag(string tag) => null;
        public ForageInfo? Forage(string qualifiedItemId) => ForageData;
        public DropInfo? Drop(string qualifiedItemId) => DropData;
        public RecipeInfo? Recipe(string qualifiedItemId) => RecipeData;
        public int ClockNow { get; set; } = 1200;
        public bool RainingNow { get; set; }
        public BuildingInfo? Building(string buildingType) => null;
        public int DeepestLevel(string area) => 0;
        public int SeasonNow { get; set; } = 1;
        public string SeasonName(int season) => new[] { "spring", "summer", "fall", "winter" }[season];
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
    public void ACropSaysWhatToPlantWhenItGrowsAndWhetherItRegrows()
    {
        var grow = new GrowInfo("Coffee Bean", new[] { 0, 1 }, 10, 2);
        var lines = Build(new QuestFact("q", FactKind.Harvest, null, "(O)433", null, 0, 1, None), new Source { GrowData = grow });
        Assert.Equal(new[]
        {
            "hint.carry(count=0,total=1,item=item(O)433)",
            "hint.grow(seed=Coffee Bean,seasons=springhint.andsummer) hint.grow.ready(days=10) hint.grow.regrow(days=2)",
        }, lines);
    }

    [Fact]
    public void ADeliveryCropSaysWhatToPlant()
    {
        var grow = new GrowInfo("Parsnip Seeds", new[] { 0 }, 4, -1);
        var lines = Build(new QuestFact("q", FactKind.Deliver, "Pierre", "(O)24", null, 0, 1, None), new Source { GrowData = grow });
        Assert.Contains("hint.grow(seed=Parsnip Seeds,seasons=spring) hint.grow.ready(days=4)", lines);
    }

    [Fact]
    public void ACropWithNoSeasonSaysWhatToPlant()
    {
        var grow = new GrowInfo("Parsnip Seeds", Array.Empty<int>(), 4, -1);
        var lines = Build(new QuestFact("q", FactKind.Harvest, null, "(O)24", null, 0, 1, None), new Source { GrowData = grow });
        Assert.Contains("hint.grow.any(seed=Parsnip Seeds) hint.grow.ready(days=4)", lines);
    }

    [Fact]
    public void ACropReadyInOneDaySaysOneDay()
    {
        var grow = new GrowInfo("Parsnip Seeds", new[] { 0 }, 1, 1);
        var lines = Build(new QuestFact("q", FactKind.Harvest, null, "(O)24", null, 0, 1, None), new Source { GrowData = grow });
        Assert.Contains("hint.grow(seed=Parsnip Seeds,seasons=spring) hint.grow.ready.one hint.grow.regrow.one", lines);
    }

    [Fact]
    public void ABriefCropGetsOnlyThePlantingLine()
    {
        var fact = new QuestFact("q", FactKind.StoryItem, null, "(O)24", null, 0, 1, None) { Brief = true };
        var lines = Build(fact, new Source { GrowData = new GrowInfo("Parsnip Seeds", new[] { 0 }, 4, -1) });
        Assert.Equal(new[] { "hint.grow(seed=Parsnip Seeds,seasons=spring)" }, lines);
    }

    [Theory]
    [InlineData(FactKind.Deliver)]
    [InlineData(FactKind.Gather)]
    [InlineData(FactKind.Bring)]
    [InlineData(FactKind.Harvest)]
    [InlineData(FactKind.StoryItem)]
    public void AForagedItemSaysWhereAndInWhichSeasons(FactKind kind)
    {
        var forage = new ForageInfo(new[]
        {
            new ForagePlace("Backwoods", new[] { 0 }),
            new ForagePlace("Cindersap Forest", new[] { 1 }),
        });
        var lines = Build(new QuestFact("q", kind, "Pierre", "(O)18", null, 0, 1, None), new Source { ForageData = forage });
        Assert.Contains("hint.forage(places=hint.forage.place(place=Backwoods,seasons=spring)hint.andhint.forage.place(place=Cindersap Forest,seasons=summer))", lines);
    }

    [Fact]
    public void APlaceKeepsItsOwnSeasonList()
    {
        var forage = new ForageInfo(new[]
        {
            new ForagePlace("Backwoods", new[] { 0, 1 }),
            new ForagePlace("Cindersap Forest", new[] { 2 }),
        });
        var lines = Build(new QuestFact("q", FactKind.Harvest, null, "(O)18", null, 0, 1, None), new Source { ForageData = forage });
        Assert.Contains("hint.forage(places=hint.forage.place(place=Backwoods,seasons=springhint.andsummer)hint.andhint.forage.place(place=Cindersap Forest,seasons=fall))", lines);
    }

    [Fact]
    public void AYearRoundForageSaysWhereOnly()
    {
        var forage = new ForageInfo(new[] { new ForagePlace("The Beach", new[] { 0, 1, 2, 3 }) });
        var lines = Build(new QuestFact("q", FactKind.Harvest, null, "(O)18", null, 0, 1, None), new Source { ForageData = forage });
        Assert.Contains("hint.forage.any(places=The Beach)", lines);
    }

    [Fact]
    public void AnAllYearPlaceIsMarkedAmongSeasonalOnes()
    {
        var forage = new ForageInfo(new[]
        {
            new ForagePlace("The Beach", new[] { 0, 1, 2, 3 }),
            new ForagePlace("Backwoods", new[] { 0 }),
        });
        var lines = Build(new QuestFact("q", FactKind.Harvest, null, "(O)18", null, 0, 1, None), new Source { ForageData = forage });
        Assert.Contains("hint.forage(places=hint.forage.place.any(place=The Beach)hint.andhint.forage.place(place=Backwoods,seasons=spring))", lines);
    }

    [Fact]
    public void ABriefForageGetsOneLine()
    {
        var fact = new QuestFact("q", FactKind.StoryItem, null, "(O)18", null, 0, 1, None) { Brief = true };
        var lines = Build(fact, new Source { ForageData = new ForageInfo(new[] { new ForagePlace("Backwoods", new[] { 0 }) }) });
        Assert.Equal(new[] { "hint.forage(places=hint.forage.place(place=Backwoods,seasons=spring))" }, lines);
    }

    [Fact]
    public void OnlyTheFirstThreeForagePlacesAreNamed()
    {
        var forage = new ForageInfo(new[]
        {
            new ForagePlace("A", new[] { 0 }),
            new ForagePlace("B", new[] { 0 }),
            new ForagePlace("C", new[] { 0 }),
            new ForagePlace("D", new[] { 0 }),
        });
        var lines = Build(new QuestFact("q", FactKind.Deliver, null, "(O)18", null, 0, 1, None), new Source { ForageData = forage });
        Assert.Contains("hint.forage(places=hint.forage.place(place=A,seasons=spring), hint.forage.place(place=B,seasons=spring), hint.forage.place(place=C,seasons=spring) hint.more(count=1))", lines);
    }

    [Theory]
    [InlineData(FactKind.Deliver)]
    [InlineData(FactKind.Gather)]
    [InlineData(FactKind.Bring)]
    [InlineData(FactKind.Harvest)]
    [InlineData(FactKind.StoryItem)]
    public void AMonsterDropSaysWhichMonstersDropIt(FactKind kind)
    {
        var drop = new DropInfo(new[] { new MonsterDrop("Bat", 0.05, None), new MonsterDrop("Frost Bat", 0.1, None) });
        var lines = Build(new QuestFact("q", kind, "Pierre", "(O)767", null, 0, 1, None), new Source { DropData = drop });
        Assert.Contains("hint.drop(monsters=hint.drop.monster(monster=Bat,chance=5)hint.andhint.drop.monster(monster=Frost Bat,chance=10))", lines);
    }

    [Fact]
    public void AMonsterDropNamesThePlacesItCanBeFound()
    {
        var drop = new DropInfo(new[] { new MonsterDrop("Bat", 0.05, new[] { "The Mines", "Skull Cavern" }) });
        var lines = Build(new QuestFact("q", FactKind.Deliver, null, "(O)767", null, 0, 1, None), new Source { DropData = drop });
        Assert.Contains("hint.drop(monsters=hint.drop.monster.places(monster=Bat,chance=5,places=The Mineshint.andSkull Cavern))", lines);
    }

    [Fact]
    public void AMonsterWithNoKnownPlaceStillShowsItsChance()
    {
        var drop = new DropInfo(new[] { new MonsterDrop("Modded Bat", 0.05, None) });
        var lines = Build(new QuestFact("q", FactKind.Deliver, null, "(O)767", null, 0, 1, None), new Source { DropData = drop });
        Assert.Contains("hint.drop(monsters=hint.drop.monster(monster=Modded Bat,chance=5))", lines);
    }

    [Fact]
    public void OnlyTheFirstThreeDropPlacesAreNamed()
    {
        var drop = new DropInfo(new[] { new MonsterDrop("Bat", 0.05, new[] { "A", "B", "C", "D" }) });
        var lines = Build(new QuestFact("q", FactKind.Deliver, null, "(O)767", null, 0, 1, None), new Source { DropData = drop });
        Assert.Contains("hint.drop(monsters=hint.drop.monster.places(monster=Bat,chance=5,places=A, B, C hint.more(count=1)))", lines);
    }

    [Fact]
    public void ABriefMonsterDropGetsOneLine()
    {
        var fact = new QuestFact("q", FactKind.StoryItem, null, "(O)767", null, 0, 1, None) { Brief = true };
        var lines = Build(fact, new Source { DropData = new DropInfo(new[] { new MonsterDrop("Bat", 0.05, None) }) });
        Assert.Equal(new[] { "hint.drop(monsters=hint.drop.monster(monster=Bat,chance=5))" }, lines);
    }

    [Fact]
    public void OnlyTheFirstThreeDroppingMonstersAreNamed()
    {
        var drop = new DropInfo(new[]
        {
            new MonsterDrop("A", 0.05, None),
            new MonsterDrop("B", 0.1, None),
            new MonsterDrop("C", 0.15, None),
            new MonsterDrop("D", 0.2, None),
        });
        var lines = Build(new QuestFact("q", FactKind.Deliver, null, "(O)767", null, 0, 1, None), new Source { DropData = drop });
        Assert.Contains("hint.drop(monsters=hint.drop.monster(monster=A,chance=5), hint.drop.monster(monster=B,chance=10), hint.drop.monster(monster=C,chance=15) hint.more(count=1))", lines);
    }

    [Fact]
    public void ADropChanceWithAPartIsShownAsAPercent()
    {
        var drop = new DropInfo(new[] { new MonsterDrop("Bat", 0.075, None) });
        var lines = Build(new QuestFact("q", FactKind.Deliver, null, "(O)767", null, 0, 1, None), new Source { DropData = drop });
        Assert.Contains("hint.drop(monsters=hint.drop.monster(monster=Bat,chance=7.5))", lines);
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
    public void ASlayQuestSaysNothingOfTheCountBecauseTheGameShowsItThenTheReturnLineWhenDone()
    {
        var open = Build(new QuestFact("2", FactKind.Slay, "Lewis", null, "Slime", 2, 5, None));
        Assert.Empty(open);

        var done = Build(new QuestFact("2", FactKind.Slay, "Lewis", null, "Slime", 5, 5, None), new Source { Place = "Town" });
        Assert.Equal(new[] { "hint.return.where(npc=LEWIS,place=Town)" }, done);
        Assert.DoesNotContain(open, line => line.StartsWith("hint.return"));
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
        Assert.Equal(new[] { "hint.fish.places(places=Beach, Ocean)", "hint.fish.time.rainy(times=6am to 7pm)", "hint.fish.noweather" }, lines);
    }


    [Fact]
    public void MonstersWithTheSameChanceAndPlacesAreNamedTogether()
    {
        var places = new[] { "The Mines" };
        var drop = new DropInfo(new[] { new MonsterDrop("Shadow Brute", 0.04, places), new MonsterDrop("Bat", 0.5, places), new MonsterDrop("Shadow Guy", 0.04, places) });
        var lines = Build(new QuestFact("q", FactKind.Gather, null, "(O)334", null, 0, 5, None), new Source { DropData = drop });
        Assert.Contains("hint.drop(monsters=hint.drop.monster.places(monster=Shadow Brutehint.andShadow Guy,chance=4,places=The Mines)hint.andhint.drop.monster.places(monster=Bat,chance=50,places=The Mines))", lines);
    }

    [Fact]
    public void ADeliveryOfAFishSaysWhereItBites()
    {
        var fish = new FishInfo(new[] { "Beach" }, false, null, "both");
        var lines = Build(new QuestFact("q", FactKind.Deliver, "Gus", "(O)705", null, 0, 1, None), new Source { FishData = fish });
        Assert.Contains("hint.fish.places(places=Beach)", lines);
    }

    [Fact]
    public void AKillQuestSaysWhereTheMonsterLivesUntilTheKillsAreDone()
    {
        var source = new Source { MonsterAt = new[] { "The Mines", "Skull Cavern" } };
        Assert.Equal(new[] { "hint.slay.where(places=The Mineshint.andSkull Cavern)" }, Build(new QuestFact("2", FactKind.Slay, "Lewis", null, "Bat", 0, 5, None), source));

        var done = Build(new QuestFact("2", FactKind.Slay, "Lewis", null, "Bat", 5, 5, None), source);
        Assert.DoesNotContain(done, l => l.StartsWith("hint.slay.where"));
    }

    [Fact]
    public void AKillQuestForSeveralKindsNamesNoPlace()
    {
        var source = new Source { MonsterAt = new[] { "The Mines" } };
        Assert.Empty(Build(new QuestFact("15", FactKind.Slay, "Lewis", null, "Slime|Jelly|Sludge", 0, 10, None), source));
    }

    [Fact]
    public void AnItemAMachineMakesSaysWhichMachineAndWhatGoesIn()
    {
        var source = new Source { Made = { new MadeWay("Keg", "(O)304"), new MadeWay("Cask", "(O)303"), new MadeWay("Third", "(O)1") } };
        var lines = Build(new QuestFact("q", FactKind.Deliver, "Pam", "(O)303", null, 0, 1, None), source);
        Assert.Contains("hint.made(machine=Keg,item=item(O)304) hint.made(machine=Cask,item=item(O)303)", lines);
    }

    [Fact]
    public void AMachineThatTakesSeveralThingsIsNamedOnceWithTheChoice()
    {
        var source = new Source { Made = { new MadeWay("Cheese Press", "(O)184"), new MadeWay("Cheese Press", "(O)186") } };
        var lines = Build(new QuestFact("q", FactKind.Deliver, "Gus", "(O)424", null, 0, 2, None), source);
        Assert.Contains("hint.made(machine=Cheese Press,item=item(O)184hint.oritem(O)186)", lines);
    }

    [Fact]
    public void ACropThatGrowsInEverySeasonSaysOnlyWhatToPlant()
    {
        var grow = new GrowInfo("Qi Bean", new[] { 0, 1, 2, 3 }, 4, -1);
        var lines = Build(new QuestFact("q", FactKind.Harvest, null, "(O)890", null, 0, 1, None), new Source { GrowData = grow });
        Assert.Contains("hint.grow.any(seed=Qi Bean) hint.grow.ready(days=4)", lines);
    }

    [Fact]
    public void AShopThatAsksFarMoreThanTheCheapestIsLeftOut()
    {
        var source = new Source { Offers = { new ShopOffer("Robin", 10), new ShopOffer("Gift Shop", 10000), new ShopOffer("Other", 90), new ShopOffer("NoPrice", 0) } };
        var lines = Build(new QuestFact("q", FactKind.Gather, null, "(O)388", null, 0, 50, None), source);
        Assert.Contains("hint.sold(shops=hint.price(shop=Robin,price=10), hint.price(shop=Other,price=90), NoPrice)", lines);
    }

    [Fact]
    public void AFishWithHoursAndNoWeatherKeepsTheHoursOnTheirOwnLine()
    {
        var fish = new FishInfo(new[] { "Beach" }, false, "6am to 7pm", "both");
        var lines = Build(new QuestFact("3", FactKind.Fish, "Willy", "(O)131", null, 1, 3, None), new Source { FishData = fish });
        Assert.Contains("hint.fish.time(times=6am to 7pm)", lines);
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
        Assert.Empty(lines);
    }

    [Fact]
    public void ABriefStoryItemGetsOneShortLineForEachFish()
    {
        var fish = new FishInfo(new[] { "Beach" }, false, "6am to 7pm", "both", null, null, new[] { 0, 1 });
        var fact = new QuestFact("q", FactKind.StoryItem, null, "(O)145", null, 0, 1, None) { Brief = true };
        Assert.Equal(new[] { "hint.story.fish(item=item(O)145,places=Beach) hint.fish.seasons(seasons=springhint.andsummer)" }, Build(fact, new Source { FishData = fish }));
    }

    [Fact]
    public void ASingleStoryItemGetsTheFullHelpForIt()
    {
        var fish = new FishInfo(new[] { "Beach" }, false, "6am to 7pm", "both");
        var lines = Build(new QuestFact("q", FactKind.StoryItem, null, "(O)145", null, 0, 1, None), new Source { FishData = fish });
        Assert.Contains("hint.story.carry(count=0,item=item(O)145)", lines);
        Assert.Contains("hint.fish.places(places=Beach)", lines);
        Assert.Contains("hint.fish.time(times=6am to 7pm)", lines);
    }

    [Fact]
    public void ABriefStoryItemThatIsNotAFishStillSaysWhereToGetIt()
    {
        var source = new Source { Offers = { new ShopOffer("Pierre", 120) }, RecipeData = new RecipeInfo(false, true, new[] { "Wood x2" }) };
        var fact = new QuestFact("q", FactKind.StoryItem, null, "(O)24", null, 0, 1, None) { Brief = true };
        var lines = Build(fact, source);
        Assert.Contains("hint.sold(shops=hint.price(shop=Pierre,price=120))", lines);
        Assert.Contains("hint.craft.known(ingredients=Wood x2)", lines);
    }

    [Fact]
    public void ADoneFishingQuestSaysToGoBackInsteadOfWhereToFish()
    {
        var fish = new FishInfo(new[] { "Beach" }, false, null, "both");
        var lines = Build(new QuestFact("3", FactKind.Fish, "Willy", "(O)131", null, 3, 3, None), new Source { FishData = fish });
        Assert.Equal(new[] { "hint.return(npc=WILLY)" }, lines);
    }

    [Fact]
    public void ALostItemSaysWhereItLiesUntilItIsFound()
    {
        var open = new QuestFact("9", FactKind.LostItem, "Linus", "(O)789", null, 0, 1, None, "Mountain");
        Assert.Equal(new[] { "hint.lost(place=The Mountain)" }, Build(open, new Source { PlaceDisplay = "The Mountain" }));

        var found = open with { Have = 1 };
        Assert.Equal(new[] { "hint.return.where(npc=LINUS,place=Mountain Camp)" }, Build(found, new Source { Place = "Mountain Camp" }));
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
        Assert.Contains("hint.cook.unknown hint.unlock.friendship(hearts=3,npc=Gus)", lines);

        var skill = new RecipeInfo(false, false, None, new RecipeUnlock(UnlockKind.Skill, null, "Mining", 2));
        Assert.Contains("hint.craft.unknown hint.unlock.skill(skill=Mining,level=2)", Build(new QuestFact("4", FactKind.Craft, null, "(BC)13", null, 0, 1, None), new Source { RecipeData = skill }));

        // a way the game does not spell out is left unsaid, so the line is only that it is not known
        var other = new RecipeInfo(false, false, None, new RecipeUnlock(UnlockKind.Other, null, null, 0));
        Assert.Equal(new[] { "hint.craft.unknown" }, Build(new QuestFact("4", FactKind.Craft, null, "(BC)13", null, 0, 1, None), new Source { RecipeData = other }));
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
        Assert.Equal(new[] { "hint.left(names=N1, N2, N3, N4, N5, N6 hint.more(count=5))" }, lines);
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
        public GrowInfo? Grow(string qualifiedItemId) => null;
        public GrowInfo? GrowForTag(string tag) => null;
        public ForageInfo? Forage(string qualifiedItemId) => null;
        public DropInfo? Drop(string qualifiedItemId) => null;
        public RecipeInfo? Recipe(string qualifiedItemId) => null;
        public int ClockNow => 1200;
        public bool RainingNow => false;
        public BuildingInfo? Building(string buildingType) => null;
        public int DeepestLevel(string area) => 0;
        public int SeasonNow => 1;
        public string SeasonName(int season) => "s" + season;
        public IReadOnlyList<ShopOffer> ShopsFor(string qualifiedItemId) => Array.Empty<ShopOffer>();
    }

    [Fact]
    public void AnOpenKillQuestDoesNotSayWhereThePersonIs()
    {
        var lines = HintBuilder.Build(new QuestFact("2", FactKind.Slay, "Lewis", null, "Slime", 1, 5, Array.Empty<string>()), new ModConfig(), new Source(), new Texts());
        Assert.Empty(lines);
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
        public GrowInfo? Grow(string qualifiedItemId) => null;
        public GrowInfo? GrowForTag(string tag) => null;
        public ForageInfo? Forage(string qualifiedItemId) => null;
        public DropInfo? Drop(string qualifiedItemId) => null;
        public RecipeInfo? Recipe(string qualifiedItemId) => null;
        public int ClockNow => Clock;
        public bool RainingNow => Raining;
        public BuildingInfo? Building(string buildingType) => null;
        public int DeepestLevel(string area) => 0;
        public int SeasonNow => 1;
        public string SeasonName(int season) => "s" + season;
        public IReadOnlyList<ShopOffer> ShopsFor(string qualifiedItemId) => Offers;
    }

    private static readonly string[] None = Array.Empty<string>();

    private static List<string> Build(QuestFact fact, Source source, ModConfig? config = null) =>
        HintBuilder.Build(fact, config ?? new ModConfig(), source, new Texts());

    private static FishInfo SeasonalFish(params int[] seasons) =>
        new(new[] { "Beach" }, false, null, "both", null, null, seasons);

    [Fact]
    public void AFishSaysWhichSeasonsItBitesIn()
    {
        var source = new Source { FishData = SeasonalFish(0, 1) };
        var lines = Build(new QuestFact("1", FactKind.Fish, "Willy", "(O)145", null, 0, 1, None), source);
        Assert.Contains("hint.fish.seasons(seasons=s0hint.ands1)", lines);
    }

    [Fact]
    public void ThreeSeasonsAreListedWithCommasAndAnAnd()
    {
        var source = new Source { FishData = SeasonalFish(0, 1, 2) };
        var lines = Build(new QuestFact("1", FactKind.Fish, "Willy", "(O)145", null, 0, 1, None), source);
        Assert.Contains("hint.fish.seasons(seasons=s0, s1hint.ands2)", lines);
    }

    [Fact]
    public void OutOfSeasonSaysSoAndSkipsTheClock()
    {
        // the fake's season now is 1, summer, so a winter fish is out of season
        var source = new Source { FishData = new FishInfo(new[] { "Beach" }, false, "6am to 7pm", "sunny", null, new[] { (600, 1900) }, new[] { 3 }), Clock = 1200 };
        var lines = Build(new QuestFact("1", FactKind.Fish, "Willy", "(O)707", null, 0, 1, None), source);
        Assert.Contains("hint.fish.noseason", lines);
        Assert.DoesNotContain("hint.fish.now", lines);
        Assert.DoesNotContain("hint.fish.notime", lines);
    }

    [Fact]
    public void InSeasonKeepsTheRightNowLine()
    {
        var source = new Source { FishData = new FishInfo(new[] { "Beach" }, false, "6am to 7pm", "both", null, new[] { (600, 1900) }, new[] { 1 }), Clock = 1200 };
        var lines = Build(new QuestFact("1", FactKind.Fish, "Willy", "(O)128", null, 0, 1, None), source);
        Assert.Contains("hint.fish.now", lines);
        Assert.DoesNotContain("hint.fish.noseason", lines);
    }

    [Fact]
    public void AShopThatSellsTheItemIsNamedWithItsPrice()
    {
        var source = new Source { Offers = { new ShopOffer("Pierre", 120), new ShopOffer("Robin", 0) } };
        var lines = Build(new QuestFact("1", FactKind.Deliver, null, "(O)245", null, 0, 1, None), source);
        Assert.Contains("hint.sold(shops=hint.price(shop=Pierre,price=120), Robin)", lines);
    }

    [Fact]
    public void OnlyTheFirstThreeShopsAreNamed()
    {
        var source = new Source { Offers = { new ShopOffer("A", 1), new ShopOffer("B", 2), new ShopOffer("C", 3), new ShopOffer("D", 4) } };
        var lines = Build(new QuestFact("1", FactKind.Deliver, null, "(O)245", null, 0, 1, None), source);
        Assert.Contains("hint.sold(shops=hint.price(shop=A,price=1), hint.price(shop=B,price=2), hint.price(shop=C,price=3))", lines);
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
