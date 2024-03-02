using System.Text.RegularExpressions;

namespace DiscordBot.Helpers
{
    public static class StringExtensions
    {
        public static string GenerateSlug(this string str)
        {
            // invalid chars
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // trim 
            str = str.Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens
            return str;
        }
    }
}
