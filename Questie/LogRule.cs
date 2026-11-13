using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace Questie
{
    /// <summary>
    /// The thin rule the quest log shows between the game's own text and the notes added to it, so it is plain which part is added.
    /// It is drawn through the blank line the notes leave, under the same scrolling and clipping as the text.
    /// </summary>
    internal static class LogRule
    {
        private const int Height = 2;

        /// <summary>A darker brown of the quest log's own frame, kept faint.</summary>
        private static readonly Color Brown = new Color(120, 66, 22) * 0.5f;

        private static readonly RasterizerState ClipToScissor = new() { ScissorTestEnable = true };

        /// <summary>The quest log's own margins around the description, which its text width is the log's width less.</summary>
        public const int TextInset = 128;

        /// <summary>The game wraps the description a little differently from how lines are measured here, so they are kept this much narrower.</summary>
        public const int WrapMargin = 24;

        /// <summary>Draws the rule, and returns whether it did, so the caller can draw the mouse cursor over it again.</summary>
        /// <param name="blankLine">The line of the whole description, counting from zero, that is the blank one before the notes.</param>
        public static bool Draw(SpriteBatch b, QuestLog log, int top, int blankLine, int textWidth)
        {
            if (blankLine < 0)
                return false;

            var bottom = log.yPositionOnScreen + log.height - 32;
            var left = log.xPositionOnScreen + 64;
            var y = (int)(top - log.scrollAmount + blankLine * Game1.dialogueFont.LineSpacing + Game1.dialogueFont.LineSpacing / 2);
            if (y <= top || y >= bottom)
                return false; // scrolled out of the description's frame

            var frame = Utility.ConstrainScissorRectToScreen(new Rectangle(log.xPositionOnScreen + 32, top, log.width - 64, bottom - top));
            var saved = b.GraphicsDevice.ScissorRectangle;
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, ClipToScissor);
            try
            {
                b.GraphicsDevice.ScissorRectangle = frame;
                b.Draw(Game1.staminaRect, new Rectangle(left, y, textWidth - WrapMargin, Height), Brown);
            }
            finally
            {
                // back to the state the game's drawing left the batch in
                b.End();
                b.GraphicsDevice.ScissorRectangle = saved;
                b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
            return true;
        }
    }
}
