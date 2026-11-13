namespace Questie.Tests;

public class ForageConditionsTests
{
    [Theory]
    [InlineData("", null)]
    [InlineData("YEAR 2", null)]
    [InlineData("WEATHER Here Rain", null)]
    [InlineData("TRUE", null)]
    [InlineData("FALSE", "")]
    [InlineData("!TRUE", "")]
    [InlineData("!FALSE", null)]
    [InlineData("SEASON spring", "0")]
    [InlineData("SEASON spring summer", "0,1")]
    [InlineData("YEAR 2, SEASON spring", "0")]
    [InlineData("LOCATION_SEASON Here spring summer", "0,1")]
    [InlineData("SEASON_DAY spring 15 summer 20", "0,1")]
    [InlineData("!SEASON winter", "0,1,2")]
    [InlineData("SEASON spring, !SEASON winter", "0")]
    [InlineData("SEASON spring, SEASON summer", "")]
    [InlineData("DATE_RANGE Fall 15 1", null)]
    [InlineData("DATE_RANGE Summer 15 1 Winter 15 1", "1,2,3")]
    [InlineData("DATE_RANGE Fall 15 1 Spring 28 2", "0,2,3")]
    [InlineData("!DATE_RANGE Summer 15 1 Winter 15 1", "0")]
    [InlineData("ANY \"SEASON spring\" \"SEASON fall\"", "0,2")]
    [InlineData("ANY \"SEASON spring, DAY_OF_WEEK Friday\" \"SEASON fall\"", "0,2")]
    [InlineData("ANY \"SEASON spring\" \"YEAR 2\"", null)]
    [InlineData("!ANY \"SEASON winter\" \"SEASON fall\"", "0,1")]
    [InlineData("!ANY \"SEASON spring\" \"YEAR 2\"", "1,2,3")]
    public void ReadsTheSeasonsAConditionAllows(string condition, string? expected)
    {
        Assert.Equal(expected, Parse(condition));
    }

    [Fact]
    public void ABlankConditionDoesNotLimitSeasons()
    {
        Assert.Null(ForageConditions.Seasons(null));
        Assert.Null(ForageConditions.Seasons("  "));
    }

    private static string? Parse(string condition) =>
        ForageConditions.Seasons(condition) is { } seasons ? string.Join(",", seasons) : null;
}
