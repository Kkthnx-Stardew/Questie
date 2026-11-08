using StardewValley;

namespace Questie
{
    /// <summary>
    /// One player's quests as facts, and the marker plan built from them. Each split screen player has their own, because the
    /// quests are theirs. It is refreshed when something changes and a few times a second, and read every frame by the markers.
    /// </summary>
    internal sealed class QuestTracker
    {
        private readonly List<QuestFact> _facts = new();

        public MarkerPlan Plan { get; } = new();

        public IReadOnlyList<QuestFact> Facts => _facts;

        public void Refresh(ModConfig config)
        {
            var player = Game1.player;
            if (player is null)
            {
                Clear();
                return;
            }

            QuestReader.Read(player, _facts);
            Plan.Rebuild(_facts, config, id => player.Items.CountId(id));
        }

        public void Clear()
        {
            _facts.Clear();
            Plan.Rebuild(_facts, new ModConfig { EnableMarkers = false }, _ => 0);
        }
    }
}
