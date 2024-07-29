using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace DiscordBot.Modules.Twitter;

public partial class TwitterService
{
    private const string _twitterRegexString = @"(https:\/\/+(x|twitter)[^\?\s]*)";

    public static Regex TwitterFilter { get; } = TwitterRegex();

    /// <summary>
    /// Converts x.com / twitter.com URL to fixupx.com url, so discord embeds will work
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static string FixupUrl(string url)
    {
        var match = TwitterRegex().Match(url);
        if (match.Success)
        {
            return match.Groups[0].Value
                .Replace(@"https://twitter.com/", @"https://x.com/")
                .Replace(@"https://x.com/", @"https://fixupx.com/");
        }

        return string.Empty;
    }

    [GeneratedRegex(_twitterRegexString)]
    private static partial Regex TwitterRegex();
}

