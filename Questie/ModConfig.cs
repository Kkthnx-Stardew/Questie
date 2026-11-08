namespace Questie
{
    /// <summary>Settings read from config.json. This file has no game dependencies so the tests can compile it.</summary>
    public class ModConfig
    {
        /// <summary>Whether the bouncing marker is drawn at all.</summary>
        public bool EnableMarkers { get; set; } = true;

        /// <summary>Mark the person an item must be delivered to.</summary>
        public bool MarkDeliveries { get; set; } = true;

        /// <summary>Mark a delivery person only while the farmer carries enough of the item.</summary>
        public bool OnlyWhenCarried { get; set; } = true;

        /// <summary>Mark the person to go back to once a kill, fishing or gathering quest has been done.</summary>
        public bool MarkTurnIn { get; set; } = true;

        /// <summary>Mark the villagers a Socialize quest (such as Introductions) still needs.</summary>
        public bool MarkSocialize { get; set; } = true;

        /// <summary>Mark the monsters a kill quest asks for.</summary>
        public bool MarkMonsters { get; set; } = true;

        /// <summary>Add progress and help lines to the end of a quest's description in the quest log.</summary>
        public bool ShowHints { get; set; } = true;

        /// <summary>For a fishing quest, say where, when and in what weather the fish bites.</summary>
        public bool ShowFishInfo { get; set; } = true;

        /// <summary>For a delivery or gathering quest, say which shops sell the item.</summary>
        public bool ShowShops { get; set; } = true;

        /// <summary>For a quest about an item, say how to craft or cook it, and whether the recipe is known.</summary>
        public bool ShowRecipes { get; set; } = true;

        /// <summary>Say where a quest's person is right now.</summary>
        public bool ShowWhereIsNpc { get; set; } = true;

        /// <summary>Mark the items a quest wants with a small exclamation mark in the backpack, chests and toolbar, and say why in the tooltip.</summary>
        public bool ShowItemMarks { get; set; } = true;

        /// <summary>Beside a special order donation box, list what the order still needs.</summary>
        public bool ShowDonations { get; set; } = true;

        /// <summary>On the last day of a timed quest, such as a Help Wanted request, show a short message when the day starts.</summary>
        public bool WarnExpiring { get; set; } = true;

        /// <summary>On loading a save, drop people from a Socialize quest who no longer exist, so the quest can be finished.</summary>
        public bool FixSocializeQuests { get; set; } = true;

        /// <summary>Copies every setting from another instance. Used to restore the defaults from the config menu.</summary>
        public void CopyFrom(ModConfig other)
        {
            foreach (var property in typeof(ModConfig).GetProperties())
            {
                if (property.CanRead && property.CanWrite)
                    property.SetValue(this, property.GetValue(other));
            }
        }
    }
}
