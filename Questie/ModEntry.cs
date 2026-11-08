using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace Questie
{
    public class ModEntry : Mod
    {
        /// <summary>How often the markers are worked out again while nothing else has told them to, in game updates. Twice a second follows a kill or a catch.</summary>
        private const int RefreshTicks = 30;

        private ModConfig _config = new();

        // Each split screen player has their own quests, so their own plan.
        private readonly PerScreen<QuestTracker> _tracker = new(() => new QuestTracker());

        private readonly QuestMarkers _markers = new();
        private FishLookup _fish = null!; // set in Entry
        private RecipeLookup _recipes = null!; // set in Entry
        private ShopLookup _shops = null!; // set in Entry
        private ChestLookup _chests = null!; // set in Entry
        private GameHintSource _source = null!; // set in Entry
        private DonationPanel _donations = null!; // set in Entry

        public override void Entry(IModHelper helper)
        {
            _config = LoadConfig(helper);

            _fish = new FishLookup(count => helper.Translation.Get("hint.more", new { count }), (start, end) => helper.Translation.Get("hint.time.range", new { start, end }));
            _recipes = new RecipeLookup((item, have, need) => helper.Translation.Get("hint.ingredient", new { item, have, need }));
            _shops = new ShopLookup();
            _chests = new ChestLookup();
            _source = new GameHintSource(helper.Translation, _fish, _recipes, _shops, _chests);
            QuestHints.Apply(Monitor, () => _config, _source);
            _donations = new DonationPanel(_source);
            ItemBadges.Apply(Monitor, () => _config, () => _tracker.Value, _source);

            helper.Events.GameLoop.GameLaunched += (_, _) => ConfigMenu.Setup(this, helper, () => _config, OnConfigSaved);
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Player.Warped += (_, e) => { if (e.IsLocalPlayer) Refresh(); };
            helper.Events.Player.InventoryChanged += (_, e) => { if (e.IsLocalPlayer) Refresh(); };
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            helper.Events.Display.MenuChanged += (_, _) => _donations.Release();
            helper.Events.Content.AssetsInvalidated += OnAssetsInvalidated;
            helper.Events.Content.LocaleChanged += (_, _) => ForgetLookups();

            helper.ConsoleCommands.Add("questie_report", "Lists your quests as Questie sees them and who or what it marks, to paste into a bug report.", (_, _) => Monitor.Log(Report(), LogLevel.Info));
        }

        // ---------------------------------------------------------------- settings

        private ModConfig LoadConfig(IModHelper helper)
        {
            ModConfig config;
            try
            {
                config = helper.ReadConfig<ModConfig>() ?? new ModConfig();
            }
            catch (Exception ex)
            {
                // a config.json that cannot be read (a typo, text where a switch belongs) must not stop the mod from loading
                Monitor.Log($"Could not read config.json, so the default settings are used and the file is left as it is. Fix it or delete it. ({ex.Message})", LogLevel.Warn);
                config = new ModConfig();
            }
            return config;
        }

        private void OnConfigSaved()
        {
            Helper.WriteConfig(_config);
            Refresh();
        }

        // ---------------------------------------------------------------- the game's events

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            Repair();
            Refresh();
            WarmUp();
        }

        /// <summary>
        /// Builds the shop and recipe indexes now, while the save is still loading, so the first time the quest log is opened does not
        /// stop for the few frames it takes. They are built again, when asked for, if the game's data changes later.
        /// </summary>
        private void WarmUp()
        {
            try
            {
                _shops.Find("(O)0");
                _recipes.Find("(O)0");
            }
            catch (Exception ex)
            {
                Monitor.Log($"Could not get the shop and recipe lists ready ahead of time, so they are built when first needed. ({ex.Message})", LogLevel.Trace);
            }
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            Repair();
            Refresh();
            WarnExpiring();
        }

        private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
        {
            _tracker.Value.Clear();
            ForgetLookups();
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (Context.IsWorldReady && e.IsMultipleOf(RefreshTicks))
                Refresh();
        }

        private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
        {
            if (Context.IsWorldReady)
                _markers.Draw(e.SpriteBatch, _tracker.Value, _config);
        }

        private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
        {
            if (_config.ShowDonations && Context.IsWorldReady)
                _donations.Draw(e.SpriteBatch);
        }

        private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
        {
            foreach (var name in e.NamesWithoutLocale)
            {
                if (name.IsEquivalentTo("Data/Fish") || name.IsEquivalentTo("Data/Locations") || name.IsEquivalentTo("Data/CraftingRecipes")
                    || name.IsEquivalentTo("Data/CookingRecipes") || name.IsEquivalentTo("Data/Objects") || name.IsEquivalentTo("Data/Shops"))
                {
                    ForgetLookups();
                    return;
                }
            }
        }

        // ---------------------------------------------------------------- the work

        private void Refresh()
        {
            if (Context.IsWorldReady)
                _tracker.Value.Refresh(_config);
        }

        /// <summary>Says so, once as the day starts, for each timed quest whose last day it is. A quest with no days left is already gone.</summary>
        private void WarnExpiring()
        {
            if (!_config.WarnExpiring || !Context.IsWorldReady)
                return;
            foreach (var quest in Game1.player.questLog)
            {
                if (quest.completed.Value || !quest.IsTimedQuest() || quest.GetDaysLeft() != 1)
                    continue;
                Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("message.expiring", new { quest = quest.questTitle }), HUDMessage.newQuest_type));
            }
        }

        private void Repair()
        {
            if (!_config.FixSocializeQuests || !Context.IsWorldReady)
                return;
            try
            {
                SocializeFixer.Run(Monitor);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Could not check the meeting quest against the villagers. ({ex.Message})", LogLevel.Warn);
            }
        }

        private void ForgetLookups()
        {
            _fish.Reset();
            _recipes.Reset();
            _shops.Reset();
            _chests.Reset();
            _donations.Reset();
            QuestHints.Forget();
        }

        /// <summary>The text printed by the questie_report command.</summary>
        private string Report()
        {
            if (!Context.IsWorldReady)
                return "No save is loaded.";

            var tracker = _tracker.Value;
            tracker.Refresh(_config);
            var lines = new List<string> { $"Questie {ModManifest.Version} | SMAPI {Constants.ApiVersion}, Stardew Valley {Game1.version}", $"{tracker.Facts.Count} quests Questie knows how to help with." };
            foreach (var fact in tracker.Facts)
                lines.Add($"  quest {fact.Id} {fact.Kind} npc={fact.Npc ?? "-"} item={fact.ItemId ?? "-"} monster={fact.Monster ?? "-"} {fact.Have}/{fact.Need} left=[{string.Join(", ", fact.Remaining)}]");
            // how many characters the quest log would show beyond the game's own description, which proves the notes reach the log
            foreach (var quest in Game1.player.questLog)
            {
                if (QuestReader.ToFact(quest) is null)
                    continue;
                var shown = ((StardewValley.Quests.IQuest)quest).GetDescription();
                var notes = shown.Length > quest.questDescription.Length ? shown[quest.questDescription.Length..].Trim().Replace("\n", " | ") : "(none)";
                lines.Add($"  notes for quest {quest.id.Value} '{quest.questTitle}': {notes}");
            }
            lines.Add("Marked people: " + (tracker.Plan.HasNpcs ? string.Join(", ", tracker.Plan.Npcs.Select(n => $"{n} ({tracker.Plan.NpcMark(n)})")) : "none"));
            lines.Add("Marked monsters: " + (tracker.Plan.HasMonsters ? string.Join(", ", tracker.Plan.Monsters) : "none"));
            return string.Join(Environment.NewLine, lines);
        }
    }
}
