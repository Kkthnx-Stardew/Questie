# Questie

A quest helper for Stardew Valley. It marks the people and monsters your quests need, marks the items in your backpack that a quest wants, and adds progress, where to fish, where to buy, how to make an item, where a monster lives and who you still have to meet to the end of each quest in your journal. It uses the game's own pictures and words, so it looks like part of the game.

This is a beta (0.9.0-beta.8). It works and has been tested in the game, but it has had few players, so please report anything odd. Version 1.0 is kept for the first full release.

![Markers over people and monsters](docs/images/markers.png)

## What it does

**Markers over heads.** A bouncing marker, drawn with the game's own emote bubbles, shows who and what a quest needs right now.

| Marker | Meaning |
| --- | --- |
| An exclamation mark | Someone to hand something in to. For a delivery it only shows while you carry enough of the item, and the item is shown beside it. It also marks the person to go back to once a kill, fishing or gathering quest is done, and the person of a lost item quest once you have found it. A special order that asks you to deliver items marks its person the same way, once you carry a stack big enough. |
| A speech bubble | A villager a meeting quest such as Introductions still needs you to greet. |
| A skull | A monster a kill quest or a special order asks for. The slime quest marks every slime, jelly and sludge, as the game counts them. |

**Notes in the journal.** Notes are added to the end of a quest's description, so nothing the game says is replaced. Each note is a bullet in the game's own dash style, a thin rule sets the notes apart from the game's text, and the villager's portrait or the item's own picture sits in front of each name.

| The quest asks for | The notes say |
| --- | --- |
| Any item | How many you carry and how many lie in your chests and fridge. Which shops sell it and for how much. How it is crafted or cooked and whether you know the recipe, or which machine makes it from what. |
| A crop | Which seed to plant, the seasons, how many days it takes and whether it regrows. |
| Forage | Which places it can be found in and the seasons at each. |
| A monster drop | Which monsters drop it, how often, and where they live. |
| A fish, in a fishing quest or a delivery | Where it bites, in which seasons, at what hours and in what weather, and whether it bites right now. A crab pot fish says which water. |
| Kills | Where the monster lives. |
| Meeting people | Who is left to meet. |
| A person to hand in to | Where they are right now. |
| A lost item | Where it lies, until you find it. |
| A story quest | What its objective leaves out, such as which item to bring, who to give it to and what that person trades back. This covers The Pirate's Wife, Ol' Pappy Elixir and the game's other plain story quests. |
| A building or a depth | Who builds it and what it costs, or how deep you have been. |

The long form, with sources, is in [docs/NOTES.md](docs/NOTES.md).

![A delivery for a fish, with where and when it bites](docs/images/log-fishing.png)

![A meeting quest with notes](docs/images/log-meeting.png)

![Portraits and item pictures in front of names](docs/images/log-portraits.png)

![An item picture in front of its name, and the count in chests](docs/images/log-chests.png)

**Marks on items.** An item a quest or a special order wants gets the game's own quest exclamation mark in the top left corner of its slot, in your backpack, in chests and on the toolbar, the way the quality star sits in the bottom left. Hover over it and the tooltip says which quest wants it and what to do. A delivery item is marked until you hand it in. A fish or a resource is marked until you have caught or gathered enough. An item that fits a special order that asks for items you hand over, donate or ship is marked too, and its tooltip says what to do with it.

![Quest items marked in the backpack, with the tooltip](docs/images/item-marks.png)

**Donation boxes.** A special order that takes donations, such as the eggs for Gus, shows what it asks for and how far you have come beside the box while it is open, so you do not have to close the box and open the journal to find out. The game keeps the count up to date as items go in and come out. Each line that asks for one item has that item's picture in front of it. The list stays clear of the box and your backpack, and says how many lines more there are when the screen has no room for all of them.

![A donation box with the order's list and the items' pictures](docs/images/donation-box.png)

**Special orders.** The journal page of a special order that asks for items says how many you carry that fit, and where to take them, which person for a delivery, which place the donation box stands in, or the shipping bin. An order with several objectives names each one. An objective that asks for one item has that item's own picture beside it on the page, in the strip left of the list, such as the void souls and slime eggs of the Bridge Magic order. A delivery order tells you the items have to be handed over together in one stack, which is how the game takes them. Orders that ask for items by tag, such as any vegetable, are matched with the game's own rule. An order that asks for one crop, such as the Crop Order, says which seed to plant, the seasons, how long it takes and whether it regrows.

**A warning on the last day.** When a timed quest, such as a Help Wanted request, is on its last day, a message tells you as the day starts.

**A meeting quest that can be finished.** Introductions lists the villagers who exist when it starts. If a mod removes one of them, the quest can never be completed, and if a mod adds one, the quest never asks for them. When a save loads, Questie takes people who no longer exist off the list and adds sociable villagers you have not met, using the game's own rule for who counts.

## Multiplayer

Quests belong to each player, so markers and notes follow your own journal. In the same game, a farmhand sees a marker over a villager for their delivery and the host sees none.

![The farmhand's view](docs/images/multiplayer-farmhand.png)

![The host's view of the same place](docs/images/multiplayer-host.png)

## Install

1. Install [SMAPI](https://smapi.io/).
2. Download the zip from the [Nexus Mods page](https://www.nexusmods.com/stardewvalley/mods/55257) or the [Releases page](https://github.com/Kkthnx-Stardew/Questie/releases) and unzip it into your `Mods` folder.
3. Run the game through SMAPI.

Stardew Valley 1.6.9 or newer. [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) is optional and gives you the settings in the game.

## Settings

Everything is on by default. Edit `config.json` after the first run, or use Generic Mod Config Menu.

| Setting | What it does |
| --- | --- |
| `EnableMarkers` | The bouncing markers at all. |
| `MarkDeliveries` | Mark the person an item must be delivered to. |
| `OnlyWhenCarried` | Mark a delivery person only while you carry enough of the item. |
| `MarkTurnIn` | Mark the person to go back to when a quest is done. |
| `MarkSocialize` | Mark the villagers a meeting quest still needs. |
| `MarkMonsters` | Mark the monsters a kill quest asks for. |
| `ShowHints` | Add notes to the end of quests in the journal. |
| `ShowFishInfo` | Say where, when and in what weather a fish bites. |
| `ShowShops` | Say which shops sell the item a quest wants. |
| `ShowNameIcons` | Put the portrait or item picture in front of a name in the quest log, and the item picture beside a special order's objective. |
| `ShowRecipes` | Say how to craft or cook an item, and how to learn the recipe. |
| `ShowWhereIsNpc` | Say where a quest's person is right now. |
| `ShowItemMarks` | Mark the items a quest wants with an exclamation mark, and add a line to their tooltip. |
| `ShowDonations` | List what a special order asks for beside its donation box. |
| `WarnExpiring` | Show a message on the last day of a timed quest. |
| `FixSocializeQuests` | Repair a saved meeting quest when the villagers change. |

## Console command

`questie_report` lists the quests Questie knows how to help with, the notes it adds to each, and who and what it marks. Paste it into a bug report.

## Performance

Markers are worked out when something changes and twice a second, and drawn from a lookup, so a frame does no searching. The item marks cost one set lookup for each slot on screen. Measured in the game with five quests, working out the markers takes about a microsecond and asking for a quest's notes about a twentieth of a microsecond. The notes themselves are rebuilt at most three times a second for the quest on screen. More in [docs/PERFORMANCE.md](docs/PERFORMANCE.md).

## Works with expansions

Tried in the game with Stardew Valley Expanded 1.15.11, Ridgeside Village 2.5.17, East Scarp 3.0.9, Sunberry Village and More New Fish, among about seventy mods. Their villagers get markers, portraits and a place on a meeting quest the way the game's own do, their fish show their places and seasons, their shops and recipes are named, and their donation orders get the list beside the box, such as the Bridge Magic order. Quests those mods add that are of the game's usual kinds work like any other. A quest they add as a plain story quest gets help when its objective text names items the game knows, such as the five fish of Ol' Pappy Elixir: each item is marked, and a fish says where and in which seasons it bites. An expansion's plain quest that names no item still shows the game's own text only.

## Limits

- It is tested on Stardew Valley 1.6.15 with SMAPI 4.5.2, alone and with one host and one joining player on one computer. Three or more players, split screen and play over the internet are untested.
- The notes are in English. The translation file is `i18n/default.json`, and other languages can be added the usual way.
- Special orders are not quests in the game's code. Questie covers the objectives that ask you to deliver, donate or ship items and to kill monsters. Objectives that count what you pick up, catch or reach, such as collecting, fishing and mine floors, are left to the game's own progress bars. A kill order marks its monsters on your farm too, although the game does not count those.
- Fish seasons come from the game's location list. A fish that a mod limits to a season in some other way may be listed for more seasons than it bites in.
- Crop seasons come from the game's crop data. A greenhouse or Ginger Island can grow a crop out of its listed season, which the note does not mention.
- Forage places come from the save's locations and their forage data. Season rules in a condition, including negated ones, are read; a condition with no season rule, such as a year or a letter, does not hide the place. A forage entry that names its item with an item query is expanded to every object it can produce. An item dug from an artifact spot or the dirt rather than spawned as forage may not be covered.
- A special order gets a grow line only when its rule requires one item tag that names a crop. An order that takes only a category, a colour or several alternatives gets none.
- Monster drops come from the game's monster data, plus a small table of the extras the game adds in its monster code, such as Ectoplasm, Prismatic Jelly, Red Cabbage Seeds, Bone Swords, Pepper Rex eggs and bones, Tiger Slime seeds, Haunted Skull candy and Iridium Golem ore. An extra not in that table, such as the mine-bottom Diamond or Prismatic Shard, may not be covered.
- Monster places are the game's own fixed places for its monsters. The game does not store them in its monster data, so a monster added by another mod shows its chance without a place unless it reuses the id or name of a known monster.
- Quest kinds from other mods that are not one of the game's own are left alone.
- Items a plain story quest names only in its description, and not in its objective line, are not read. Only objects and big craftables are looked for, since those are what a quest asks the farmer to bring.

## How it was made

Questie was written from scratch. The behaviour of an older quest helper mod was studied to see what players need, and none of its code was used. The notes on what was reviewed are in [docs/RESEARCH.md](docs/RESEARCH.md).

## Contributing and license

Bug reports with the output of `questie_report` are the most useful thing. MIT licensed, see [LICENSE.txt](LICENSE.txt).
