using StardewValley;
using StardewValley.GameData.Machines;

namespace Questie
{
    /// <summary>
    /// Finds which machines make an item out of another item, from the game's own machine data, so a quest for pale ale can say it is
    /// made in a keg from hops. Only rules that name one item in and one item out are used. A rule that makes whatever was put in
    /// (wine from any fruit, jelly, pickles) names no single item and says nothing.
    /// </summary>
    internal sealed class MachineLookup
    {
        private static readonly IReadOnlyList<MadeWay> None = Array.Empty<MadeWay>();

        private Dictionary<string, List<MadeWay>>? _byOutput;

        /// <summary>Forgets the index. Called when the machine data or the language changes.</summary>
        public void Reset() => _byOutput = null;

        public IReadOnlyList<MadeWay> Find(string qualifiedItemId)
        {
            if (string.IsNullOrWhiteSpace(qualifiedItemId))
                return None;
            if (_byOutput is null)
            {
                try
                {
                    _byOutput = Build();
                }
                catch
                {
                    // a machine from another mod that does not look the way this expects must not take the rest of the notes with it
                    _byOutput = new Dictionary<string, List<MadeWay>>(StringComparer.Ordinal);
                }
            }
            return _byOutput.TryGetValue(qualifiedItemId, out var ways) ? ways : None;
        }

        private static Dictionary<string, List<MadeWay>> Build()
        {
            var index = new Dictionary<string, List<MadeWay>>(StringComparer.Ordinal);
            foreach (var (machineId, machine) in DataLoader.Machines(Game1.content))
            {
                if (machine?.OutputRules is null)
                    continue;
                var machineName = MachineName(machineId);
                foreach (var rule in machine.OutputRules)
                {
                    if (rule.Triggers is null || rule.OutputItem is null)
                        continue;
                    foreach (var trigger in rule.Triggers)
                    {
                        if (trigger.Trigger != MachineOutputTrigger.ItemPlacedInMachine || string.IsNullOrWhiteSpace(trigger.RequiredItemId))
                            continue;
                        var input = ItemRegistry.QualifyItemId(trigger.RequiredItemId);
                        if (input is null || !ItemRegistry.Exists(input))
                            continue;
                        foreach (var output in rule.OutputItem)
                        {
                            var made = string.IsNullOrWhiteSpace(output.ItemId) ? null : ItemRegistry.QualifyItemId(output.ItemId);
                            // an item query such as DROP_IN_PRESERVE is not an item, so it does not exist and is left out
                            if (made is null || made == input || !ItemRegistry.Exists(made))
                                continue;
                            if (!index.TryGetValue(made, out var ways))
                                index[made] = ways = new List<MadeWay>();
                            if (!ways.Any(w => w.Machine == machineName && w.InputItemId == input))
                                ways.Add(new MadeWay(machineName, input));
                        }
                    }
                }
            }
            return index;
        }

        private static string MachineName(string machineId)
        {
            var qualified = machineId.Contains(')') ? machineId : "(BC)" + machineId;
            return ItemRegistry.GetDataOrErrorItem(qualified).DisplayName;
        }
    }
}
