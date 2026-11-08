# Questie

A quest helper for Stardew Valley. It marks the people and monsters your quests need, and adds progress, where to fish, how to make an item and who you still have to meet to the end of each quest in your journal. It uses the game's own pictures and words, so it looks like part of the game.

![Markers over people and monsters](docs/images/markers.png)

## What it does

**Markers over heads.** A bouncing marker, drawn with the game's own emote bubbles, shows who and what a quest needs right now.

| Marker | Meaning |
| --- | --- |
| An exclamation mark | Someone to hand something in to. For a delivery it only shows while you carry enough of the item, and the item is shown beside it. It also marks the person to go back to once a kill, fishing or gathering quest is done, and the person of a lost item quest once you have found it. |
| A speech bubble | A villager a meeting quest such as Introductions still needs you to greet. |
| A skull | A monster a kill quest asks for. The slime quest marks every slime, jelly and sludge, as the game counts them. |

**Notes in the journal.** Lines are added to the end of a quest's description, so nothing the game says is replaced.

- Progress such as "Caught 1 of 3", "Slain 2 of 5" or "Greeted 2 of 28", and how many you carry of what a delivery or gathering quest needs.
- For a fishing quest, where the fish bites, in which seasons, at what times of day and in what weather. A crab pot fish says which water. This is read from the game's own fish and location data, so it also covers fish and places added by other mods.
- For an item quest, how to make the item and whether you know the recipe. When you do not, it says how to learn it, such as at 3 hearts with Emily or at Foraging level 6.
- Who is left to meet in a meeting quest, and where a person is right now (or that they are here).
- Where a lost item lies, until you find it.

![A fishing quest with notes](docs/images/log-fishing.png)

![A meeting quest with notes](docs/images/log-meeting.png)

**A meeting quest that can be finished.** Introductions lists the villagers who exist when it starts. If a mod removes one of them, the quest can never be completed, and if a mod adds one, the quest never asks for them. When a save loads, Questie takes people who no longer exist off the list and adds sociable villagers you have not met, using the game's own rule for who counts.

## Multiplayer

Quests belong to each player, so markers and notes follow your own journal. In the same game, a farmhand sees a marker over a villager for their delivery and the host sees none.

![The farmhand's view](docs/images/multiplayer-farmhand.png)

![The host's view of the same place](docs/images/multiplayer-host.png)

## Install

1. Install [SMAPI](https://smapi.io/).
2. Download the zip from the [Releases page](https://github.com/Kkthnx-Stardew/Questie/releases) and unzip it into your `Mods` folder.
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
| `ShowRecipes` | Say how to craft or cook an item, and how to learn the recipe. |
| `ShowWhereIsNpc` | Say where a quest's person is right now. |
| `FixSocializeQuests` | Repair a saved meeting quest when the villagers change. |

## Console command

`questie_report` lists the quests Questie knows how to help with, the notes it adds to each, and who and what it marks. Paste it into a bug report.

## Performance

Markers are worked out when something changes and twice a second, and drawn from a lookup, so a frame does no searching. Measured in the game with five quests, working out the markers takes about a microsecond and asking for a quest's notes about a twentieth of a microsecond. The notes themselves are rebuilt at most three times a second for the quest on screen. More in [docs/PERFORMANCE.md](docs/PERFORMANCE.md).

## Limits

- It is tested on Stardew Valley 1.6.15 with SMAPI 4.5.2, alone and with one host and one joining player on one computer. Three or more players, split screen and play over the internet are untested.
- The notes are in English. The translation file is `i18n/default.json`, and other languages can be added the usual way.
- Special orders are not quests in the game's code and are left as they are.
- Quest kinds from other mods that are not one of the game's own are left alone.

## How it was made

Questie was written from scratch. The behaviour of an older quest helper mod was studied to see what players need, and none of its code was used. The notes on what was reviewed are in [docs/RESEARCH.md](docs/RESEARCH.md).

## Contributing and license

Bug reports with the output of `questie_report` are the most useful thing. MIT licensed, see [LICENSE.txt](LICENSE.txt).
