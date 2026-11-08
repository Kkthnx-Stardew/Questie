using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.SpecialOrders;
using StardewValley.SpecialOrders.Objectives;

namespace Questie
{
    /// <summary>
    /// Beside the box a special order takes donations in, lists the order's objectives and how far each has come, so there is no need to close the box and
    /// open the quest log to find out. The game keeps the counts up to date as items go in and come out.
    /// </summary>
    internal sealed class DonationPanel
    {
        private const int Padding = 20;
        private const int Gap = 24;
        private const int TextWidth = 380;
        private const int MaxNeeds = 12;

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

            var font = Game1.smallFont;
            var lineHeight = (int)font.MeasureString("Ag").Y;
            var title = _texts.Get("donate.title", ("order", order.GetName()));

            // work out the lines first so the box can be sized to them
            var rows = new List<(string Text, bool Done, bool Progress)>();
            foreach (var need in needs.Take(MaxNeeds))
            {
                foreach (var line in Game1.parseText(need.Text, font, TextWidth).Split('\n', StringSplitOptions.RemoveEmptyEntries))
                    rows.Add((line, need.Done, false));
                var progress = DonationLines.Progress(need, _texts);
                if (progress.Length > 0)
                    rows.Add((progress, need.Done, true));
            }

            var titleLines = Game1.parseText(title, Game1.dialogueFont, TextWidth).Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var titleHeight = (int)(Game1.dialogueFont.MeasureString("Ag").Y * titleLines.Length);
            var width = TextWidth + Padding * 2;
            var height = Padding * 2 + titleHeight + 8 + rows.Count * lineHeight;

            var grab = menu.ItemsToGrabMenu;
            var x = grab.xPositionOnScreen - width - Gap;
            if (x < 16)
                x = grab.xPositionOnScreen + grab.width + Gap; // no room on the left, so the right
            var y = Math.Max(16, grab.yPositionOnScreen);
            x = Math.Clamp(x, 16, Math.Max(16, Game1.uiViewport.Width - width - 16));

            IClickableMenu.drawTextureBox(batch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, width, height, Color.White);

            var cursorY = y + Padding;
            foreach (var line in titleLines)
            {
                Utility.drawTextWithShadow(batch, line, Game1.dialogueFont, new Vector2(x + Padding, cursorY), Game1.textColor);
                cursorY += (int)Game1.dialogueFont.MeasureString("Ag").Y;
            }
            cursorY += 8;
            foreach (var (text, done, progress) in rows)
            {
                var color = done ? Color.ForestGreen : progress ? Game1.unselectedOptionColor : Game1.textColor;
                Utility.drawTextWithShadow(batch, text, font, new Vector2(x + Padding, cursorY), color);
                cursorY += lineHeight;
            }
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
