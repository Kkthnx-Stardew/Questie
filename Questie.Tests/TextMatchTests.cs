namespace Questie.Tests;

public class TextMatchTests
{
    [Theory]
    [InlineData("Void Soul", "Void Souls")]
    [InlineData("Peach", "Peaches")]
    [InlineData("Radish", "Radishes")]
    [InlineData("Box", "Boxes")]
    [InlineData("Blueberry", "Blueberries")]
    [InlineData("Key", "Keys")]
    [InlineData("Octopus", "Octopuses")]
    [InlineData("Potato", "Potatoes")]
    [InlineData("Mango", "Mangoes")]
    [InlineData("Small Manta Ray", "Small Manta Rays")]
    public void ANameGetsItsPlural(string name, string plural)
    {
        Assert.Contains(plural, TextMatch.Plurals(name));
    }

    [Fact]
    public void APluralIsFoundAndItsWholeLengthIsReturned()
    {
        var at = TextMatch.IndexOfName("Bring 5 void souls to town", "Void Soul", out var length);
        Assert.Equal(8, at);
        Assert.Equal(10, length);
    }

    [Fact]
    public void ASingularIsFoundAsBefore()
    {
        Assert.Equal(0, TextMatch.IndexOfName("Void Soul", "Void Soul", out var length));
        Assert.Equal(9, length);
    }

    [Fact]
    public void ANameIsNotFoundInsideALongerWord()
    {
        Assert.Equal(-1, TextMatch.IndexOfName("void soulish", "Void Soul", out _));
        Assert.Equal(-1, TextMatch.IndexOfName("void soulsong", "Void Soul", out _));
    }

    [Fact]
    public void APluralDoesNotChangeAVowelBeforeY()
    {
        Assert.Contains("Keys", TextMatch.Plurals("Key"));
        Assert.DoesNotContain("Kies", TextMatch.Plurals("Key"));
    }
}
