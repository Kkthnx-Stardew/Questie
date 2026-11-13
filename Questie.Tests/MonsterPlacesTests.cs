namespace Questie.Tests;

public class MonsterPlacesTests
{
    [Fact]
    public void AKnownMonsterHasItsPlaces()
    {
        Assert.Equal(new[] { "The Mines", "Skull Cavern" }, MonsterPlaces.For("Bat", "Bat"));
    }

    [Fact]
    public void APlaceIsFoundFromTheIdWhenTheDisplayNameIsTranslated()
    {
        Assert.Equal(new[] { "The Mines" }, MonsterPlaces.For("Frost Bat", "Fledermaus"));
    }

    [Fact]
    public void AVariantNameBeatsItsParentsId()
    {
        Assert.Equal(new[] { "Quarry Mine", "Skull Cavern", "The Mines" }, MonsterPlaces.For("Bat", "Haunted Skull"));
    }

    [Fact]
    public void AnUnknownMonsterHasNoPlace()
    {
        Assert.Empty(MonsterPlaces.For("Modded Bat", "Modded Bat"));
    }
}
