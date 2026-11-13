using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
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

        private readonly record struct Row(string Text, bool Done, bool Progress, int Need, string? Icon = null, bool Indent = false);

        private readonly record struct Content(string[] Title, List<Row> Rows, int Height);

        /// <summary>The laid out panel, kept for a few frames. Working the text out for every objective on every frame costs far more than drawing it.</summary>
        private sealed class Layout
        {
            public SpecialOrder Order = null!;
            public int Tick;
            public Box Occupied;
            public int ScreenWidth;
            public int ScreenHeight;
            public Box Spot;
            public string[] Title = Array.Empty<string>();
            public List<Row> Rows = new();
            public string More = "";
        }

        /// <summary>How long a layout is kept, in game updates. The counts follow items moved in or out within a tenth of a second.</summary>
        private const int RefreshTicks = 6;

        private Layout? _layout;

        /// <summary>The size an item's picture is drawn at, in pixels, and the room it takes in front of a line.</summary>
        private const int IconSize = 32;

        private const int IconRoom = IconSize + 8;

        private readonly IHintTexts _texts;
        private readonly ItemTagIndex _tags = new();

        private static DonationPanel? _shown;
        private static IMonitor? _monitor;
        private static int _failures;

        public DonationPanel(IHintTexts texts) => _texts = texts;

        /// <summary>
        /// Draws the list just before the menu draws its mouse cursor, so the cursor is not hidden behind the list. The game draws the
        /// cursor at the end of a menu's own drawing, so a prefix of that call is the only place above the menu and below the cursor.
        /// Returns false when the game's drawing could not be hooked, so the caller can fall back to drawing after the menu.
        /// </summary>
        public static bool Apply(IMonitor monitor, DonationPanel panel)
        {
            _monitor = monitor;
            try
            {
                var harmony = new Harmony("Kkthnx.Questie.DonationPanel");
                harmony.Patch(
                    original: AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.drawMouse)),
                    prefix: new HarmonyMethod(typeof(DonationPanel), nameof(BeforeMouse)));
                _shown = panel;
                return true;
            }
            catch (Exception ex)
            {
                monitor.Log($"Could not draw the list beside a donation box under the mouse cursor, so it is drawn after the box instead. ({ex.Message})", LogLevel.Warn);
                return false;
            }
        }

        private static void BeforeMouse(SpriteBatch b)
        {
            if (_shown is null)
                return;
            try
            {
                _shown.Draw(b);
            }
            catch (Exception ex)
            {
                // a menu must never break because of the list beside it
                if (++_failures <= 3)
                    _monitor?.Log($"Could not draw the list beside the donation box. ({ex.GetType().Name}: {ex.Message})", LogLevel.Warn);
            }
        }

        /// <summary>Forgets the items found by tag. Called when the game's item data changes.</summary>
        public void Reset() => _tags.Reset();

        /// <summary>Lets go of the order the panel was showing. Called when a menu opens or closes, since nothing is drawn once there is no menu.</summary>
        public void Release() => _layout = null;

        public void Draw(SpriteBatch batch)
        {
            if (Game1.activeClickableMenu is not QuestContainerMenu menu || Game1.player?.team?.specialOrders is not { } orders)
            {
                _layout = null; // lets go of the order once its box is closed
                return;
            }

            var order = FindOrder(orders, menu);
            if (order is null)
            {
                _layout = null;
                return;
            }

            var occupied = Occupied(menu);
            var screenWidth = Game1.uiViewport.Width;
            var screenHeight = Game1.uiViewport.Height;
            if (_layout is null || !ReferenceEquals(_layout.Order, order) || Game1.ticks - _layout.Tick >= RefreshTicks || Game1.ticks < _layout.Tick
                || _layout.Occupied != occupied || _layout.ScreenWidth != screenWidth || _layout.ScreenHeight != screenHeight)
                _layout = Arrange(order, occupied, screenWidth, screenHeight);

            if (_layout is { Rows.Count: > 0 } ready)
                DrawLayout(batch, ready);
        }

        /// <summary>Works out what the panel says and where it goes.</summary>
        private Layout Arrange(SpecialOrder order, Box occupied, int screenWidth, int screenHeight)
        {
            var layout = new Layout { Order = order, Tick = Game1.ticks, Occupied = occupied, ScreenWidth = screenWidth, ScreenHeight = screenHeight };
            var needs = ReadNeeds(order);
            if (needs.Count == 0)
                return layout;

            var title = _texts.Get("donate.title", ("order", order.GetName()));

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
                return layout;

            layout.Spot = spot;
            layout.Title = content.Title;
            layout.Rows = Fit(content, spot.Height, LineHeight(Game1.smallFont), out var hidden);
            layout.More = hidden > 0 ? _texts.Get("donate.more", ("count", hidden.ToString())) : "";
            return layout;
        }

        private static void DrawLayout(SpriteBatch batch, Layout layout)
        {
            var spot = layout.Spot;
            var font = Game1.smallFont;
            var lineHeight = LineHeight(font);

            IClickableMenu.drawTextureBox(batch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), spot.X, spot.Y, spot.Width, spot.Height, Color.White);

            var cursorY = spot.Y + Padding;
            foreach (var line in layout.Title)
            {
                Utility.drawTextWithShadow(batch, line, Game1.dialogueFont, new Vector2(spot.X + Padding, cursorY), Game1.textColor);
                cursorY += LineHeight(Game1.dialogueFont);
            }
            cursorY += TitleGap;
            foreach (var row in layout.Rows)
            {
                var textX = spot.X + Padding + (row.Indent ? IconRoom : 0);
                if (row.Icon is not null)
                    DrawIcon(batch, row.Icon, new Vector2(spot.X + Padding, cursorY + (lineHeight - IconSize) / 2f));
                Utility.drawTextWithShadow(batch, row.Text, font, new Vector2(textX, cursorY), row.Done ? Color.ForestGreen : row.Progress ? Game1.unselectedOptionColor : Game1.textColor);
                cursorY += lineHeight;
            }
            if (layout.More.Length > 0)
                Utility.drawTextWithShadow(batch, layout.More, font, new Vector2(spot.X + Padding, cursorY), Game1.unselectedOptionColor);
        }

        private static void DrawIcon(SpriteBatch batch, string qualifiedItemId, Vector2 at)
        {
            var data = ItemRegistry.GetDataOrErrorItem(qualifiedItemId);
            var source = data.GetSourceRect();
            batch.Draw(data.GetTexture(), at, source, Color.White, 0f, Vector2.Zero, IconSize / (float)Math.Max(source.Width, source.Height), SpriteEffects.None, 1f);
        }

        private static int LineHeight(SpriteFont font) => (int)font.MeasureString("Ag").Y;

        /// <summary>The title and every need wrapped to a width, and how tall that makes the panel.</summary>
        private Content Compose(string title, IReadOnlyList<DonationNeed> needs, int textWidth)
        {
            var titleLines = Game1.parseText(title, Game1.dialogueFont, textWidth).Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var rows = new List<Row>();
            for (var i = 0; i < needs.Count; i++)
            {
                // a line with an item's picture is indented to make room for it, and the picture sits beside its first row
                var icon = needs[i].IconTag is { } tag ? _tags.Find(tag) : null;
                var indent = icon is not null;
                var first = true;
                foreach (var line in Game1.parseText(needs[i].Text, Game1.smallFont, indent ? textWidth - IconRoom : textWidth).Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    rows.Add(new Row(line, needs[i].Done, false, i, first ? icon : null, indent));
                    first = false;
                }
                var progress = DonationLines.Progress(needs[i], _texts);
                if (progress.Length > 0)
                    rows.Add(new Row(progress, needs[i].Done, true, i, null, indent));
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

        /// <summary>Everything the menu uses, the box, the backpack, the buttons and the frames around them.</summary>
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
                var iconTag = objective is DonateObjective donate ? DonationLines.IconTag(donate.acceptableContextTagSets) : null;
                needs.Add(new DonationNeed(descriptions[i], objective.GetCount(), objective.GetMaxCount(), objective is DonateObjective, iconTag));
            }
            return needs;
        }
    }
}
