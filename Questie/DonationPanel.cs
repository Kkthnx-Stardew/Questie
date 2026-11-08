using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.SpecialOrders;
using StardewValley.SpecialOrders.Objectives;

namespace Questie
{
    /// <summary>
    /// Beside the box a special order takes donations in, lists the order's objectives and how far each has come, so there is no need
    /// to close the box and open the quest log to find out. The game keeps the counts up to date as items go in and come out. The
    /// panel never covers the box or the backpack, because the player needs every slot of both.
    /// </summary>
    internal sealed class DonationPanel
    {
        private const int Padding = 20;
        private const int TextWidth = 380;
        private const int MinTextWidth = 200;
        private const int TitleGap = 8;

        /// <summary>The frames and the margin around the slots of a menu, which the panel stays clear of too.</summary>
        private const int FrameAllowance = 32;

        private readonly record struct Row(string Text, bool Done, bool Progress, int Need);

        private readonly record struct Content(string[] Title, List<Row> Rows, int Height);

        private readonly IHintTexts _texts;

        public DonationPanel(IHintTexts texts) => _texts = texts;

        public void Draw(SpriteBatch batch)
        {
            if (Game1.activeClickableMenu is not QuestContainerMenu menu || Game1.player?.team?.specialOrders is not { } orders)
                return;

            var order = FindOrder(orders, menu);
            if (order is null)
                return;

            var needs = ReadNeeds(order);
            if (needs.Count == 0)
                return;

            var title = _texts.Get("donate.title", ("order", order.GetName()));
            var occupied = Occupied(menu);
            var screenWidth = Game1.uiViewport.Width;
            var screenHeight = Game1.uiViewport.Height;

            var wanted = TextWidth + Padding * 2;
            var content = Compose(title, needs, TextWidth);
            var spot = PanelLayout.Place(occupied, screenWidth, screenHeight, wanted, MinTextWidth + Padding * 2, content.Height).Box;
            if (spot.Width < wanted)
            {
                // squeezed beside the menu, so the lines wrap again at the width there is
                content = Compose(title, needs, spot.Width - Padding * 2);
                spot = PanelLayout.Place(occupied, screenWidth, screenHeight, spot.Width, spot.Width, content.Height).Box;
            }
            if (spot.Width <= Padding * 2 || spot.Height <= Padding * 2)
                return;

            var font = Game1.smallFont;
            var lineHeight = LineHeight(font);
            var rows = Fit(content, spot.Height, lineHeight, out var hidden);

            IClickableMenu.drawTextureBox(batch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), spot.X, spot.Y, spot.Width, spot.Height, Color.White);

            var cursorY = spot.Y + Padding;
            foreach (var line in content.Title)
            {
                Utility.drawTextWithShadow(batch, line, Game1.dialogueFont, new Vector2(spot.X + Padding, cursorY), Game1.textColor);
                cursorY += LineHeight(Game1.dialogueFont);
            }
            cursorY += TitleGap;
            foreach (var row in rows)
            {
                Utility.drawTextWithShadow(batch, row.Text, font, new Vector2(spot.X + Padding, cursorY), row.Done ? Color.ForestGreen : row.Progress ? Game1.unselectedOptionColor : Game1.textColor);
                cursorY += lineHeight;
            }
            if (hidden > 0)
                Utility.drawTextWithShadow(batch, _texts.Get("donate.more", ("count", hidden.ToString())), font, new Vector2(spot.X + Padding, cursorY), Game1.unselectedOptionColor);
        }

        private static int LineHeight(SpriteFont font) => (int)font.MeasureString("Ag").Y;

        /// <summary>The title and every need wrapped to a width, and how tall that makes the panel.</summary>
        private Content Compose(string title, IReadOnlyList<DonationNeed> needs, int textWidth)
        {
            var titleLines = Game1.parseText(title, Game1.dialogueFont, textWidth).Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var rows = new List<Row>();
            for (var i = 0; i < needs.Count; i++)
            {
                foreach (var line in Game1.parseText(needs[i].Text, Game1.smallFont, textWidth).Split('\n', StringSplitOptions.RemoveEmptyEntries))
                    rows.Add(new Row(line, needs[i].Done, false, i));
                var progress = DonationLines.Progress(needs[i], _texts);
                if (progress.Length > 0)
                    rows.Add(new Row(progress, needs[i].Done, true, i));
            }
            var height = Padding * 2 + titleLines.Length * LineHeight(Game1.dialogueFont) + TitleGap + rows.Count * LineHeight(Game1.smallFont);
            return new Content(titleLines, rows, height);
        }

        /// <summary>
        /// The rows that fit in the height there is, whole needs only. When some do not fit, the last line is kept for a count of the
        /// needs that were left out.
        /// </summary>
        private static List<Row> Fit(Content content, int height, int lineHeight, out int hiddenNeeds)
        {
            hiddenNeeds = 0;
            var room = height - Padding * 2 - content.Title.Length * LineHeight(Game1.dialogueFont) - TitleGap;
            var fit = Math.Max(0, room / lineHeight);
            if (content.Rows.Count <= fit)
                return content.Rows;

            var keep = Math.Max(0, fit - 1);
            // a need cut in half is dropped whole, so every need shown can be read to its end
            while (keep > 0 && content.Rows[keep - 1].Need == content.Rows[keep].Need)
                keep--;
            hiddenNeeds = content.Rows.Skip(keep).Select(r => r.Need).Distinct().Count();
            return content.Rows.Take(keep).ToList();
        }

        /// <summary>Everything the menu uses: the box, the backpack, the buttons, and the frames around them.</summary>
        private static Box Occupied(QuestContainerMenu menu)
        {
            Box? union = null;
            void Add(Rectangle r) => union = union is { } u ? u.Union(new Box(r.X, r.Y, r.Width, r.Height)) : new Box(r.X, r.Y, r.Width, r.Height);

            foreach (var slot in menu.ItemsToGrabMenu.inventory)
                Add(slot.bounds);
            foreach (var slot in menu.inventory.inventory)
                Add(slot.bounds);
            if (menu.okButton is not null)
                Add(menu.okButton.bounds);
            if (menu.trashCan is not null)
                Add(menu.trashCan.bounds);

            return (union ?? new Box(0, 0, 0, 0)).Grow(FrameAllowance);
        }

        /// <summary>The order whose donated items this menu shows. The menu is given the order's own list, so it is the same object.</summary>
        private static SpecialOrder? FindOrder(IEnumerable<SpecialOrder> orders, QuestContainerMenu menu)
        {
            foreach (var order in orders)
            {
                if (ReferenceEquals(order.donatedItems, menu.ItemsToGrabMenu.actualInventory))
                    return order;
            }
            return null;
        }

        private static List<DonationNeed> ReadNeeds(SpecialOrder order)
        {
            var needs = new List<DonationNeed>();
            var descriptions = order.GetObjectiveDescriptions();
            for (var i = 0; i < order.objectives.Count && i < descriptions.Count; i++)
            {
                // every objective is listed so the item is named ("Gather 24 eggs"), but only the donation counts what is in the box
                var objective = order.objectives[i];
                needs.Add(new DonationNeed(descriptions[i], objective.GetCount(), objective.GetMaxCount(), objective is DonateObjective));
            }
            return needs;
        }
    }
}
