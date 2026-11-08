using System.Globalization;

namespace Questie
{
    /// <summary>Reads the parts of a fish's entry in the game's Data/Fish text and turns them into words.</summary>
    public static class FishTimes
    {
        /// <summary>
        /// The times of day a fish bites, from the time field of Data/Fish: pairs of 24 hour clock values such as "600 2600"
        /// (up to 2am the next day). Returns an empty list when the field is blank or cannot be read.
        /// </summary>
        public static List<(int Start, int End)> ParseWindows(string? field)
        {
            var windows = new List<(int, int)>();
            if (string.IsNullOrWhiteSpace(field))
                return windows;

            var parts = field.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i + 1 < parts.Length; i += 2)
            {
                if (int.TryParse(parts[i], NumberStyles.None, CultureInfo.InvariantCulture, out var start)
                    && int.TryParse(parts[i + 1], NumberStyles.None, CultureInfo.InvariantCulture, out var end)
                    && end > start)
                    windows.Add((start, end));
            }
            return windows;
        }

        /// <summary>Whether the windows cover the whole day, so no time needs to be mentioned.</summary>
        public static bool IsAllDay(IReadOnlyList<(int Start, int End)> windows) =>
            windows.Count == 1 && windows[0].Start <= 600 && windows[0].End >= 2600;

        /// <summary>A clock value such as 1830 as "6:30pm". Values past 2400 count as the next morning, and 2600 is "2am".</summary>
        public static string FormatTime(int clock)
        {
            var hour = clock / 100 % 24;
            var minute = clock % 100;
            var suffix = hour >= 12 ? "pm" : "am";
            var twelve = hour % 12 == 0 ? 12 : hour % 12;
            return minute == 0 ? $"{twelve}{suffix}" : $"{twelve}:{minute:00}{suffix}";
        }

        /// <summary>The windows as text, such as "6am to 7pm" or "6am to 11am, 5pm to 2am".</summary>
        public static string FormatWindows(IReadOnlyList<(int Start, int End)> windows, Func<int, string>? format = null) =>
            string.Join(", ", windows.Select(w => $"{(format ?? FormatTime)(w.Start)} to {(format ?? FormatTime)(w.End)}"));
    }
}
