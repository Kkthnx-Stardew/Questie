using System.Runtime.CompilerServices;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Quests;

namespace Questie
{
    /// <summary>
    /// Adds progress and help lines to the end of a quest's description. The quest log asks for the description on every frame it is
    /// open, so the lines are worked out at most a few times a second for each quest and reused in between.
    /// </summary>
    internal static class QuestHints
    {
        /// <summary>How long a quest's lines are reused, in game updates. A third of a second is quick enough to follow a catch or a kill.</summary>
        private const int RefreshTicks = 20;

        private sealed class Cached
        {
            // far enough in the past that the first request always builds, and small enough that subtracting it cannot overflow
            public int Tick = -1_000_000;
            public string Text = "";
        }

        private static readonly ConditionalWeakTable<Quest, Cached> Cache = new();

        private static Func<ModConfig>? _config;
        private static GameHintSource? _source;
        private static IMonitor? _monitor;
        private static bool _failed;
        private static bool _reportedFirstCall;

        public static void Apply(IMonitor monitor, Func<ModConfig> config, GameHintSource source)
        {
            _monitor = monitor;
            _config = config;
            _source = source;

            try
            {
                new Harmony("Kkthnx.Questie").Patch(
                    original: AccessTools.Method(typeof(Quest), nameof(Quest.GetDescription)),
                    postfix: new HarmonyMethod(typeof(QuestHints), nameof(GetDescriptionPostfix)));
            }
            catch (Exception ex)
            {
                monitor.Log($"Could not add notes to the quest log, so the quest log stays as the game makes it. ({ex.Message})", LogLevel.Error);
            }
        }

        /// <summary>Drops every stored line, for example when the language changes.</summary>
        public static void Forget() => Cache.Clear();

        private static void GetDescriptionPostfix(Quest __instance, ref string __result)
        {
            if (!_reportedFirstCall)
            {
                _reportedFirstCall = true;
                _monitor?.Log("The quest log asked for a description, so the notes are in place.", LogLevel.Trace);
            }

            if (_failed || _config is null || _source is null || !Context.IsWorldReady || !_config().ShowHints || __instance.completed.Value)
                return;

            try
            {
                var cached = Cache.GetOrCreateValue(__instance);
                if (Game1.ticks - cached.Tick >= RefreshTicks || Game1.ticks < cached.Tick)
                {
                    cached.Text = Build(__instance);
                    cached.Tick = Game1.ticks;
                }
                if (cached.Text.Length > 0)
                    __result = __result + "\n\n" + cached.Text;
            }
            catch (Exception ex)
            {
                // a quest from another mod that does not look the way this expects must never break the quest log
                _failed = true;
                _monitor?.Log($"Could not add notes to a quest, so notes are off until the game restarts. ({ex.Message})", LogLevel.Error);
            }
        }

        private static string Build(Quest quest)
        {
            if (QuestReader.ToFact(quest) is not { } fact)
                return "";
            return string.Join("\n", HintBuilder.Build(fact, _config!(), _source!, _source!));
        }
    }
}
