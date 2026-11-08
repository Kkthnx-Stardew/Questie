using System.Text;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Questie
{
    /// <summary>
    /// Marks the items in the backpack, in a chest and on the toolbar that a quest wants, with the game's own quest exclamation mark
    /// in the top left corner of the slot (the quality star is in the bottom left), and adds a line to the item's tooltip that says
    /// which quest wants it.
    /// </summary>
    internal static class ItemBadges
    {
        /// <summary>The yellow exclamation mark the game shows beside a new quest.</summary>
        private static readonly Rectangle Icon = new(403, 496, 5, 14);

        private static readonly Vector2[] Outline = { new(-1f, 0f), new(1f, 0f), new(0f, -1f), new(0f, 1f) };

        private const float IconScale = 2f;
        private const int Margin = 6;
        private const int TooltipWidth = 272; // the width the game wraps an item's own description to
        private const int RefreshTicks = 20;

        private static Func<ModConfig>? _config;
        private static Func<QuestTracker>? _tracker;
        private static GameHintSource? _source;
        private static IMonitor? _monitor;

        // The tooltip text is worked out a few times a second at most while the mouse rests on an item.
        private static string _noteItem = "";
        private static int _noteTick = int.MinValue / 2;
        private static string _note = "";

        public static void Apply(IMonitor monitor, Func<ModConfig> config, Func<QuestTracker> tracker, GameHintSource source)
        {
            _monitor = monitor;
            _config = config;
            _tracker = tracker;
            _source = source;

            try
            {
                var harmony = new Harmony("Kkthnx.Questie.Badges");
                harmony.Patch(AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) }),
                    postfix: new HarmonyMethod(typeof(ItemBadges), nameof(InventoryDrawn)));
                harmony.Patch(AccessTools.Method(typeof(Toolbar), nameof(Toolbar.draw), new[] { typeof(SpriteBatch) }),
                    postfix: new HarmonyMethod(typeof(ItemBadges), nameof(ToolbarDrawn)));
                harmony.Patch(AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.drawHoverText), new[]
                    {
                        typeof(SpriteBatch), typeof(StringBuilder), typeof(SpriteFont), typeof(int), typeof(int), typeof(int), typeof(string), typeof(int), typeof(string[]), typeof(Item),
                        typeof(int), typeof(string), typeof(int), typeof(int), typeof(int), typeof(float), typeof(CraftingRecipe), typeof(IList<Item>), typeof(Texture2D),
                        typeof(Rectangle?), typeof(Color?), typeof(Color?), typeof(float), typeof(int), typeof(int)
                    }),
                    prefix: new HarmonyMethod(typeof(ItemBadges), nameof(AddNote)));
            }
            catch (Exception ex)
            {
                monitor.Log($"Could not mark quest items, so item icons and tooltips stay as the game makes them. ({ex.Message})", LogLevel.Error);
            }
        }

        private static bool Active(out HashSet<string> wanted)
        {
            wanted = null!;
            if (_config is null || _tracker is null || !Context.IsWorldReady || !_config().ShowItemMarks)
                return false;
            wanted = _tracker().WantedItems;
            return wanted.Count > 0;
        }

        private static void InventoryDrawn(InventoryMenu __instance, SpriteBatch b)
        {
            if (!Active(out var wanted))
                return;
            var items = __instance.actualInventory;
            var slots = __instance.inventory;
            for (var i = 0; i < slots.Count && i < items.Count; i++)
            {
                if (items[i] is { } item && wanted.Contains(item.QualifiedItemId))
                    DrawIcon(b, slots[i].bounds, 1f);
            }
        }

        private static void ToolbarDrawn(Toolbar __instance, SpriteBatch b)
        {
            if (Game1.activeClickableMenu is not null || !Active(out var wanted))
                return;
            var items = Game1.player.Items;
            var buttons = __instance.buttons;
            for (var i = 0; i < buttons.Count && i < items.Count; i++)
            {
                if (items[i] is { } item && wanted.Contains(item.QualifiedItemId))
                    DrawIcon(b, buttons[i].bounds, __instance.transparency);
            }
        }

        private static void DrawIcon(SpriteBatch b, Rectangle slot, float alpha)
        {
            var at = new Vector2(slot.X + Margin, slot.Y + Margin);
            // a dark outline one picture pixel wide keeps the yellow readable on the tan slot and on bright items
            foreach (var step in Outline)
                b.Draw(Game1.mouseCursors, at + step * IconScale, Icon, Color.Black * (0.55f * alpha), 0f, Vector2.Zero, IconScale, SpriteEffects.None, 0.99f);
            b.Draw(Game1.mouseCursors, at, Icon, Color.White * alpha, 0f, Vector2.Zero, IconScale, SpriteEffects.None, 1f);
        }

        /// <summary>Adds the quest lines to the end of an item's tooltip text. The game hands the same builder to the call that draws it, so it is checked first.</summary>
        private static void AddNote(StringBuilder text, Item hoveredItem)
        {
            if (hoveredItem is null || !Active(out var wanted) || !wanted.Contains(hoveredItem.QualifiedItemId))
                return;

            try
            {
                var id = hoveredItem.QualifiedItemId;
                if (id != _noteItem || Game1.ticks - _noteTick >= RefreshTicks || Game1.ticks < _noteTick)
                {
                    _noteItem = id;
                    _noteTick = Game1.ticks;
                    var lines = ItemNeeds.Notes(id, _tracker!().Facts, _source!, _source!);
                    _note = lines.Count == 0 ? "" : string.Join("\n", lines.Select(line => Game1.parseText(line, Game1.smallFont, TooltipWidth).TrimEnd()));
                }
                if (_note.Length == 0)
                    return;

                var current = text.ToString();
                if (!current.EndsWith(_note, StringComparison.Ordinal))
                    text.Append(current.Length == 0 ? "" : "\n\n").Append(_note);
            }
            catch (Exception ex)
            {
                _monitor?.Log($"Could not add the quest line to a tooltip. ({ex.GetType().Name}: {ex.Message})", LogLevel.Trace);
            }
        }
    }
}
