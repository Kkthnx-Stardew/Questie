using StardewModdingAPI;

namespace Questie
{
    /// <summary>Adds the settings to Generic Mod Config Menu, when it is installed.</summary>
    internal static class ConfigMenu
    {
        public static void Setup(IMod mod, IModHelper helper, Func<ModConfig> config, Action saved)
        {
            var menu = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (menu is null)
                return;

            var manifest = mod.ModManifest;
            var t = helper.Translation;

            menu.Register(
                mod: manifest,
                reset: () => config().CopyFrom(new ModConfig()),
                save: saved);

            menu.AddSectionTitle(manifest, () => t.Get("gmcm.markers"));
            Toggle(menu, manifest, t, "enable", () => config().EnableMarkers, v => config().EnableMarkers = v);
            Toggle(menu, manifest, t, "deliveries", () => config().MarkDeliveries, v => config().MarkDeliveries = v);
            Toggle(menu, manifest, t, "carried", () => config().OnlyWhenCarried, v => config().OnlyWhenCarried = v);
            Toggle(menu, manifest, t, "turnin", () => config().MarkTurnIn, v => config().MarkTurnIn = v);
            Toggle(menu, manifest, t, "socialize", () => config().MarkSocialize, v => config().MarkSocialize = v);
            Toggle(menu, manifest, t, "monsters", () => config().MarkMonsters, v => config().MarkMonsters = v);

            menu.AddSectionTitle(manifest, () => t.Get("gmcm.log"));
            Toggle(menu, manifest, t, "hints", () => config().ShowHints, v => config().ShowHints = v);
            Toggle(menu, manifest, t, "fish", () => config().ShowFishInfo, v => config().ShowFishInfo = v);
            Toggle(menu, manifest, t, "shops", () => config().ShowShops, v => config().ShowShops = v);
            Toggle(menu, manifest, t, "recipes", () => config().ShowRecipes, v => config().ShowRecipes = v);
            Toggle(menu, manifest, t, "where", () => config().ShowWhereIsNpc, v => config().ShowWhereIsNpc = v);
            Toggle(menu, manifest, t, "itemmarks", () => config().ShowItemMarks, v => config().ShowItemMarks = v);
            Toggle(menu, manifest, t, "donations", () => config().ShowDonations, v => config().ShowDonations = v);
            Toggle(menu, manifest, t, "expiring", () => config().WarnExpiring, v => config().WarnExpiring = v);
            Toggle(menu, manifest, t, "fix", () => config().FixSocializeQuests, v => config().FixSocializeQuests = v);
        }

        private static void Toggle(IGenericModConfigMenuApi menu, IManifest manifest, ITranslationHelper t, string key, Func<bool> get, Action<bool> set) =>
            menu.AddBoolOption(manifest, get, set, () => t.Get($"gmcm.{key}.name"), () => t.Get($"gmcm.{key}.tip"));
    }
}
