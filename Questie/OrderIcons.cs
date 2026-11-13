using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.SpecialOrders.Objectives;

namespace Questie
{
    /// <summary>
    /// Puts the picture of the item an objective asks for beside it on a special order's page in the journal. The order's own lines have
    /// no room for a picture, so each one goes in the empty strip left of the page's objective list, lined up with its line. An objective
    /// that takes several kinds of item, such as any vegetable, names no single item and gets no picture.
    /// </summary>
    internal static class OrderIcons
    {
        private const int IconSize = 32;

        /// <summary>How far in from the page's edge the pictures sit. The page's frame takes the first 24 pixels.</summary>
        private const int IconInset = 32;

        /// <summary>The room a progress bar takes below its objective, worked out the way the journal works it out.</summary>
        private const int BarHeight = (12 + 4) * 4;

        private static readonly RasterizerState ClipToScissor = new() { ScissorTestEnable = true };

        private static readonly ItemTagIndex Tags = new();

        private static Func<ModConfig>? _config;
        private static IMonitor? _monitor;
        private static int _failures;
        private const int MaxLoggedFailures = 3;

        public static void Apply(IMonitor monitor, Func<ModConfig> config)
        {
            _monitor = monitor;
            _config = config;

            try
            {
                var harmony = new Harmony("Kkthnx.Questie.OrderIcons");
                harmony.Patch(
                    original: AccessTools.Method(typeof(QuestLog), nameof(QuestLog.draw), new[] { typeof(SpriteBatch) }),
                    postfix: new HarmonyMethod(typeof(OrderIcons), nameof(LogDrawn)));
            }
            catch (Exception ex)
            {
                monitor.Log($"Could not put item pictures beside special order objectives, so their pages stay as the game makes them. ({ex.Message})", LogLevel.Warn);
            }
        }

        /// <summary>Forgets the items found by tag. Called when the game's item data changes.</summary>
        public static void Reset() => Tags.Reset();

        private static void LogDrawn(QuestLog __instance, SpriteBatch b, IQuest ____shownQuest, int ___questPage)
        {
            if (_config is null || !_config().ShowNameIcons || ___questPage == -1 || ____shownQuest is not SpecialOrder order)
                return;
            if (order.questState.Value != SpecialOrderStatus.InProgress || order.ShouldDisplayAsComplete())
                return;

            try
            {
                var icons = new string?[order.objectives.Count];
                var any = false;
                for (var i = 0; i < icons.Length; i++)
                {
                    icons[i] = IconFor(order.objectives[i]);
                    any |= icons[i] is not null;
                }
                if (!any)
                    return;

                // the same place and line heights the journal uses for the description and its objectives
                var font = Game1.dialogueFont;
                var lineHeight = font.MeasureString("Ag").Y;
                var top = __instance.yPositionOnScreen + 96;
                var bottom = __instance.yPositionOnScreen + __instance.height - 32;
                var description = Game1.parseText(order.GetDescription(), font, __instance.width - 128);
                var yPos = top + font.MeasureString(description).Y + 32f - __instance.scrollAmount;
                var objectives = order.GetObjectiveDescriptions();

                var frame = Utility.ConstrainScissorRectToScreen(new Rectangle(__instance.xPositionOnScreen + 32, top, __instance.width - 64, bottom - top));
                var opened = false;
                var savedScissor = b.GraphicsDevice.ScissorRectangle;
                try
                {
                    for (var i = 0; i < icons.Length; i++)
                    {
                        // the line's height, which a wrapped objective makes more than one line tall
                        var text = i < objectives.Count ? objectives[i] : "";
                        var parsed = Game1.parseText(text, font, __instance.width - 192);

                        var iconY = yPos - 8f + (lineHeight - IconSize) / 2f;
                        if (icons[i] is { } icon && iconY + IconSize > frame.Top && iconY < frame.Bottom)
                        {
                            if (!opened)
                            {
                                opened = true;
                                b.End();
                                b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, ClipToScissor);
                                b.GraphicsDevice.ScissorRectangle = frame;
                            }
                            DrawIcon(b, icon, new Vector2(__instance.xPositionOnScreen + IconInset, iconY));
                        }

                        yPos += font.MeasureString(parsed).Y;
                        var objective = order.objectives[i];
                        if (objective.GetMaxCount() > 1 && objective.ShouldShowProgress())
                            yPos += BarHeight;
                    }
                }
                finally
                {
                    if (opened)
                    {
                        // back to the state the game's drawing left the batch in
                        b.End();
                        b.GraphicsDevice.ScissorRectangle = savedScissor;
                        b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                    }
                }

                // the pictures are drawn after the journal put the cursor on the screen, so the cursor goes back on top
                if (opened)
                    __instance.drawMouse(b);
            }
            catch (Exception ex)
            {
                // a page of an order from another mod must never break the journal
                if (++_failures <= MaxLoggedFailures)
                    _monitor?.Log($"Could not draw item pictures on a special order page. ({ex.GetType().Name}: {ex.Message})", LogLevel.Warn);
            }
        }

        /// <summary>The one item an objective asks for, or null when it takes several kinds or asks for no item.</summary>
        private static string? IconFor(OrderObjective objective)
        {
            var tag = objective switch
            {
                DonateObjective donate => DonationLines.IconTag(donate.acceptableContextTagSets),
                DeliverObjective deliver => DonationLines.IconTag(deliver.acceptableContextTagSets),
                ShipObjective ship => DonationLines.IconTag(ship.acceptableContextTagSets),
                _ => null,
            };
            return tag is null ? null : Tags.Find(tag);
        }

        private static void DrawIcon(SpriteBatch b, string qualifiedItemId, Vector2 at)
        {
            var data = ItemRegistry.GetDataOrErrorItem(qualifiedItemId);
            var source = data.GetSourceRect();
            b.Draw(data.GetTexture(), at, source, Color.White, 0f, Vector2.Zero, IconSize / (float)Math.Max(source.Width, source.Height), SpriteEffects.None, 1f);
        }
    }
}
