# Research notes

What was looked at before Questie was written, and what was not copied.

## The reference

An older public quest helper mod for the same game was reviewed in its recent version, together with its public description. It is licensed GPL-3.0, which does not fit the MIT licence of this mod. Nothing was copied from it. No code, no method names and no strings were used, and Questie was written from the game's own code and a running game.

It was read to learn what players get from it, not how it is built. In plain terms it does these things:

- Marks villagers a quest needs and monsters a kill quest asks for, by drawing over them while they are drawn.
- Adds fishing information (where, hours, weather) and crafting or cooking information to some quest descriptions.
- Lists who is left to greet in a meeting quest.
- Rebuilds a meeting quest when a mod adds or removes villagers.
- Adds item or fish information to two particular story quests, a fish dinner and a cake, by their ids.

## What Questie does differently, and why

| Area | The older mod | Questie |
| --- | --- | --- |
| Finding what to mark | Looks through every quest for every villager on every frame | Works the markers out when quests or the backpack change and twice a second, and draws from a lookup |
| Marker look | One small marker | Three, from the game's emote bubbles and stat icons, with the item beside a delivery |
| Quests covered | Delivery, fishing, kill, gathering, meeting, lost item, and two story quests by id | Also crafting, go-to-a-place, building and harvest notes, depth in the mines and Skull Cavern, seven story quests that bring an item, and the slime quest that counts every slime |
| Recipes | Says whether a recipe is known | Also says how to learn it, in hearts or skill levels |
| Fish | Where, hours and weather | Also the seasons the fish bites in, taken from the location rules the game catches fish by, whether it bites right now, crab pot water, readable names for places with no display name, and no festival or minigame places |
| Where is a person | No | Yes, and "is here" when they are in the same place |
| Items | Information about an item in a quest, nothing more | Marks the items in the backpack, chests and toolbar, counts what lies in chests, names who sells it, and puts the item's picture or the villager's portrait in front of the name |
| Special order boxes | No | A list of what the order asks for and how far you have come, placed clear of the box and the backpack |
| Meeting quest | Rebuilds by rule | Both directions, with the rule written as a tested function and a log line for each change |
| Tests | None found in its folder | Game-free logic is unit tested |

## The game

Everything about quests, fish, recipes and drawing was checked against the game's own code (Stardew Valley 1.6.15, decompiled for reading only) and against a running game, not against another mod. The places it matters most:

- Quest classes and their fields, and that the journal asks the quest for its description on every frame it is open.
- The fish data layout (difficulty, motion, times, weather, crab pot water) and the location fish lists with seasons.
- The recipe text layouts for crafting and cooking, and how unlock conditions are written (a villager and hearts, or a skill and level).
- The emote picture, four frames to an emote, and the strip of stat icons in the cursors picture.

Fish, recipe and place facts shown in the journal were compared with the game wiki for a sample (Rainbow Trout, Pufferfish, Crimsonfish, Salad, Vegetable Medley, Lightning Rod, Keg) and matched.

## Open questions

- Split screen is written for (each player has their own marker plan) but has not been run.
- Quest kinds added by other mods as new classes are not covered.
