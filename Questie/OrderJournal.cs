using System.Runtime.CompilerServices;
using HarmonyLib;
using StardewModdingAPI;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Quests;
using StardewValley.SpecialOrders;

namespace Questie
{
    /// <summary>
    /// Adds what the farmer carries for a special order, and where to take it, to the end of the order's description in the journal.
    /// The order's own progress bars stay as they are. The journal asks for the description on every frame it is open, so the lines
    /// are reused for a few frames.
    /// </summary>
    internal static class OrderJournal
    {
        /// <summary>How long an order's lines are reused, in game updates.</summary>
        private const int RefreshTicks = 20;

        private sealed class Cached
        {
            // far enough in the past that the first request always builds, and small enough that subtracting it cannot overflow
            public int Tick = -1_000_000;
            public string Text = "";

            /// <summary>The text width the notes were wrapped for, or <see cref="NoWidth"/> when they were not wrapped for the journal.</summary>
            public int Width = NoWidth;

            /// <summary>The line of the whole description, counting from zero, where the notes start. Negative when they are not laid out for the journal.</summary>
            public int FirstNoteLine = -1;
        }

        private const int NoWidth = -1;

        private const string Bullet = "- ";

        private static readonly ConditionalWeakTable<SpecialOrder, Cached> Cache = new();

        private static Func<ModConfig>? _config;
        private static Func<QuestTracker>? _tracker;
        private static GameHintSource? _source;
        private static IMonitor? _monitor;
        private static int _failures;
        private const int MaxLoggedFailures = 3;

        public static void Apply(IMonitor monitor, Func<ModConfig> config, Func<QuestTracker> tracker, GameHintSource source)
        {
            _monitor = monitor;
            _config = config;
            _tracker = tracker;
            _source = source;

            try
            {
                var harmony = new Harmony("Kkthnx.Questie.Orders");
                harmony.Patch(AccessTools.Method(typeof(SpecialOrder), nameof(SpecialOrder.GetDescription)), postfix: new HarmonyMethod(typeof(OrderJournal), nameof(GetDescriptionPostfix)));
                harmony.Patch(AccessTools.Method(typeof(QuestLog), nameof(QuestLog.draw), new[] { typeof(SpriteBatch) }), postfix: new HarmonyMethod(typeof(OrderJournal), nameof(LogDrawn)));
            }
            catch (Exception ex)
            {
                monitor.Log($"Could not add notes to special orders, so they stay as the game makes them. ({ex.Message})", LogLevel.Error);
            }
        }

        /// <summary>Drops every stored line, for example when the language changes.</summary>
        public static void Forget() => Cache.Clear();

        private static void GetDescriptionPostfix(SpecialOrder __instance, ref string __result)
        {
            if (_config is null || _tracker is null || _source is null || !Context.IsWorldReady || !_config().ShowHints || __instance.questState.Value != SpecialOrderStatus.InProgress)
                return;

            var cached = Cache.GetOrCreateValue(__instance);
            var width = Game1.activeClickableMenu is QuestLog log ? log.width - LogRule.TextInset : NoWidth;
            if (Game1.ticks - cached.Tick >= RefreshTicks || Game1.ticks < cached.Tick || width != cached.Width)
            {
                cached.Tick = Game1.ticks;
                cached.Width = width;
                cached.FirstNoteLine = -1;
                try
                {
                    var key = __instance.questKey.Value ?? "";
                    var wants = _tracker().Orders.Where(w => w.OrderKey == key);
                    // the order's own text has no room for pictures, so the names go in as plain text, each note a bullet like in a quest's page
                    var notes = IconText.Strip(string.Join("\n", OrderNotes.Journal(wants, _config(), _source, _source).Select(line => Bullet + line)));
                    if (width != NoWidth && notes.Length > 0)
                    {
                        // wrapped here so a bullet's second line sits under its text, as it does in a quest's page
                        var layout = IconText.Layout(notes, width - LogRule.WrapMargin, line => Game1.dialogueFont.MeasureString(line).X, 0);
                        notes = layout.Text;
                        cached.FirstNoteLine = Game1.parseText(__result + "\n\n" + notes, Game1.dialogueFont, width).Count(c => c == '\n') + 1 - layout.LineCount;
                    }
                    cached.Text = notes;
                }
                catch (Exception ex)
                {
                    // an order from another mod that does not look the way this expects must never break the journal
                    cached.Text = "";
                    if (++_failures <= MaxLoggedFailures)
                        _monitor?.Log($"Could not add notes to the special order '{__instance.questKey.Value}'. ({ex.GetType().Name}: {ex.Message})", LogLevel.Warn);
                }
            }
            if (cached.Text.Length > 0)
                __result = __result + "\n\n" + cached.Text;
        }

        /// <summary>Draws the rule between the order's own text and the notes, once the journal has drawn its page.</summary>
        private static void LogDrawn(QuestLog __instance, SpriteBatch b, IQuest ____shownQuest, int ___questPage)
        {
            if (_config is null || ___questPage == -1 || ____shownQuest is not SpecialOrder order || !Cache.TryGetValue(order, out var cached) || cached.Text.Length == 0 || cached.FirstNoteLine < 1)
                return;
            try
            {
                if (LogRule.Draw(b, __instance, __instance.yPositionOnScreen + 96, cached.FirstNoteLine - 1, cached.Width))
                    __instance.drawMouse(b); // the rule is drawn after the journal put the cursor on the screen, so the cursor goes back on top
            }
            catch (Exception ex)
            {
                _monitor?.Log($"Could not draw the rule on a special order page. ({ex.GetType().Name}: {ex.Message})", LogLevel.Trace);
            }
        }
    }
}
