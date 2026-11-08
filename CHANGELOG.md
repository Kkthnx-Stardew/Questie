# Changelog

Versions follow [Semantic Versioning](https://semver.org/). Newest first.

## 0.9.0-beta.3 (2026-11-08)

- Pictures in the quest log. A villager's name gets their portrait in front of it and an item's name gets the item's own picture, in the notes of a delivery, a gathering quest, a lost item, a meeting quest and the rest. The text leaves room for each picture, and a name never splits across two lines.
- Stock in chests. A delivery or gathering quest now says how many of the item lie in your chests and fridge as well as in your backpack, such as "You carry 0 Stone, have 50 in chests, and need 25." A mini shipping bin and a loader are not counted, and a Junimo chest is counted once however many show it. The count is taken again at most once a second, and only while a quest note asks for it.
- A setting to turn the pictures off, in config.json and Generic Mod Config Menu.

## 0.9.0-beta.2 (2026-11-08)

- The list beside a donation box no longer covers the box or the backpack. It is placed clear of everything the menu uses, on the left or the right, narrower when the sides are tight, and above or below when there is no room at the sides. An order with more lines than fit shows the first ones whole and says how many more there are. Before, a long order such as one with seven objectives hid backpack slots.

## 0.9.0-beta.1 (2026-11-08)

The first beta. Version 1.0 is kept for the first full release. Built against Stardew Valley 1.6.15 and SMAPI 4.5.2 with no warnings, and 96 tests pass. It was run in the game alone and with one host and one joining player on one computer. Three or more players, split screen and play over the internet are untested.

- Markers over the people and monsters your quests need, drawn with the game's own emote bubbles and stat icons. An exclamation mark for a hand-in (with the item beside it for a delivery), a speech bubble for a villager a meeting quest still needs, and a skull for a monster a kill quest asks for.
- Marks on items. An item a quest wants gets the game's quest exclamation mark in the top left of its slot in the backpack, in chests and on the toolbar, and its tooltip says which quest wants it.
- A list beside a special order donation box that shows what the order asks for and how far you have come, so the box does not have to be closed to check the journal.
- Notes at the end of a quest in the journal. Progress, what you carry, who is left to meet, where a lost item lies, where a person is right now, which shops sell an item, how to make an item with a count of each ingredient you carry, and how to learn its recipe.
- Fishing help. Where the fish bites with the seasons for each place, the times of day, the weather, whether it can be caught right now, and crab pot water for a crab pot fish. It reads the game's own data, so fish and places from other mods are covered.
- A message on the last day of a timed quest.
- A meeting quest that can be finished. When a save loads, villagers who no longer exist are taken off the list and sociable villagers you have not met are added, by the game's own rule. Each change is logged.
- Settings for each part, in config.json and in Generic Mod Config Menu.
- The `questie_report` console command prints the quests Questie sees, the notes it adds and who and what it marks, for bug reports.
