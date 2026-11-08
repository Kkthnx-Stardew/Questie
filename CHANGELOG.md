# Changelog

Versions follow [Semantic Versioning](https://semver.org/). Newest first.

## 1.0.0 (2026-11-08)

The first release. Built against Stardew Valley 1.6.15 and SMAPI 4.5.2 with no warnings, and 60 tests pass. It was run in the game alone and with one host and one joining player on one computer. Three or more players, split screen and play over the internet are untested.

- Markers over the people and monsters your quests need, drawn with the game's own emote bubbles and stat icons. An exclamation mark for a hand-in (with the item beside it for a delivery), a speech bubble for a villager a meeting quest still needs, and a skull for a monster a kill quest asks for.
- Notes at the end of a quest in the journal. Progress, what you carry, who is left to meet, where a lost item lies, where a person is right now, how to make an item and how to learn its recipe.
- Fishing help. Where the fish bites with the seasons for each place, the times of day, the weather, and crab pot water for a crab pot fish. It reads the game's own data, so fish and places from other mods are covered.
- A meeting quest that can be finished. When a save loads, villagers who no longer exist are taken off the list and sociable villagers you have not met are added, by the game's own rule. Each change is logged.
- Settings for each part, in config.json and in Generic Mod Config Menu.
- The `questie_report` console command prints the quests Questie sees, the notes it adds and who and what it marks, for bug reports.
