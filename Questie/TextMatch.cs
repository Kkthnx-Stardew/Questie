namespace Questie
{
    /// <summary>
    /// Whole-word matching for the names a quest uses in its own words. A name also matches its simple English plural, so
    /// "void souls" finds the "Void Soul" an expansion's quest asks for.
    /// </summary>
    internal static class TextMatch
    {
        /// <summary>The plural forms of a name: "Void Souls", "Peaches", "Blueberries", "Boxes" and "Potatoes".</summary>
        public static IEnumerable<string> Plurals(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
                yield break;

            var last = char.ToLowerInvariant(name[^1]);
            if (last is 's' or 'x' or 'z' || EndsWith(name, "ch") || EndsWith(name, "sh"))
            {
                yield return name + "es";
            }
            else if (last == 'y' && !IsVowel(name[^2]))
            {
                yield return name[..^1] + "ies";
            }
            else if (last == 'o')
            {
                // potato and tomato take "es", avocado and taro take "s"
                yield return name + "s";
                yield return name + "es";
            }
            else
            {
                yield return name + "s";
            }
        }

        /// <summary>Where the name or one of its plurals starts, and how many characters matched, or -1.</summary>
        public static int IndexOfName(string text, string name, out int length)
        {
            var at = IndexOfWord(text, name);
            if (at >= 0)
            {
                length = name.Length;
                return at;
            }

            foreach (var plural in Plurals(name))
            {
                at = IndexOfWord(text, plural);
                if (at >= 0)
                {
                    length = plural.Length;
                    return at;
                }
            }

            length = 0;
            return -1;
        }

        /// <summary>Where the word starts in the text as a whole word, or -1.</summary>
        public static int IndexOfWord(string text, string word)
        {
            var at = 0;
            while ((at = text.IndexOf(word, at, StringComparison.CurrentCultureIgnoreCase)) >= 0)
            {
                var before = at == 0 || !char.IsLetterOrDigit(text[at - 1]);
                var after = at + word.Length >= text.Length || !char.IsLetterOrDigit(text[at + word.Length]);
                if (before && after)
                    return at;
                at += word.Length;
            }
            return -1;
        }

        private static bool EndsWith(string name, string suffix) => name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);

        private static bool IsVowel(char c) => char.ToLowerInvariant(c) is 'a' or 'e' or 'i' or 'o' or 'u';
    }
}
