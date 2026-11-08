# Performance

## Design

- The markers are worked out from the quests and the backpack when something changes (a warp, a change in the backpack, a new day) and twice a second, not every frame. A frame only looks up villagers by name and monsters by name part.
- A frame with no quest that needs a marker does no work beyond one check.
- A quest's notes are worked out at most three times a second for the quest being shown, and the quest log's own repeated requests get the stored text back. The number of lines the quest log wraps the notes to is kept with them, so the pictures are placed without wrapping the text again.
- Fish places, shop lists and recipe indexes are worked out once and kept until the data or the language changes. The shop and recipe lists are built while a save loads, so the first time the quest log opens does not pause.
- What lies in the chests is counted again at most once a second, and only while a quest note asks for it.
- The list beside a donation box is laid out at most ten times a second and drawn from that layout, and it lets go of the order when the box closes.
- The marks on item icons are one set lookup for each slot on screen.

## Measured

In the game, on the test PC, with about seventy mods loaded (Stardew Valley Expanded, Ridgeside Village, East Scarp, Sunberry Village and More New Fish among them, 218 villagers, 214 added fish) and a real save with chests across the world.

| What | Time |
| --- | --- |
| Work out the quest facts and the marker plan | about 1 to 4 microseconds |
| Count every chest and the fridge in every place | about 0.12 to 0.17 milliseconds, once a second at most |
| Build the notes of a quest for the quest log, with pictures, a meeting quest listing 107 villagers included | about 0.04 to 0.08 milliseconds |
| Ask for a quest's notes while they are stored | about 0.1 microseconds |
| Work out where and when a fish bites | about 0.05 milliseconds, once for each fish |
| Build the shop list, once | about 24 milliseconds, while the save loads |
| Build the recipe list, once | about 4 milliseconds, while the save loads |
| Find an item by its name tag, first time | about 1 millisecond |

Memory after 5000 refreshes of the quest facts and 200 counts of every chest, and after 300 rebuilds of every quest's notes with pictures, was unchanged to within a fraction of a kilobyte once the game had collected, so nothing builds up.

The test calls the mod through reflection, which adds a little time. The drawing of a marker, a picture in the quest log or the donation list was not timed on its own. Each is a few sprite draws for each thing in view.
