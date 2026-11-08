using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;

namespace Questie
{
    /// <summary>
    /// Draws the bouncing markers over the people and monsters the quests need, using the game's own emote bubbles so they look
    /// like part of the game: an exclamation mark for a hand-in, a speech bubble for someone to greet and a skull for a monster.
    /// </summary>
    internal sealed class QuestMarkers
    {
        // frames of the game's emote picture, which the emote list in the farmer class names
        private const int ExclamationFrame = 16;
        private const int SpeechFrame = 42; // the three dot frame of the "pause" emote, which starts with one dot
        // the skull from the strip of stat icons in the game cursors picture (the one tooltips use for attack and defense)
        private static readonly Rectangle SkullIcon = new(140, 428, 10, 10);
        private const float SkullScale = 4f;

        private const int FrameSize = 16;
        private const float Scale = 4f;

        /// <summary>The picture of an item shown beside a delivery marker, and how it is cut out of its sheet.</summary>
        private readonly Dictionary<string, (Texture2D Texture, Rectangle Source)> _items = new(StringComparer.Ordinal);

        public void Forget() => _items.Clear();

        public void Draw(SpriteBatch batch, QuestTracker tracker, ModConfig config)
        {
            var plan = tracker.Plan;
            if (!config.EnableMarkers || plan.IsEmpty || Game1.eventUp || Game1.currentLocation is not { } location)
                return;
            var emotes = Game1.emoteSpriteSheet;
            if (emotes is null)
                return;

            // a gentle bounce, like the game's own quest marker
            var bounce = 4f * MathF.Sin((float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250f);

            var characters = location.characters;
            for (var i = 0; i < characters.Count; i++)
            {
                var npc = characters[i];
                if (npc.IsInvisible || !Utility.isOnScreen(npc.Position, 192))
                    continue;

                if (npc is Monster monster)
                {
                    if (plan.HasMonsters && monster.Health > 0 && plan.MarksMonster(monster.Name))
                        DrawSkull(batch, npc, bounce);
                }
                else if (plan.HasNpcs && npc.IsVillager && plan.TryGetMark(npc.Name, out var mark))
                {
                    DrawEmote(batch, emotes, npc, mark.Kind == MarkKind.Greet ? SpeechFrame : ExclamationFrame, bounce);
                    if (mark.ItemId is not null)
                        DrawItem(batch, npc, mark.ItemId, bounce);
                }
            }
        }

        /// <summary>The spot above a character's head, in world pixels: its sprite stands on its tile and reaches up from there.</summary>
        private static Vector2 Above(NPC npc, float lift)
        {
            var height = npc.Sprite?.getHeight() ?? 32;
            return new Vector2(npc.Position.X + 32f, npc.Position.Y + 64f - height * 4f * npc.Scale - lift);
        }

        private static void DrawEmote(SpriteBatch batch, Texture2D emotes, NPC npc, int frame, float bounce)
        {
            var source = new Rectangle(frame * FrameSize % emotes.Width, frame * FrameSize / emotes.Width * FrameSize, FrameSize, FrameSize);
            var world = Above(npc, 64f + 8f - bounce) - new Vector2(FrameSize * Scale / 2f, 0f);
            batch.Draw(emotes, Game1.GlobalToLocal(Game1.viewport, world), source, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 1f);
        }

        private static void DrawSkull(SpriteBatch batch, NPC monster, float bounce)
        {
            var size = SkullIcon.Width * SkullScale;
            var local = Game1.GlobalToLocal(Game1.viewport, Above(monster, size + 16f - bounce) - new Vector2(size / 2f, 0f));

            // a soft dark copy behind it keeps it readable on any ground
            batch.Draw(Game1.mouseCursors, local + new Vector2(2f, 2f), SkullIcon, Color.Black * 0.35f, 0f, Vector2.Zero, SkullScale, SpriteEffects.None, 0.99f);
            batch.Draw(Game1.mouseCursors, local, SkullIcon, Color.White, 0f, Vector2.Zero, SkullScale, SpriteEffects.None, 1f);
        }

        private void DrawItem(SpriteBatch batch, NPC npc, string itemId, float bounce)
        {
            if (!_items.TryGetValue(itemId, out var picture))
            {
                var data = ItemRegistry.GetDataOrErrorItem(itemId);
                picture = (data.GetTexture(), data.GetSourceRect());
                _items[itemId] = picture;
            }

            // beside the bubble, at half the size an item has in the backpack
            var world = Above(npc, 64f + 8f - bounce) + new Vector2(FrameSize * Scale / 2f + 4f, 8f);
            batch.Draw(picture.Texture, Game1.GlobalToLocal(Game1.viewport, world), picture.Source, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 1f);
        }
    }
}
