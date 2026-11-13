namespace Questie.Tests;

public class ForageEntryItemsTests
{
    [Fact]
    public void APlainItemIsOneSource()
    {
        var ids = ForageEntryItems.Resolve(
            "  (O)18  ",
            id => id == "(O)18" ? id : null,
            _ => throw new InvalidOperationException("a plain item should not be resolved as a query"));

        Assert.Equal(new[] { "(O)18" }, ids);
    }

    [Fact]
    public void AnItemQueryGivesEveryItemItCanProduce()
    {
        string? query = null;
        var ids = ForageEntryItems.Resolve(
            "RANDOM_ITEMS (O) 18 20",
            _ => null,
            value =>
            {
                query = value;
                return new[] { "(O)18", " (O)20 ", "(O)18" };
            });

        Assert.Equal("RANDOM_ITEMS (O) 18 20", query);
        Assert.Equal(new[] { "(O)18", "(O)20" }, ids);
    }

    [Fact]
    public void AnItemQueryWithNoResultsNamesNoSource()
    {
        var ids = ForageEntryItems.Resolve("RANDOM_ITEMS (F) 999999", _ => null, _ => Array.Empty<string>());

        Assert.Empty(ids);
    }

    [Fact]
    public void AFailingQueryDoesNotThrow()
    {
        var ids = ForageEntryItems.Resolve("BAD_QUERY", _ => null, _ => throw new InvalidOperationException());

        Assert.Empty(ids);
    }

    [Fact]
    public void ABlankItemNamesNoSource()
    {
        Assert.Empty(ForageEntryItems.Resolve(null, _ => null, _ => Array.Empty<string>()));
        Assert.Empty(ForageEntryItems.Resolve("  ", _ => null, _ => Array.Empty<string>()));
    }
}
