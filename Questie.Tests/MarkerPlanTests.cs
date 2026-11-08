namespace Questie.Tests;

public class MarkerPlanTests
{
    private static readonly string[] None = Array.Empty<string>();

    private static QuestFact Deliver(string npc, string item, int need = 1) => new("1", FactKind.Deliver, npc, item, null, 0, need, None);

    private static QuestFact Slay(string monster, int have, int need, string? npc = null) => new("2", FactKind.Slay, npc, null, monster, have, need, None);

    private static MarkerPlan Build(ModConfig config, Func<string, int> carried, params QuestFact[] facts)
    {
        var plan = new MarkerPlan();
        plan.Rebuild(facts, config, carried);
        return plan;
    }

    [Fact]
    public void ADeliveryIsMarkedOnlyWhileTheItemIsCarried()
    {
        var fact = Deliver("Pierre", "(O)24", 2);
        Assert.False(Build(new ModConfig(), _ => 1, fact).MarksNpc("Pierre"));
        Assert.True(Build(new ModConfig(), _ => 2, fact).MarksNpc("Pierre"));
    }

    [Fact]
    public void ADeliveryIsAlwaysMarkedWhenTheCarryRuleIsOff()
    {
        var config = new ModConfig { OnlyWhenCarried = false };
        Assert.True(Build(config, _ => 0, Deliver("Pierre", "(O)24")).MarksNpc("Pierre"));
    }

    [Fact]
    public void DeliveriesCanBeSwitchedOff()
    {
        var config = new ModConfig { MarkDeliveries = false };
        Assert.True(Build(config, _ => 9, Deliver("Pierre", "(O)24")).IsEmpty);
    }

    [Fact]
    public void AKillQuestMarksTheMonsterUntilItIsDone()
    {
        var plan = Build(new ModConfig(), _ => 0, Slay("Green Slime", 1, 5, "Lewis"));
        Assert.True(plan.MarksMonster("Green Slime"));
        Assert.False(plan.MarksNpc("Lewis"));
    }

    [Fact]
    public void AFinishedKillQuestMarksThePersonToReturnTo()
    {
        var plan = Build(new ModConfig(), _ => 0, Slay("Green Slime", 5, 5, "Lewis"));
        Assert.True(plan.MarksNpc("Lewis"));
        Assert.False(plan.MarksMonster("Green Slime"));
    }

    [Fact]
    public void AMonsterIsMatchedByPartOfItsName()
    {
        var plan = Build(new ModConfig(), _ => 0, Slay("Slime", 0, 3));
        Assert.True(plan.MarksMonster("Green Slime"));
        Assert.False(plan.MarksMonster("Bat"));
    }

    [Fact]
    public void ATurnInWithNoPersonMarksNobody()
    {
        Assert.True(Build(new ModConfig(), _ => 0, Slay("Green Slime", 5, 5, "null")).IsEmpty);
        Assert.True(Build(new ModConfig(), _ => 0, Slay("Green Slime", 5, 5, null)).IsEmpty);
    }

    [Fact]
    public void SocializeMarksEveryoneLeft()
    {
        var fact = new QuestFact("3", FactKind.Socialize, null, null, null, 1, 3, new[] { "Abigail", "Alex" });
        var plan = Build(new ModConfig(), _ => 0, fact);
        Assert.True(plan.MarksNpc("Abigail"));
        Assert.True(plan.MarksNpc("Alex"));
        Assert.False(plan.MarksNpc("Pierre"));
    }

    [Fact]
    public void NothingIsMarkedWhenMarkersAreOff()
    {
        var config = new ModConfig { EnableMarkers = false };
        Assert.True(Build(config, _ => 9, Deliver("Pierre", "(O)24"), Slay("Green Slime", 0, 3)).IsEmpty);
    }

    [Fact]
    public void RebuildingReplacesTheOldPlan()
    {
        var plan = new MarkerPlan();
        plan.Rebuild(new[] { Deliver("Pierre", "(O)24") }, new ModConfig(), _ => 1);
        Assert.True(plan.MarksNpc("Pierre"));
        plan.Rebuild(Array.Empty<QuestFact>(), new ModConfig(), _ => 1);
        Assert.True(plan.IsEmpty);
    }
}

public class MarkerKindTests
{
    private static readonly string[] None = Array.Empty<string>();

    [Fact]
    public void ADeliveryRemembersTheItemToShow()
    {
        var plan = new MarkerPlan();
        plan.Rebuild(new[] { new QuestFact("1", FactKind.Deliver, "Pierre", "(O)24", null, 0, 1, None) }, new ModConfig(), _ => 1);
        Assert.True(plan.TryGetMark("Pierre", out var mark));
        Assert.Equal(new Mark(MarkKind.HandIn, "(O)24"), mark);
    }

    [Fact]
    public void AGreetingHasTheGreetKindAndNoItem()
    {
        var plan = new MarkerPlan();
        plan.Rebuild(new[] { new QuestFact("5", FactKind.Socialize, null, null, null, 0, 2, new[] { "Abigail" }) }, new ModConfig(), _ => 0);
        Assert.Equal(new Mark(MarkKind.Greet, null), plan.TryGetMark("Abigail", out var mark) ? mark : default);
    }

    [Fact]
    public void AHandInOutranksAGreetingForTheSamePerson()
    {
        var plan = new MarkerPlan();
        var facts = new[]
        {
            new QuestFact("5", FactKind.Socialize, null, null, null, 0, 2, new[] { "Pierre" }),
            new QuestFact("1", FactKind.Deliver, "Pierre", "(O)24", null, 0, 1, None),
        };
        plan.Rebuild(facts, new ModConfig(), _ => 1);
        Assert.Equal(MarkKind.HandIn, plan.NpcMark("Pierre"));
    }

    [Fact]
    public void ALostItemMarksThePersonOnlyOnceItIsFoundAndCarried()
    {
        var fact = new QuestFact("9", FactKind.LostItem, "Linus", "(O)789", null, 1, 1, None);
        var notFound = fact with { Have = 0 };
        var plan = new MarkerPlan();
        plan.Rebuild(new[] { notFound }, new ModConfig(), _ => 1);
        Assert.True(plan.IsEmpty);
        plan.Rebuild(new[] { fact }, new ModConfig(), _ => 0);
        Assert.True(plan.IsEmpty);
        plan.Rebuild(new[] { fact }, new ModConfig(), _ => 1);
        Assert.Equal(MarkKind.HandIn, plan.NpcMark("Linus"));
    }

    [Fact]
    public void AMarkedPersonWithNoMarkerIsNone()
    {
        Assert.Equal(MarkKind.None, new MarkerPlan().NpcMark("Pierre"));
    }
}
