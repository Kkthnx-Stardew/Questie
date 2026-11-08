using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Characters;
using StardewValley.Quests;

namespace Questie
{
    /// <summary>
    /// Makes a Socialize quest such as Introductions match the villagers who exist now, using the game's own rule for who counts.
    /// The rule is the one the game uses when it first writes the quest. A villager counts when their data says so, or when their
    /// home region is the Town, and only the ones the Social tab does not show from the start have to be greeted. A villager is only
    /// added once they are in the world, because a mod can hold one back until a condition is met.
    /// </summary>
    internal static class SocializeFixer
    {
        public static void Run(IMonitor monitor)
        {
            var player = Game1.player;
            if (player is null)
                return;

            var sociable = new HashSet<string>(StringComparer.Ordinal);
            var needGreeting = new List<string>();
            foreach (var (name, data) in Game1.characterData)
            {
                if (!(data.IntroductionsQuest ?? data.HomeRegion == "Town"))
                    continue;
                sociable.Add(name);
                // a villager a mod only unlocks later is in the data but not in the world yet, and the quest cannot be finished
                // by greeting someone who is not there, so they are added on the day they appear
                if (data.SocialTab != SocialTabBehavior.AlwaysShown && Game1.getCharacterFromName(name) is not null)
                    needGreeting.Add(name);
            }

            foreach (var quest in player.questLog)
            {
                // a daily Socialize quest is rebuilt by the game every time, so only a saved one can fall out of date
                if (quest is not SocializeQuest socialize || socialize.completed.Value || socialize.dailyQuest.Value)
                    continue;

                var changes = SocializeRepair.Plan(socialize.whoToGreet.ToList(), sociable, needGreeting, name => player.friendshipData.ContainsKey(name));
                if (!changes.Any)
                    continue;

                foreach (var name in changes.Remove)
                {
                    socialize.whoToGreet.Remove(name);
                    monitor.Log($"The meeting quest listed '{name}', who is no longer in the game, so it was taken off the list.", LogLevel.Info);
                }
                foreach (var name in changes.Add)
                {
                    socialize.whoToGreet.Add(name);
                    monitor.Log($"The meeting quest did not list '{name}', who is in the game and not met yet, so it was added.", LogLevel.Info);
                }
                socialize.total.Value = Math.Max(socialize.whoToGreet.Count, socialize.total.Value + changes.TotalChange);
            }
        }
    }
}
