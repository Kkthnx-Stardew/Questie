# Changelog

Versions follow [Semantic Versioning](https://semver.org/). Newest first.

## 0.9.0-beta.6 (2026-11-08)

- Special orders. An item that fits an order that asks you to deliver, donate or ship items now gets the quest mark in the backpack, in chests and on the toolbar, and its tooltip says which order wants it and what to do with it. Orders ask for items by tag, such as any vegetable, so the game's own rule decides which items fit. The journal page of such an order says how many you carry that fit and where to take them, the person for a delivery, the place of the donation box, or the shipping bin, and that a delivery has to be handed over in one stack. The person a delivery order is for gets the hand-in marker while you carry a stack big enough, and the monsters of a kill order get the skull.
- Shop prices are worked out the way the shop works them out, with the default price of the item, the profit margin and the price modifiers, so an item with no fixed price, such as seeds, shows what it really costs. Before, only a price written in the shop's data was shown.
- The journal no longer says "Slain 2 of 5", "Caught 1 of 3", "Collected 4 of 10" or "Greeted 2 of 28" under the game's own line that already says it.

## 0.9.0-beta.5 (2026-11-08)

- "Where to buy" no longer lists festival stalls, which are open for a day and showed up as names such as "Festival Night Market Magic Boat Day1". A shop that always stocks the item is named before one that only stocks it at times.
- The words around a shop price, an ingredient count and a time range, "(120g)", "(2 of 1)" and "6am to 1pm", are now in the translation file, so a translation can word them its own way.

## 0.9.0-beta.4 (2026-11-08)

- Fishing quests now say which seasons the fish bites in, such as "Bites in spring and summer.", and say "It is not in season now." when it is the wrong season, instead of a right now line that could not be true. The seasons come from the season rules of each place, including the ones written as conditions, because those are what the game catches fish by. Before, only the places with a plain season field were read, so most fish showed no season at all.
- Festival and minigame places are no longer listed as places to fish.
- Story quests that ask you to bring an item are covered. They are the rainbow shell, the beets for the mayor's fridge, the copper ore, the cave carrot, the largemouth bass for the fish casserole, the maple syrup for the strange note and the chocolate cake for the cake-walk. Each shows how many you carry, where to take it and how to get it, and the item is marked in your backpack.
- Building quests say who builds the building and what it costs.
- Harvest quests show how many of the item you carry.
- The quests about reaching a level in the mines or the Skull Cavern say how deep you have been.

- The list beside a donation box now puts the item's own picture in front of each line that asks for one item, such as the void souls and hardwood of the Bridge Magic order. A line that takes several kinds, such as any red item, has no picture.
- A pass over speed, memory and tidiness. The donation list and the quest log pictures are laid out once and reused instead of rebuilt every frame, the shop and recipe lists are built while a save loads so the first journal opening does not pause, the donation list lets go of its order when the box closes, and pictures in the quest log are clipped to the page like the text so a half scrolled line keeps its portraits. Measured numbers are in docs/PERFORMANCE.md.
- A donation line that accepts one of several lists of items no longer shows the picture of only the first.
- A place whose name the game cannot find is no longer listed as "(no translation...)", and shop names with an acronym, such as RSV Heaps Store, are split into words.
- Checked with Stardew Valley Expanded, Ridgeside Village, East Scarp, Sunberry Village and More New Fish loaded together. Everything loaded without a problem from Questie, and their fish, villagers, shops, recipes, quests and donation orders were read correctly.
- The meeting quest repair only adds a villager once that villager is in the world, because an expansion can hold villagers back until a condition is met and the quest could not be finished by greeting someone who is not there.
- Portraits are looked up less often in the quest log, because finding a villager searches every place and an expansion adds a lot of places.

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
