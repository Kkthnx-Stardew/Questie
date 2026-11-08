namespace Questie.Tests;

public class FishTimesTests
{
    [Fact]
    public void ReadsOneWindow()
    {
        Assert.Equal(new[] { (600, 1900) }, FishTimes.ParseWindows("600 1900"));
    }

    [Fact]
    public void ReadsSeveralWindows()
    {
        Assert.Equal(new[] { (600, 1100), (1700, 2600) }, FishTimes.ParseWindows("600 1100 1700 2600"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("soon")]
    [InlineData("1900 600")]
    [InlineData("600")]
    public void ABlankOrBrokenFieldGivesNoWindows(string? field)
    {
        Assert.Empty(FishTimes.ParseWindows(field));
    }

    [Fact]
    public void AWholeDayIsRecognised()
    {
        Assert.True(FishTimes.IsAllDay(FishTimes.ParseWindows("600 2600")));
        Assert.False(FishTimes.IsAllDay(FishTimes.ParseWindows("600 1900")));
        Assert.False(FishTimes.IsAllDay(FishTimes.ParseWindows("600 1100 1700 2600")));
    }

    [Theory]
    [InlineData(600, "6am")]
    [InlineData(1200, "12pm")]
    [InlineData(1830, "6:30pm")]
    [InlineData(2400, "12am")]
    [InlineData(2600, "2am")]
    [InlineData(0, "12am")]
    public void FormatsClockValues(int clock, string expected)
    {
        Assert.Equal(expected, FishTimes.FormatTime(clock));
    }

    [Fact]
    public void FormatsWindows()
    {
        Assert.Equal("6am to 11am, 5pm to 2am", FishTimes.FormatWindows(FishTimes.ParseWindows("600 1100 1700 2600")));
    }
}

public class FishTimesNowTests
{
    [Theory]
    [InlineData(600, true)]
    [InlineData(1859, true)]
    [InlineData(1900, false)]
    [InlineData(500, false)]
    public void IsOpenFollowsTheWindow(int clock, bool expected)
    {
        Assert.Equal(expected, FishTimes.IsOpen(new[] { (600, 1900) }, clock));
    }

    [Fact]
    public void NoWindowsMeansAllDay()
    {
        Assert.True(FishTimes.IsOpen(Array.Empty<(int, int)>(), 300));
    }

    [Theory]
    [InlineData("sunny", false, true)]
    [InlineData("sunny", true, false)]
    [InlineData("rainy", true, true)]
    [InlineData("rainy", false, false)]
    [InlineData("both", true, true)]
    [InlineData("both", false, true)]
    public void WeatherSuitsFollowsTheFishData(string weather, bool raining, bool expected)
    {
        Assert.Equal(expected, FishTimes.WeatherSuits(weather, raining));
    }
}
