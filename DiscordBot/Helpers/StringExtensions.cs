using System.Text.RegularExpressions;

namespace DiscordBot.Helpers
{
    public static partial class StringExtensions
    {
        public static string GenerateSlug(this string str)
        {
            // invalid chars
            str = InvalidChars().Replace(str, "");
            // convert multiple spaces into one space
            str = TrimSpaces().Replace(str, " ").Trim();
            // trim 
            str = str.Trim();
            str = Hyphens().Replace(str, "-"); // hyphens
            return str;
        }

        public static string TrimLetters(this string str)
        {
            return LetterFilter().Replace(str, "");
        }

        [GeneratedRegex("[^0-9]")]
        private static partial Regex LetterFilter();
        [GeneratedRegex(@"[^a-z0-9\s-]")]
        private static partial Regex InvalidChars();
        [GeneratedRegex(@"\s+")]
        private static partial Regex TrimSpaces();
        [GeneratedRegex(@"\s")]
        private static partial Regex Hyphens();

        public static string ToBooruSlug(this string? str)
            => (str ?? string.Empty).Trim().Replace(" ", "_");

        public static string TrimmedName(this string? str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return string.Empty;
            var match = TrimSeason().Match(str);

            if (!match.Success)
            {
                match = TrimYear().Match(str);
                if (!match.Success)
                {
                    return str;
                }
            }

            return match.Groups[1].Value;
        }

        [GeneratedRegex("(.*) S\\d$")]
        private static partial Regex TrimSeason();

        [GeneratedRegex("(.*) (d)$")]
        private static partial Regex TrimYear();
    }
}
