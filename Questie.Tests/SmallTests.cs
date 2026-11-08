using System.Text.Json;

namespace Questie.Tests;

public class SocializeRepairTests
{
    private static IReadOnlySet<string> Set(params string[] names) => new HashSet<string>(names);

    [Fact]
    public void RemovesPeopleWhoAreNoLongerSociable()
    {
        var changes = SocializeRepair.Plan(new[] { "Abigail", "Gone", "Alex" }, Set("Abigail", "Alex"), new[] { "Abigail", "Alex" }, _ => false);
        Assert.Equal(new[] { "Gone" }, changes.Remove);
        Assert.Empty(changes.Add);
        Assert.Equal(-1, changes.TotalChange);
    }

    [Fact]
    public void AddsNewSociablePeopleWhoHaveNotBeenMet()
    {
        var changes = SocializeRepair.Plan(new[] { "Abigail" }, Set("Abigail", "Newcomer", "MetAlready"), new[] { "Abigail", "Newcomer", "MetAlready" }, name => name == "MetAlready");
        Assert.Equal(new[] { "Newcomer" }, changes.Add);
        Assert.Equal(1, changes.TotalChange);
    }

    [Fact]
    public void ChangesNothingWhenTheQuestMatches()
    {
        var changes = SocializeRepair.Plan(new[] { "Abigail" }, Set("Abigail"), new[] { "Abigail" }, _ => false);
        Assert.False(changes.Any);
        Assert.Equal(0, changes.TotalChange);
    }

    [Fact]
    public void ToleratesRepeatedNamesInTheSource()
    {
        var changes = SocializeRepair.Plan(Array.Empty<string>(), Set("Newcomer"), new[] { "Newcomer", "Newcomer" }, _ => false);
        Assert.Equal(new[] { "Newcomer" }, changes.Add);
    }
}

public class ModConfigTests
{
    [Fact]
    public void EverythingIsOnByDefault()
    {
        var config = new ModConfig();
        foreach (var property in typeof(ModConfig).GetProperties().Where(p => p.PropertyType == typeof(bool)))
            Assert.True((bool)property.GetValue(config)!, property.Name);
    }

    [Fact]
    public void CopyFromCopiesEverySetting()
    {
        var changed = new ModConfig();
        foreach (var property in typeof(ModConfig).GetProperties().Where(p => p.PropertyType == typeof(bool)))
            property.SetValue(changed, false);
        var target = new ModConfig();
        target.CopyFrom(changed);
        foreach (var property in typeof(ModConfig).GetProperties())
            Assert.Equal(property.GetValue(changed), property.GetValue(target));
    }
}

/// <summary>Checks the files that ship with the mod: the manifest and the translations.</summary>
public class ShippedFilesTests
{
    private static string ProjectDir()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "Questie.sln")))
            dir = Path.GetDirectoryName(dir);
        Assert.NotNull(dir);
        return Path.Combine(dir!, "Questie");
    }

    [Fact]
    public void ManifestHasTheRequiredFields()
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(ProjectDir(), "manifest.json")));
        var root = doc.RootElement;
        Assert.Equal("Kkthnx.Questie", root.GetProperty("UniqueID").GetString());
        Assert.Equal("Questie.dll", root.GetProperty("EntryDll").GetString());
        Assert.Equal("%ProjectVersion%", root.GetProperty("Version").GetString());
        Assert.True(Version.TryParse(root.GetProperty("MinimumGameVersion").GetString(), out var min) && min >= new Version(1, 6, 9));
    }

    [Fact]
    public void EveryHintKeyTheCodeAsksForHasATranslation()
    {
        var keys = Keys(Path.Combine(ProjectDir(), "i18n", "default.json"));
        var asked = new[]
        {
            "hint.slain", "hint.caught", "hint.collected", "hint.greeted", "hint.carry", "hint.return", "hint.where", "hint.fish.places",
            "hint.fish.time", "hint.fish.sunny", "hint.fish.rainy", "hint.craft.known", "hint.craft.unknown", "hint.cook.known",
            "hint.cook.unknown", "hint.left", "hint.more", "hint.here", "hint.lost", "hint.fish.trap", "hint.fish.everywhere",
            "hint.unlock.friendship", "hint.unlock.skill", "hint.unlock.other", "hint.go", "hint.fish.now", "hint.fish.notime", "hint.fish.noweather", "hint.sold", "hint.sold.sometimes", "hint.carry.chests", "gmcm.nameicons.name", "gmcm.nameicons.tip", "message.expiring", "note.deliver", "note.progress", "note.lost", "gmcm.itemmarks.name", "gmcm.itemmarks.tip", "donate.title", "donate.progress", "donate.done", "donate.more", "gmcm.donations.name", "gmcm.donations.tip",
        };
        foreach (var key in asked)
            Assert.Contains(key, keys);
    }

    [Fact]
    public void NoTranslationValueIsBlank()
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(ProjectDir(), "i18n", "default.json")));
        foreach (var entry in doc.RootElement.EnumerateObject())
            Assert.False(string.IsNullOrWhiteSpace(entry.Value.GetString()), entry.Name);
    }

    private static HashSet<string> Keys(string file)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(file));
        return doc.RootElement.EnumerateObject().Select(p => p.Name).ToHashSet();
    }
}

public class DonationLinesTests
{
    private sealed class Texts : IHintTexts
    {
        public string Get(string key, params (string Name, string Value)[] tokens) => key + ":" + string.Join(",", tokens.Select(t => t.Name + "=" + t.Value));
    }

    [Fact]
    public void ShowsWhatIsLeft()
    {
        Assert.Equal("donate.progress:have=12,need=40,left=28", DonationLines.Progress(new DonationNeed("Amethyst", 12, 40), new Texts()));
    }

    [Fact]
    public void SaysDoneWhenEnoughIsIn()
    {
        Assert.Equal("donate.done:", DonationLines.Progress(new DonationNeed("Amethyst", 40, 40), new Texts()));
    }

    [Fact]
    public void AYesOrNoNeedSaysNothingUntilDone()
    {
        Assert.Equal("", DonationLines.Progress(new DonationNeed("Gem", 0, 1), new Texts()));
        Assert.Equal("donate.done:", DonationLines.Progress(new DonationNeed("Gem", 1, 1), new Texts()));
    }

    [Fact]
    public void AStepWithoutACountSaysNothing()
    {
        var step = new DonationNeed("Gather 24 eggs.", 0, 24, false);
        Assert.Equal("", DonationLines.Progress(step, new Texts()));
        Assert.False(step.Done);
    }
}
