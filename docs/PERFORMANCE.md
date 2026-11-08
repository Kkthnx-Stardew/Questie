# Performance

## Design

- The markers are worked out from the quests and the backpack when something changes (a warp, a change in the backpack, a new day) and twice a second, not every frame. A frame only looks up villagers by name and monsters by name part.
- A frame with no quest that needs a marker does no work beyond one check.
- A quest's notes are worked out at most three times a second for the quest being shown, and the quest log's own repeated requests get the stored text back.
- Fish places and recipe indexes are worked out once per fish or recipe and kept until the data or the language changes.

## Measured

In the game, with five quests in the journal (a delivery, a fishing quest, a kill quest, a gathering quest and a meeting quest listing 26 villagers), 5000 calls each, on the test PC:

| What | Time | Notes |
| --- | --- | --- |
| Work out the quest facts and the marker plan | about 1.1 microseconds | Called through reflection by the test, which adds some time and about 860 bytes. The mod's own allocation is the five small fact records |
| Ask for a quest's notes while they are stored | about 0.06 microseconds | The quest log does this every frame it is open |

At two refreshes a second the first number is far below anything a player could notice. The drawing of a marker was not timed on its own. It is a few sprite draws for each marked person or monster in view.

The first time a fish's places are worked out was not measured. It walks the location data once, and the result is kept.
