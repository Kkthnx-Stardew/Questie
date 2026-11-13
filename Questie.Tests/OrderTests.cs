namespace Questie.Tests;

public class OrderTests
{
    private sealed class Texts : IHintTexts
    {
        public string Get(string key, params (string Name, string Value)[] tokens) =>
            key + (tokens.Length == 0 ? "" : "(" + string.Join(",", tokens.Select(t => $"{t.Name}={t.Value}")) + ")");
    }

    private sealed class Source : IHintSource
    {
        public bool Here;
        public string? Place;
        public string? BoxPlace = "Town";
        public string NpcName(string npc) => npc.ToUpperInvariant();
        public string ItemName(string qualifiedItemId) => qualifiedItemId;
        public int Carried(string qualifiedItemId) => 0;
        public int InChests(string qualifiedItemId) => 0;
        public bool IsHere(string npc) => Here;
        public string? WhereIs(string npc) => Place;
        public string? PlaceName(string internalName) => BoxPlace;
        public FishInfo? Fish(string qualifiedItemId) => null;
        public GrowInfo? Grow(string qualifiedItemId) => null;
        public GrowInfo? GrowForTag(string tag)
        {
            GrowTag = tag;
            return GrowTagData;
        }
        public GrowInfo? GrowTagData;
        public string? GrowTag;
        public ForageInfo? Forage(string qualifiedItemId) => null;
        public DropInfo? Drop(string qualifiedItemId) => null;
        public int ClockNow => 1200;
        public bool RainingNow => false;
        public BuildingInfo? Building(string buildingType) => null;
        public int DeepestLevel(string area) => 0;
        public int SeasonNow => 0;
        public string SeasonName(int season) => "spring";
        public IReadOnlyList<ShopOffer> ShopsFor(string qualifiedItemId) => Array.Empty<ShopOffer>();
        public RecipeInfo? Recipe(string qualifiedItemId) => null;
    }

    private static OrderWant Deliver(int have, int need, int carried, int biggest, string? item = "(O)24") =>
        new() { OrderKey = "k", Order = "Order", Kind = OrderWantKind.Deliver, Npc = "Pierre", Have = have, Need = need, Carried = carried, LargestStack = biggest, HeldItemId = item };

    private static List<string> Journal(OrderWant want, Source? source = null, ModConfig? config = null) =>
        OrderNotes.Journal(new[] { want }, config ?? new ModConfig(), source ?? new Source(), new Texts());

    [Fact]
    public void ADeliveryNeedsOneStackThatCoversWhatIsLeft()
    {
        Assert.True(Deliver(0, 5, 5, 5).CanHandOver);
        Assert.False(Deliver(0, 5, 6, 3).CanHandOver); // six carried, but in two stacks
        Assert.False(Deliver(5, 5, 0, 0).CanHandOver); // nothing left to hand over
    }

    [Fact]
    public void ADonationTakesWhatIsCarriedALittleAtATime()
    {
        var want = new OrderWant { Kind = OrderWantKind.Donate, Have = 0, Need = 20, Carried = 3, LargestStack = 2 };
        Assert.True(want.CanHandOver);
        Assert.False(new OrderWant { Kind = OrderWantKind.Donate, Need = 20 }.CanHandOver);
    }

    [Fact]
    public void ADeliveryJournalSaysWhatIsCarriedWhereToGoAndWhereTheyAre()
    {
        var lines = Journal(Deliver(1, 4, 2, 2), new Source { Place = "Town" });
        Assert.Equal(new[]
        {
            "hint.order.carry(count=2,left=3)", "hint.order.onestack", "hint.order.deliver(npc=PIERRE)", "hint.where(npc=PIERRE,place=Town)",
        }, lines);
    }

    [Fact]
    public void SeveralObjectivesAreNamedAndSharedLinesAreSaidOnce()
    {
        var wants = new[]
        {
            new OrderWant { Kind = OrderWantKind.Deliver, Npc = "Emily", Text = "Deliver a Ruby to Emily.", Have = 0, Need = 1, Carried = 1 },
            new OrderWant { Kind = OrderWantKind.Deliver, Npc = "Emily", Text = "Deliver a Topaz to Emily", Have = 0, Need = 1 },
        };
        var lines = OrderNotes.Journal(wants, new ModConfig(), new Source { Place = "Town" }, new Texts());
        Assert.Equal(new[]
        {
            "hint.order.carry.each(what=Deliver a Ruby to Emily,count=1)",
            "hint.order.deliver(npc=EMILY)", "hint.where(npc=EMILY,place=Town)",
        }, lines);
    }

    [Fact]
    public void SeveralObjectivesWithNothingCarriedSaySoOnceInsteadOfOnceEach()
    {
        var wants = new[]
        {
            new OrderWant { Kind = OrderWantKind.Donate, Text = "Drop off 20 void souls.", Have = 0, Need = 20 },
            new OrderWant { Kind = OrderWantKind.Donate, Text = "Drop off 25 red slime eggs.", Have = 0, Need = 25 },
        };
        var lines = OrderNotes.Journal(wants, new ModConfig(), new Source(), new Texts());
        Assert.Equal(new[] { "hint.order.carry.none" }, lines);
    }

    [Fact]
    public void ASingleItemDeliveryNeedsNoWordAboutStacks()
    {
        Assert.DoesNotContain(Journal(Deliver(0, 1, 0, 0)), l => l.StartsWith("hint.order.onestack"));
    }

    [Fact]
    public void WhereThePersonIsFollowsTheSetting()
    {
        var lines = Journal(Deliver(0, 1, 0, 0), new Source { Here = true }, new ModConfig { ShowWhereIsNpc = false });
        Assert.DoesNotContain(lines, l => l.StartsWith("hint.here") || l.StartsWith("hint.where"));
        Assert.Contains("hint.here(npc=PIERRE)", Journal(Deliver(0, 1, 0, 0), new Source { Here = true }));
    }

    [Fact]
    public void ADonationJournalNamesThePlaceOfTheBox()
    {
        var want = new OrderWant { Kind = OrderWantKind.Donate, Place = "Farm", Have = 2, Need = 10, Carried = 4 };
        Assert.Equal(new[] { "hint.order.carry(count=4,left=8)", "hint.order.donate(place=Town)" }, Journal(want));
        Assert.Equal(new[] { "hint.order.carry(count=4,left=8)" }, Journal(want, new Source { BoxPlace = null }));
    }

    [Fact]
    public void AShipmentCountedInGoldHasNoCarryLine()
    {
        var want = new OrderWant { Kind = OrderWantKind.Ship, Have = 100, Need = 5000, ByValue = true };
        Assert.Equal(new[] { "hint.order.ship" }, Journal(want));
        Assert.Equal(new[] { "hint.order.carry(count=0,left=5)", "hint.order.ship" }, Journal(new OrderWant { Kind = OrderWantKind.Ship, Have = 0, Need = 5 }));
    }

    [Fact]
    public void AKillObjectiveAddsNoJournalLines()
    {
        Assert.Empty(Journal(new OrderWant { Kind = OrderWantKind.Slay, Monsters = new[] { "Bat" }, Need = 10 }));
    }

    [Theory]
    [InlineData("id_o_192", "id_o_192")]
    [InlineData(" item_potato ", "item_potato")]
    [InlineData("crop_potato", "crop_potato")]
    [InlineData("id_o_192,category_vegetable", "id_o_192")]
    [InlineData("id_o_192,quality_gold", "id_o_192")]
    [InlineData("category_vegetable", null)]
    [InlineData("item_type_vegetable", null)]
    [InlineData("id_o_192/id_o_193", null)]
    [InlineData("id_o_192,id_o_193", null)]
    [InlineData("!id_o_192", null)]
    [InlineData("", null)]
    public void OneItemTagMustNameOneItem(string set, string? expected)
    {
        Assert.Equal(expected, OrderTag.SingleItemTag(new[] { set }));
    }

    [Fact]
    public void SeveralTagListsMustAllNameTheSameItem()
    {
        Assert.Equal("id_o_192", OrderTag.SingleItemTag(new[] { "id_o_192", "id_o_192" }));
        Assert.Null(OrderTag.SingleItemTag(new[] { "id_o_192", "id_o_193" }));
        Assert.Null(OrderTag.SingleItemTag(new[] { "id_o_192", "category_vegetable" }));
        Assert.Null(OrderTag.SingleItemTag(Array.Empty<string>()));
    }

    [Fact]
    public void ACropOrderSaysWhatToPlant()
    {
        var source = new Source { GrowTagData = new GrowInfo("Potato Seeds", new[] { 0 }, 6, -1) };
        var want = new OrderWant { Kind = OrderWantKind.Ship, Have = 0, Need = 100, ItemTag = "id_o_192" };
        Assert.Equal(new[]
        {
            "hint.order.carry(count=0,left=100)", "hint.grow(seed=Potato Seeds,seasons=spring) hint.grow.ready(days=6)", "hint.order.ship",
        }, Journal(want, source));
        Assert.Equal("id_o_192", source.GrowTag);
    }

    [Fact]
    public void AnItemThatIsNotACropGetsNoGrowLine()
    {
        var want = new OrderWant { Kind = OrderWantKind.Ship, Have = 0, Need = 5, ItemTag = "item_ruby" };
        Assert.DoesNotContain(Journal(want, new Source()), line => line.StartsWith("hint.grow"));
    }

    [Fact]
    public void NotesFollowTheSettingForNotes()
    {
        Assert.Empty(Journal(Deliver(0, 1, 1, 1), null, new ModConfig { ShowHints = false }));
    }

    [Fact]
    public void TheTooltipSaysWhatToDoWithTheItem()
    {
        var texts = new Texts();
        var source = new Source();
        Assert.Equal("note.order.deliver(order=Order,npc=PIERRE)", OrderNotes.Tooltip(Deliver(0, 1, 0, 0), source, texts));
        Assert.Equal("note.order.donate(order=O)", OrderNotes.Tooltip(new OrderWant { Order = "O", Kind = OrderWantKind.Donate }, source, texts));
        Assert.Equal("note.order.ship(order=O)", OrderNotes.Tooltip(new OrderWant { Order = "O", Kind = OrderWantKind.Ship }, source, texts));
        Assert.Null(OrderNotes.Tooltip(new OrderWant { Kind = OrderWantKind.Slay }, source, texts));
    }

    // ---------------------------------------------------------------- markers

    private static MarkerPlan Plan(IReadOnlyList<OrderWant> orders, ModConfig? config = null)
    {
        var plan = new MarkerPlan();
        plan.Rebuild(Array.Empty<QuestFact>(), config ?? new ModConfig(), _ => 0, orders);
        return plan;
    }

    [Fact]
    public void ADeliveryOrderMarksThePersonOnlyWhileTheRightStackIsCarried()
    {
        var carrying = Plan(new[] { Deliver(0, 3, 3, 3, "(O)88") });
        Assert.True(carrying.TryGetMark("Pierre", out var mark));
        Assert.Equal(new Mark(MarkKind.HandIn, "(O)88"), mark);

        Assert.False(Plan(new[] { Deliver(0, 3, 3, 2) }).MarksNpc("Pierre")); // three carried in two stacks
        Assert.False(Plan(new[] { Deliver(0, 3, 0, 0, null) }).MarksNpc("Pierre"));
    }

    [Fact]
    public void ADeliveryOrderIsMarkedWithoutTheItemWhenCarryingIsNotRequired()
    {
        var plan = Plan(new[] { Deliver(0, 3, 0, 0, null) }, new ModConfig { OnlyWhenCarried = false });
        Assert.True(plan.TryGetMark("Pierre", out var mark));
        Assert.Null(mark.ItemId);
    }

    [Fact]
    public void ADeliveryOrderFollowsTheDeliverySetting()
    {
        Assert.True(Plan(new[] { Deliver(0, 1, 1, 1) }).MarksNpc("Pierre"));
        Assert.False(Plan(new[] { Deliver(0, 1, 1, 1) }, new ModConfig { MarkDeliveries = false }).MarksNpc("Pierre"));
        Assert.True(Plan(Array.Empty<OrderWant>()).IsEmpty);
    }

    [Fact]
    public void AKillOrderMarksItsMonstersUnlessMonstersAreOff()
    {
        var kill = new OrderWant { Kind = OrderWantKind.Slay, Monsters = new[] { "Bat", "Skeleton" }, Need = 5 };
        Assert.Equal(new[] { "Bat", "Skeleton" }, Plan(new[] { kill }).Monsters);
        Assert.False(Plan(new[] { kill }, new ModConfig { MarkMonsters = false }).HasMonsters);
    }

    [Fact]
    public void DonationAndShipmentOrdersMarkNobody()
    {
        var plan = Plan(new[] { new OrderWant { Kind = OrderWantKind.Donate, Need = 5, Carried = 5 }, new OrderWant { Kind = OrderWantKind.Ship, Need = 5, Carried = 5 } });
        Assert.True(plan.IsEmpty);
    }
}
