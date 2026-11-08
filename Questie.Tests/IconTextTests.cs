namespace Questie.Tests;

public class IconTextTests
{
    // every character is ten pixels wide, so the lengths in the tests can be counted by hand
    private static float Measure(string s) => s.Length * 10f;

    private static string Npc(string name) => IconText.Mark(IconKind.Npc, name, name);

    private static string Item(string id, string name) => IconText.Mark(IconKind.Item, id, name);

    [Fact]
    public void StripLeavesThePlainNames()
    {
        Assert.Equal("Bring 2 to Lewis.", IconText.Strip("Bring 2 to " + Npc("Lewis") + "."));
        Assert.Equal("no markers here", IconText.Strip("no markers here"));
    }

    [Fact]
    public void ANameGetsRoomForItsPictureAndKeepsItsFullStop()
    {
        var layout = IconText.Layout("Go to " + Npc("Lewis") + ".", 1000, Measure, 30);
        // three spaces of room after the joining space, then the name, then the full stop right after it
        Assert.Equal("Go to    Lewis.", layout.Text);
        var spot = Assert.Single(layout.Spots);
        Assert.Equal(new IconSpot(0, 60f, IconKind.Npc, "Lewis"), spot);
    }

    [Fact]
    public void ANameWithSpacesStaysOnOneLine()
    {
        var text = "Needs " + Item("(O)1", "Dwarf Scroll I");
        var layout = IconText.Layout(text, 140, Measure, 30);
        Assert.Equal(2, layout.LineCount);
        Assert.Equal("Needs\n   Dwarf Scroll I", layout.Text);
        Assert.Equal(1, layout.Spots[0].Line);
        Assert.Equal(0f, layout.Spots[0].X);
    }

    [Fact]
    public void LongTextWrapsWithinTheWidthAndTheSpotFollowsTheName()
    {
        var text = "one two three four " + Npc("Abigail");
        var layout = IconText.Layout(text, 150, Measure, 30);
        foreach (var line in layout.Text.Split('\n'))
            Assert.True(Measure(line) <= 150, line);
        var spot = Assert.Single(layout.Spots);
        var lines = layout.Text.Split('\n');
        Assert.EndsWith("Abigail", lines[spot.Line]);
        // the picture starts where the room for it starts, ten characters (three of room and seven of name) from the end
        Assert.Equal(Measure(lines[spot.Line][..^10]), spot.X, 0.01);
    }

    [Fact]
    public void LinesTheTextAlreadyBreaksIntoStay()
    {
        var layout = IconText.Layout("first\n\nthird", 1000, Measure, 30);
        Assert.Equal("first\n\nthird", layout.Text);
        Assert.Equal(3, layout.LineCount);
        Assert.Empty(layout.Spots);
    }

    [Fact]
    public void SeveralNamesEachGetASpot()
    {
        var layout = IconText.Layout(Npc("Abigail") + ", " + Npc("Sam") + " and " + Npc("Leah"), 1000, Measure, 30);
        Assert.Equal(new[] { "Abigail", "Sam", "Leah" }, layout.Spots.Select(s => s.Id));
        Assert.True(layout.Spots[0].X < layout.Spots[1].X && layout.Spots[1].X < layout.Spots[2].X);
    }
}
