namespace Questie.Tests;

public class MonsterExtraDropsTests
{
    [Fact]
    public void AGhostHasEctoplasm()
    {
        var drops = MonsterExtraDrops.For("Ghost", "Ghost");
        Assert.Contains(drops, drop => drop.ItemId == "(O)875" && drop.Chance == 0.095);
    }

    [Fact]
    public void APrismaticSlimeAlwaysHasItsJelly()
    {
        var drops = MonsterExtraDrops.For(null, "Prismatic Slime");
        Assert.Contains(drops, drop => drop.ItemId == "(O)876" && drop.Chance == 1);
    }

    [Fact]
    public void APepperRexHasItsEggAndThreeBones()
    {
        var items = MonsterExtraDrops.For("Pepper Rex", "Pepper Rex").Select(drop => drop.ItemId).ToArray();
        Assert.Equal(new[] { "(O)107", "(O)580", "(O)583", "(O)584" }, items);
    }

    [Fact]
    public void AVariantNameFindsTheExtrasOfItsClass()
    {
        Assert.Contains(MonsterExtraDrops.For("Bat", "Haunted Skull"), drop => drop.ItemId == "(O)279");
        Assert.Contains(MonsterExtraDrops.For("Serpent", "Royal Serpent"), drop => drop.ItemId == "(O)485");
    }

    [Fact]
    public void AnIdFindsItsExtrasWhenTheDisplayNameIsTranslated()
    {
        Assert.Contains(MonsterExtraDrops.For("Ghost", "Geist"), drop => drop.ItemId == "(O)875");
    }

    [Fact]
    public void AnUnknownMonsterHasNoExtraDrops()
    {
        Assert.Empty(MonsterExtraDrops.For("Modded Bat", "Modded Bat"));
    }
}
