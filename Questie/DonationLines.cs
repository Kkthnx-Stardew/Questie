namespace Questie
{
    /// <summary>One thing a donation chest still asks for, as the special order counts it.</summary>
    /// <param name="Text">The order's own wording for it.</param>
    /// <param name="Counted">Whether the order counts donations for this line. A line that only describes a step, such as where to gather, has no count.</param>
    public readonly record struct DonationNeed(string Text, int Have, int Need, bool Counted = true)
    {
        public bool Done => Counted && Have >= Need;
    }

    /// <summary>Turns a donation need into the second line of the panel. This file has no game dependencies so the tests can compile it.</summary>
    public static class DonationLines
    {
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
