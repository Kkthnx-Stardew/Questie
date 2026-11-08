using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Quests;

namespace Questie
{
    /// <summary>
    /// Adds progress and help lines to the end of a quest's description. The quest log asks for the description on every frame it is
    /// open, so the lines are worked out at most a few times a second for each quest and reused in between. In the quest log the
    /// names in the lines get their portrait or item picture in front, drawn over room the text leaves for them.
    /// </summary>
    internal static class QuestHints
    {
        /// <summary>How long a quest's lines are reused, in game updates. A third of a second is quick enough to follow a catch or a kill.</summary>
        private const int RefreshTicks = 20;

        /// <summary>The size a picture is drawn at, in pixels. The room left for it is a little wider.</summary>
        private const int IconSize = 32;

        private const int IconRoom = IconSize + 4;

        /// <summary>The game wraps the description a little differently from how the lines here are measured, so they are kept this much narrower.</summary>
        private const int WrapMargin = 24;

        /// <summary>The quest log's own margins around the description, which its text width is the log's width less.</summary>
        private const int LogTextInset = 128;

        private const int NoWidth = -1;

        private sealed class Cached
        {
            // far enough in the past that the first request always builds, and small enough that subtracting it cannot overflow
            public int Tick = -1_000_000;
            public string Text = "";
            public IReadOnlyList<IconSpot> Spots = Array.Empty<IconSpot>();
            public int LineCount;

            /// <summary>The text width the lines were laid out for, or <see cref="NoWidth"/> when they carry no pictures.</summary>
            public int Width = NoWidth;
        }

        private static readonly ConditionalWeakTable<Quest, Cached> Cache = new();

        private static Func<ModConfig>? _config;
        private static GameHintSource? _source;
        private static IMonitor? _monitor;
        private const int MaxLoggedFailures = 3;
        private static int _failures;
        private static bool _reportedFirstCall;

        public static void Apply(IMonitor monitor, Func<ModConfig> config, GameHintSource source)
        {
            _monitor = monitor;
            _config = config;
            _source = source;

            try
            {
                var harmony = new Harmony("Kkthnx.Questie");
                harmony.Patch(
                    original: AccessTools.Method(typeof(Quest), nameof(Quest.GetDescription)),
                    postfix: new HarmonyMethod(typeof(QuestHints), nameof(GetDescriptionPostfix)));
                harmony.Patch(
                    original: AccessTools.Method(typeof(QuestLog), nameof(QuestLog.draw), new[] { typeof(SpriteBatch) }),
                    postfix: new HarmonyMethod(typeof(QuestHints), nameof(LogDrawn)));
            }
            catch (Exception ex)
            {
                monitor.Log($"Could not add notes to the quest log, so the quest log stays as the game makes it. ({ex.Message})", LogLevel.Error);
            }
        }

        /// <summary>Drops every stored line, for example when the language changes.</summary>
        public static void Forget() => Cache.Clear();

        /// <summary>The width the quest log wraps a description to while it is open and pictures are wanted, or <see cref="NoWidth"/>.</summary>
        private static int LogTextWidth() =>
            _config!().ShowNameIcons && Game1.activeClickableMenu is QuestLog log ? log.width - LogTextInset : NoWidth;

        private static void GetDescriptionPostfix(Quest __instance, ref string __result)
        {
            if (!_reportedFirstCall)
            {
                _reportedFirstCall = true;
                _monitor?.Log("The quest log asked for a description, so the notes are in place.", LogLevel.Trace);
            }

            if (_config is null || _source is null || !Context.IsWorldReady || !_config().ShowHints || __instance.completed.Value)
                return;

            var cached = Cache.GetOrCreateValue(__instance);
            var width = LogTextWidth();
            if (Game1.ticks - cached.Tick >= RefreshTicks || Game1.ticks < cached.Tick || width != cached.Width)
            {
                cached.Tick = Game1.ticks;
                try
                {
                    Lay(cached, Build(__instance), width);
                }
                catch (Exception ex)
                {
                    // A quest that does not look the way this expects (a quest or an item from another mod, say) must never break the
                    // quest log, and must not stop the notes of the other quests. It gets no notes and is tried again in a moment.
                    cached.Text = "";
                    cached.Spots = Array.Empty<IconSpot>();
                    cached.LineCount = 0;
                    cached.Width = width;
                    if (++_failures <= MaxLoggedFailures)
                        _monitor?.Log($"Could not add notes to the quest '{__instance.questTitle}'. ({ex.GetType().Name}: {ex.Message})", LogLevel.Warn);
                }
            }
            if (cached.Text.Length > 0)
                __result = __result + "\n\n" + cached.Text;
        }

        /// <summary>Lays the lines out for the quest log, leaving room for pictures, or strips the pictures' markers anywhere else.</summary>
        private static void Lay(Cached cached, string text, int width)
        {
            cached.Width = width;
            if (width == NoWidth || text.Length == 0)
            {
                cached.Text = IconText.Strip(text);
                cached.Spots = Array.Empty<IconSpot>();
                cached.LineCount = 0;
                return;
            }

            var font = Game1.dialogueFont;
            var layout = IconText.Layout(text, width - WrapMargin, line => font.MeasureString(line).X, IconRoom);
            cached.Text = layout.Text;
            cached.Spots = layout.Spots;
            cached.LineCount = layout.LineCount;
        }

        private static string Build(Quest quest)
        {
            if (QuestReader.ToFact(quest) is not { } fact)
                return "";
            return string.Join("\n", HintBuilder.Build(fact, _config!(), _source!, _source!));
        }

        /// <summary>Draws the pictures over the room the notes left in the quest log's description, under the same scrolling and clipping.</summary>
        private static void LogDrawn(QuestLog __instance, SpriteBatch b, IQuest ____shownQuest, int ___questPage)
        {
            if (_config is null || ___questPage == -1 || ____shownQuest is not Quest quest || !Cache.TryGetValue(quest, out var cached) || cached.Spots.Count == 0)
                return;

            try
            {
                var font = Game1.dialogueFont;
                var lines = Game1.parseText(____shownQuest.GetDescription(), font, __instance.width - LogTextInset).Count(c => c == '\n') + 1;
                var firstNoteLine = lines - cached.LineCount;
                if (firstNoteLine < 0)
                    return;

                // the same offsets the game draws the description with
                var shift = ____shownQuest.IsTimedQuest() && ____shownQuest.GetDaysLeft() > 0 && SpriteText.getWidthOfString(____shownQuest.GetName()) > __instance.width / 2 ? -48 : 0;
                var left = __instance.xPositionOnScreen + 64;
                var top = __instance.yPositionOnScreen + 96 + shift;
                var bottom = __instance.yPositionOnScreen + __instance.height - 32;
                var lineHeight = font.LineSpacing;

                foreach (var spot in cached.Spots)
                {
                    var y = top - __instance.scrollAmount + (firstNoteLine + spot.Line) * lineHeight + (lineHeight - IconSize) / 2f;
                    if (y < top || y + IconSize > bottom)
                        continue; // scrolled out of the description's frame
                    DrawIcon(b, spot, new Vector2(left + spot.X, y));
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"Could not draw the pictures in the quest log. ({ex.GetType().Name}: {ex.Message})", LogLevel.Trace);
            }
        }

        private static void DrawIcon(SpriteBatch b, IconSpot spot, Vector2 at)
        {
            if (spot.Kind == IconKind.Npc)
            {
                if (Game1.getCharacterFromName(spot.Id)?.Portrait is { } portrait)
                    b.Draw(portrait, at, new Rectangle(0, 0, 64, 64), Color.White, 0f, Vector2.Zero, IconSize / 64f, SpriteEffects.None, 1f);
                return;
            }

            var data = ItemRegistry.GetDataOrErrorItem(spot.Id);
            var source = data.GetSourceRect();
            b.Draw(data.GetTexture(), at, source, Color.White, 0f, Vector2.Zero, IconSize / (float)Math.Max(source.Width, source.Height), SpriteEffects.None, 1f);
        }
    }
}
