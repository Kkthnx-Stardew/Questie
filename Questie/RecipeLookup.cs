using System.Globalization;
using StardewValley;

namespace Questie
{
    /// <summary>Finds how an item is crafted or cooked, from the game's own recipe data.</summary>
    internal sealed class RecipeLookup
    {
        private readonly record struct Entry(string Name, bool Cooking, string? Unlock);

        /// <summary>The recipe that makes each item, by the item's qualified id. Built the first time it is needed.</summary>
        private Dictionary<string, Entry>? _byOutput;

        /// <summary>Forgets the index. Called when the recipe data changes.</summary>
        public void Reset() => _byOutput = null;

        public RecipeInfo? Find(string qualifiedItemId)
        {
            _byOutput ??= BuildIndex();
            if (!_byOutput.TryGetValue(qualifiedItemId, out var entry))
                return null;

            var player = Game1.player;
            var known = entry.Cooking ? player.cookingRecipes.ContainsKey(entry.Name) : player.craftingRecipes.ContainsKey(entry.Name);
            if (known)
                return new RecipeInfo(entry.Cooking, true, Ingredients(entry), null);
            return new RecipeInfo(entry.Cooking, false, Array.Empty<string>(), Unlock(entry.Unlock));
        }

        private static Dictionary<string, Entry> BuildIndex()
        {
            var index = new Dictionary<string, Entry>(StringComparer.Ordinal);
            Add(index, CraftingRecipe.craftingRecipes, cooking: false);
            Add(index, CraftingRecipe.cookingRecipes, cooking: true);
            return index;
        }

        private static void Add(Dictionary<string, Entry> index, Dictionary<string, string>? recipes, bool cooking)
        {
            if (recipes is null)
                return;
            foreach (var (name, text) in recipes)
            {
                // crafting: ingredients / unused / output / big craftable / unlock / name. cooking: ingredients / unused / output / unlock / name.
                var fields = text.Split('/');
                if (fields.Length < 4)
                    continue;
                var output = fields[2].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (output.Length == 0)
                    continue;

                var bigCraftable = !cooking && fields[3].Equals("true", StringComparison.OrdinalIgnoreCase);
                var qualified = output[0].Contains(')') ? output[0] : (bigCraftable ? "(BC)" : "(O)") + output[0];
                var unlockIndex = cooking ? 3 : 4;
                var unlock = fields.Length > unlockIndex ? fields[unlockIndex] : null;
                index.TryAdd(qualified, new Entry(name, cooking, unlock));
            }
        }

        private static List<string> Ingredients(Entry entry)
        {
            var list = new List<string>();
            var recipe = new CraftingRecipe(entry.Name, entry.Cooking);
            var items = Game1.player.Items;
            foreach (var (id, count) in recipe.recipeList)
            {
                // the game names an ingredient itself, including a whole category such as any egg or any fish
                var label = recipe.getNameFromIndex(id);
                int have;
                if (int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var category) && category < 0)
                {
                    // a category is counted by what an item is. The game's -777 stands for seasonal wild seeds and is not counted here.
                    have = 0;
                    for (var i = 0; i < items.Count; i++)
                    {
                        if (items[i] is { } item && item.Category == category)
                            have += item.Stack;
                    }
                }
                else
                    have = items.CountId(ItemRegistry.QualifyItemId(id) ?? id);
                list.Add($"{label} ({have} of {count})");
            }
            return list;
        }

        /// <summary>Reads the game's unlock text: "f Gus 3" is hearts with a villager, "s Fishing 2" a skill level, anything else some other way.</summary>
        private static RecipeUnlock? Unlock(string? text)
        {
            if (string.IsNullOrWhiteSpace(text) || text is "default" or "null" or "none")
                return null;
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3 && int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var amount))
            {
                if (parts[0] == "f")
                    return new RecipeUnlock(UnlockKind.Friendship, NPC.GetDisplayName(parts[1]), null, amount);
                if (parts[0] == "s")
                    return new RecipeUnlock(UnlockKind.Skill, null, Farmer.getSkillDisplayNameFromIndex(Farmer.getSkillNumberFromName(parts[1])), amount);
            }
            return new RecipeUnlock(UnlockKind.Other, null, null, 0);
        }
    }
}
