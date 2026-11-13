# Changelog

Versions follow [Semantic Versioning](https://semver.org/). Newest first.

## 0.9.0-beta.8 (2026-11-13)

- A delivery or gathering quest for a fish now gets the fishing help a fishing quest gets, such as where an albacore bites for Fish Stew, the crab pot water for Wanted: Lobster, and the places of a fish an expansion adds. Before, only a fishing quest said where a fish bites.
- A kill quest now says where the monster lives, such as "Found in The Mines and Skull Cavern.", until the kills are done.
- An item a machine makes out of another item now says so, such as "Made with the Keg from Hops." for pale ale or "Made with the Cheese Press from Milk or Large Milk." for cheese. It is read from the game's machine data, so machines from other mods are covered, and it follows the recipe help setting.
- The places a fish bites list the base game's places first, so with an expansion installed the list is not filled with the expansion's places only because they come first in the alphabet.
- A place with no display name, such as Ginger Island's north area, is shown readable ("Island North") and not as its internal name.
- A crop that grows in every season, such as the Qi Bean, says what to plant and no longer lists all four seasons.
- A meeting quest lists six names and counts the rest, so the notes and the quest's own count fit in the journal without scrolling.
- The screenshots in the README and on the Nexus page are retaken from a real save with about ninety mods. The long description of every note is in docs/NOTES.md, and the README has a short table in its place.
- The notes in the quest log are laid out as a list. Each note is a bullet in the same dash the game uses for a quest's rewards, a bullet that wraps continues under its text, and a thin rule separates the game's own text from the notes, so it is plain which part Questie added. Before, every note ran on as plain sentences straight after the description.
- Notes that belonged together are one line. What to plant, how long it takes and whether it regrows is one line, hours and weather for a fish are one line, and going back to someone and where they are now is one line. A recipe you have not learned says so and how to learn it in the same line, and no longer adds a line when the game does not say how.
- Monsters that drop an item with the same chance in the same places are named together, such as "Shadow Brute, Shadow Guy and Shadow Shaman (4% in The Mines)", instead of repeating the chance and places for each. The places a fish bites are cut to three with a count of the rest, like the forage and drop lists.
- A shop that asks more than ten times what the cheapest one does is left off the list, and cheaper shops are named first.
- A shop from another mod is named without the mod author's id in front, so "Pathoschild.Central Station Gift Shop" reads "Central Station Gift Shop".
- A special order's journal page names only the objectives you carry items for, or says you carry nothing for them yet, instead of repeating every objective just above the game's own list of them. Its notes use the same bullets and rule as the quest pages.
- The list beside a donation box says "0 of 20" when nothing is in yet, since "20 to go" was the same number again.

- Item pictures beside the objectives on a special order's page in the journal. The order's own lines have no room for a picture, so each one goes in the empty strip left of the list, lined up with its line, such as the void souls, slime eggs and monster fruits of the Bridge Magic order. An objective that takes several kinds of item, such as any vegetable, names no single item and gets no picture.
- The Pirate's Wife now has notes. The quest's data only says to find a keepsake, so Questie reads which keepsake you carry and says who to give it to, what that person trades back, where they are, and what the last item is for. The keepsake is marked in your backpack like any other quest item.
- Plain story quests that name their items only in the objective text now get help too, such as Ol' Pappy Elixir and its five fish. The names are the game's own item names, longest first, so fish and items added by an expansion are found as well, in any language. A quest that names one item gets what you carry and the fishing, shop and recipe help; one that names several gets one short line for each, such as where and in which seasons a fish bites, and every named item is marked in the backpack with a tooltip that names the quest. The words say which item, not how many, so no count is invented. Simple plurals such as "void souls" are read as the item they name.
- Quest items that are crops now say what to plant, when they grow and whether they regrow. A delivery, gathering, harvest or story quest for a parsnip says to plant Parsnip Seeds in spring and that it is ready in 4 days; coffee says it regrows every 2 days. This is read from the game's crop data, so crops added by other mods are covered too.
- A special order that asks for one crop, such as the Crop Order, now says which seed to plant, the seasons, how long it takes and whether it regrows. The order names its crop by a single item tag, so an order that takes a category or several alternatives gets no grow line.
- Quest items that are foraged now say where they can be found and the seasons that apply at each place, such as "Forage it in Backwoods in spring and Cindersap Forest in summer." Each place is listed with its own seasons instead of one combined season list. The places and seasons come from the game's own location data, so forage added by other mods is covered. A forage entry with a condition other than a simple season is now listed too: a year, a letter or the weather does not hide the place, and a season rule is read even when it is negated or part of a larger condition. A forage entry that names its item with an item query is expanded to every item the query can produce, so each of them gets the same places and seasons.
- Quest items that monsters drop now say which monsters drop them, where each can be found and how often, such as "Dropped by Frost Bat (95.5% in The Mines) and Bat (94% in The Mines and Skull Cavern)." The chances are read from the game's own monster data, so monsters added by other mods are covered. Places are the game's own fixed spawn places; a monster added by another mod shows its chance without a place unless it reuses the id or name of a known monster. The extra drops the game adds in its monster code are covered too, from a small table: Ectoplasm from Ghosts during the Wizard's special order, Prismatic Jelly from a Prismatic Slime, Red Cabbage Seeds from a Serpent or Mummy, Bone Swords from Skeletons, Dinosaur Eggs and bones from a Pepper Rex, Tiger Slime seeds and tubers, Haunted Skull candy and Iridium Golem ore. A code-added extra not in that table, such as the mine-bottom Diamond or Prismatic Shard, may not be covered.
- The game's remaining plain story quests now say what their words leave out. The sand dragon wants a Solar Essence, the goblin wants a Void Mayonnaise, and the Giant Stump takes 100 Hardwood, each marked in the backpack. The rest get a line of their own: where the lumber pile is, Willy on the beach, the Skull Key's door, Gunther's museum donations, giving a gift, the Community Center, Krobus in the sewer, and the bush where A Winter Mystery's figure hides.
- The list beside a donation box is now drawn beneath the mouse cursor instead of over it, so the cursor no longer disappears behind the list.
- The pictures in the quest log are drawn before the cursor too, so the cursor stays on top when it passes over one.

## 0.9.0-beta.7 (2026-11-08)

- Questie has a page on Nexus Mods, and the manifest lists it for update checks, so SMAPI can tell you when a new version is out. Nothing else changed from beta.6.

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
