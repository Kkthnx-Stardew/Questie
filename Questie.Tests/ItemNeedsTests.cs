namespace Questie.Tests;

public class ItemNeedsTests
{
    private sealed class Source : IHintSource
    {
        public string NpcName(string npc) => npc.ToUpperInvariant();
        public string ItemName(string qualifiedItemId) => "name" + qualifiedItemId;
        public int Carried(string qualifiedItemId) => 0;
        public bool IsHere(string npc) => false;
        public string? WhereIs(string npc) => null;
        public string? PlaceName(string internalName) => internalName;
        public FishInfo? Fish(string qualifiedItemId) => null;
        public RecipeInfo? Recipe(string qualifiedItemId) => null;
        public int ClockNow => 1200;
        public bool RainingNow => false;
        public IReadOnlyList<ShopOffer> ShopsFor(string qualifiedItemId) => Array.Empty<ShopOffer>();
    }

    private sealed class Texts : IHintTexts
    {
        public string Get(string key, params (string Name, string Value)[] tokens) => key + "(" + string.Join(",", tokens.Select(t => $"{t.Name}={t.Value}")) + ")";
    }

    private static readonly IReadOnlyList<string> None = Array.Empty<string>();

    private static QuestFact Fact(FactKind kind, string? item, int have = 0, int need = 1, string? npc = "Lewis", string? title = "Title") =>
        new("1", kind, npc, item, null, have, need, None, null, title);

    [Theory]
    [InlineData(FactKind.Deliver, 0, 1, true)]
    [InlineData(FactKind.Gather, 3, 10, true)]
    [InlineData(FactKind.Gather, 10, 10, false)]
    [InlineData(FactKind.Fish, 0, 2, true)]
    [InlineData(FactKind.Fish, 2, 2, false)]
    [InlineData(FactKind.LostItem, 0, 1, false)]
    [InlineData(FactKind.LostItem, 1, 1, true)]
    [InlineData(FactKind.Craft, 0, 1, false)]
    [InlineData(FactKind.Slay, 0, 5, false)]
    public void WantsByKind(FactKind kind, int have, int need, bool wanted)
    {
        Assert.Equal(wanted, ItemNeeds.Wants(Fact(kind, "(O)24", have, need)));
    }

    [Fact]
    public void NoItemMeansNothingIsWanted()
    {
        Assert.False(ItemNeeds.Wants(Fact(FactKind.Deliver, null)));
    }

    [Fact]
    public void CollectsTheWantedIds()
    {
        var set = new HashSet<string> { "(O)stale" };
        ItemNeeds.Collect(new[] { Fact(FactKind.Deliver, "(O)24"), Fact(FactKind.Fish, "(O)128", 1, 1), Fact(FactKind.Gather, "(O)388", 0, 5) }, set);
        Assert.Equal(new[] { "(O)24", "(O)388" }, set.OrderBy(x => x));
    }

    [Fact]
    public void NotesSayWhichQuestAndWhatToDo()
    {
        var facts = new[] { Fact(FactKind.Deliver, "(O)24", 0, 3, "Lewis", "Lewis' Order"), Fact(FactKind.Gather, "(O)24", 2, 10, null, "Gathering") };
        var notes = ItemNeeds.Notes("(O)24", facts, new Source(), new Texts());
        Assert.Equal(new[] { "note.deliver(quest=Lewis' Order,count=3,npc=LEWIS)", "note.progress(quest=Gathering,have=2,need=10)" }, notes);
    }

    [Fact]
    public void NotesAreLimitedToTwoQuests()
    {
        var facts = Enumerable.Range(0, 4).Select(i => Fact(FactKind.Deliver, "(O)24", 0, 1, "Lewis", "Q" + i)).ToArray();
        Assert.Equal(2, ItemNeeds.Notes("(O)24", facts, new Source(), new Texts()).Count);
    }

    [Fact]
    public void NotesFallBackToTheItemNameWithoutATitle()
    {
        var notes = ItemNeeds.Notes("(O)24", new[] { Fact(FactKind.Gather, "(O)24", 0, 5, null, null) }, new Source(), new Texts());
        Assert.Equal("note.progress(quest=name(O)24,have=0,need=5)", notes[0]);
    }

    [Fact]
    public void OtherItemsGetNoNotes()
    {
        Assert.Empty(ItemNeeds.Notes("(O)99", new[] { Fact(FactKind.Deliver, "(O)24") }, new Source(), new Texts()));
    }
}
