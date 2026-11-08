namespace Questie
{
    /// <summary>A rectangle in screen pixels. Kept apart from the game's own type so the tests can compile this file.</summary>
    public readonly record struct Box(int X, int Y, int Width, int Height)
    {
        public int Right => X + Width;

        public int Bottom => Y + Height;

        /// <summary>The smallest box that holds both.</summary>
        public Box Union(Box other)
        {
            var x = Math.Min(X, other.X);
            var y = Math.Min(Y, other.Y);
            return new Box(x, y, Math.Max(Right, other.Right) - x, Math.Max(Bottom, other.Bottom) - y);
        }

        public Box Grow(int by) => new(X - by, Y - by, Width + by * 2, Height + by * 2);

        public bool Overlaps(Box other) => X < other.Right && other.X < Right && Y < other.Bottom && other.Y < Bottom;
    }

    /// <summary>Where a panel goes, and how much room it has there.</summary>
    /// <param name="Box">The panel's place and size. Its height is cut down when the screen has no room for all of it.</param>
    public readonly record struct PanelSpot(Box Box);

    /// <summary>
    /// Finds room for a side panel next to a menu without covering any of it. A panel that sits on top of a menu hides slots the player
    /// needs, so it goes left or right of everything the menu uses, narrower if need be, and above or below when the sides are too tight.
    /// </summary>
    public static class PanelLayout
    {
        public const int Margin = 16;
        public const int Gap = 24;

        /// <param name="menu">Everything the menu occupies, frames and buttons included.</param>
        /// <param name="screenWidth">The width of the screen the panel is drawn on.</param>
        /// <param name="screenHeight">The height of that screen.</param>
        /// <param name="wantedWidth">The width the panel would like.</param>
        /// <param name="minWidth">The narrowest it may be squeezed to beside the menu.</param>
        /// <param name="height">The height of the panel at that width.</param>
        public static PanelSpot Place(Box menu, int screenWidth, int screenHeight, int wantedWidth, int minWidth, int height)
        {
            var tallest = Math.Max(0, screenHeight - Margin * 2);
            var sideHeight = Math.Min(height, tallest);
            var sideY = Math.Clamp(menu.Y, Margin, Math.Max(Margin, screenHeight - Margin - sideHeight));

            var left = menu.X - Gap - Margin;
            var right = screenWidth - menu.Right - Gap - Margin;

            if (left >= wantedWidth)
                return new PanelSpot(new Box(menu.X - Gap - wantedWidth, sideY, wantedWidth, sideHeight));
            if (right >= wantedWidth)
                return new PanelSpot(new Box(menu.Right + Gap, sideY, wantedWidth, sideHeight));

            // narrower beside the menu, on the side with more room
            if (Math.Max(left, right) >= minWidth)
            {
                return left >= right
                    ? new PanelSpot(new Box(menu.X - Gap - left, sideY, left, sideHeight))
                    : new PanelSpot(new Box(menu.Right + Gap, sideY, right, sideHeight));
            }

            // no room at the sides, so above or below, wherever there is more
            var width = Math.Min(wantedWidth, Math.Max(0, screenWidth - Margin * 2));
            var x = Math.Clamp(menu.X + menu.Width / 2 - width / 2, Margin, Math.Max(Margin, screenWidth - Margin - width));
            var above = menu.Y - Gap - Margin;
            var below = screenHeight - menu.Bottom - Gap - Margin;
            if (above >= below)
            {
                var h = Math.Min(height, Math.Max(0, above));
                return new PanelSpot(new Box(x, menu.Y - Gap - h, width, h));
            }
            var hBelow = Math.Min(height, Math.Max(0, below));
            return new PanelSpot(new Box(x, menu.Bottom + Gap, width, hBelow));
        }
    }
}
