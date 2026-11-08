using System.Runtime.CompilerServices;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
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
        }

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
            if (Game1.ticks - cached.Tick >= RefreshTicks || Game1.ticks < cached.Tick)
            {
                cached.Tick = Game1.ticks;
                try
                {
                    var key = __instance.questKey.Value ?? "";
                    var wants = _tracker().Orders.Where(w => w.OrderKey == key);
                    // the order's own text has no room for pictures, so the names go in as plain text
                    cached.Text = IconText.Strip(string.Join("\n", OrderNotes.Journal(wants, _config(), _source, _source)));
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
    }
}
