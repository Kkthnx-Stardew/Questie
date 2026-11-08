namespace Questie
{
    /// <summary>One thing a donation chest still asks for, as the special order counts it.</summary>
    /// <param name="Text">The order's own wording for it.</param>
    /// <param name="Counted">Whether the order counts donations for this line. A line that only describes a step, such as where to gather, has no count.</param>
    /// <param name="IconTag">The single item tag the order accepts for this line, such as item_hardwood, or null when it takes several kinds of item.</param>
    public readonly record struct DonationNeed(string Text, int Have, int Need, bool Counted = true, string? IconTag = null)
    {
        public bool Done => Counted && Have >= Need;
    }

    /// <summary>Turns a donation need into the progress line of the panel. This file has no game dependencies so the tests can compile it.</summary>
    public static class DonationLines
    {
        /// <summary>
        /// The one item an objective takes, from the tags the order accepts. An order lists the tags an item must have, such as
        /// "item_hardwood", and when there is one list and it is that one tag alone it names one item. A list of alternatives, a colour or a
        /// category names no single item, so it gets no picture.
        /// </summary>
        public static string? IconTag(IEnumerable<string> tagSets)
        {
            // several lists are alternatives, so an item that fits any one of them is taken, and no single item can be named
            var lists = tagSets.ToList();
            var first = lists.Count == 1 ? lists[0].Trim() : null;
            if (string.IsNullOrEmpty(first) || first.Contains(',') || first.Contains('/') || !first.StartsWith("item_", StringComparison.Ordinal))
                return null;
            return first;
        }

        /// <summary>"12 of 40, 28 to go", or "Done". A need of one is a yes or no, so it only says Done.</summary>
        public static string Progress(DonationNeed need, IHintTexts texts)
        {
            if (!need.Counted)
                return "";
            if (need.Done)
                return texts.Get("donate.done");
            if (need.Need <= 1)
                return "";
            return texts.Get("donate.progress", ("have", need.Have.ToString()), ("need", need.Need.ToString()), ("left", (need.Need - need.Have).ToString()));
        }
    }
}
