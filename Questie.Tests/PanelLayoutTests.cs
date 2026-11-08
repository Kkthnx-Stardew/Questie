namespace Questie.Tests;

public class PanelLayoutTests
{
    // a menu about as wide as the real one: a box over a backpack, in the middle of a 2560 by 1440 screen
    private static readonly Box Menu = new(660, 380, 800, 520);

    private static Box Place(int screenWidth, int screenHeight, int wanted = 440, int min = 240, int height = 600, Box? menu = null) =>
        PanelLayout.Place(menu ?? Menu, screenWidth, screenHeight, wanted, min, height).Box;

    [Fact]
    public void GoesLeftWhenThereIsRoom()
    {
        var spot = Place(2560, 1440);
        Assert.Equal(440, spot.Width);
        Assert.True(spot.Right <= Menu.X);
        Assert.False(spot.Overlaps(Menu));
    }

    [Fact]
    public void GoesRightWhenTheLeftIsTight()
    {
        var menu = new Box(300, 380, 800, 520);
        var spot = Place(1920, 1080, menu: menu);
        Assert.True(spot.X >= menu.Right);
        Assert.False(spot.Overlaps(menu));
    }

    [Fact]
    public void IsSqueezedBesideTheMenuBeforeItCoversAnything()
    {
        var menu = new Box(300, 380, 800, 520);
        var spot = Place(1500, 1080, menu: menu);
        Assert.InRange(spot.Width, 240, 439);
        Assert.False(spot.Overlaps(menu));
    }

    [Fact]
    public void GoesAboveOrBelowWhenTheSidesAreTooNarrow()
    {
        var menu = new Box(100, 500, 1080, 520);
        var spot = Place(1280, 1080, menu: menu, height: 300);
        Assert.False(spot.Overlaps(menu));
        Assert.True(spot.Y >= 0 && spot.Bottom <= 1080);
    }

    [Fact]
    public void ACutDownHeightFitsTheRoomItWasGiven()
    {
        var menu = new Box(100, 500, 1080, 520);
        var spot = Place(1280, 1080, menu: menu, height: 900);
        Assert.False(spot.Overlaps(menu));
        Assert.True(spot.Height < 900);
    }

    [Fact]
    public void ATallPanelStaysOnTheScreen()
    {
        var spot = Place(2560, 1440, height: 5000);
        Assert.True(spot.Y >= PanelLayout.Margin);
        Assert.True(spot.Bottom <= 1440 - PanelLayout.Margin);
    }

    [Fact]
    public void BoxesKnowTheirUnionAndOverlap()
    {
        var a = new Box(0, 0, 10, 10);
        var b = new Box(20, 5, 10, 10);
        Assert.Equal(new Box(0, 0, 30, 15), a.Union(b));
        Assert.False(a.Overlaps(b));
        Assert.True(a.Overlaps(new Box(5, 5, 10, 10)));
    }
}
