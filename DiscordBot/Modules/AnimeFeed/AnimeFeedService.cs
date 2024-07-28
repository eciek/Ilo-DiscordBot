using DiscordBot.Helpers;
using DiscordBot.Modules.AnimeFeed.Models;
using System.Text.RegularExpressions;
using System.Xml;

namespace DiscordBot.Modules.AnimeFeed;

public partial class AnimeFeedService()
{
    private const string _subsPleaseUrl = @"https://nyaa.si/?page=rss&q=%5BSubsPlease%5D+1080&c=1_2&f=0";
    private const string _nyaaSiiFilter = @"\[(.*)\] (.*) - (.\d+(?:[\.\,]\d{1,2})?) \(1080p\)";
    private const string _nyaaSiiIdFilter = @"https:\/\/nyaa\.si\/view\/(\d*)";

    private readonly HttpClient _httpClient = new();

    private List<Anime> _animeList = [];

    public async Task UpdateAnimeFeedAsync(CancellationToken ct = default)
    {
        var nyaaXml = await GetSubsXML(ct);
        try
        {
            _animeList = [.. ParseFromXml(nyaaXml)
                .OrderByDescending(x => x.Id)
                .DistinctBy(x => x.Name)];
        }
        catch (Exception ex)
        {
            throw new Exception($"AnimeFeedService.ParseFromXml: Failed to deserialize XML!:\n{ex.Message}");
        }
    }

    public IEnumerable<Anime> GetAnimeList() =>
        _animeList;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Found multiple matching anime</exception>
    /// <exception cref="KeyNotFoundException">Failed to find matching anime</exception>
    public Anime MatchAnime(string query)
    {
        try
        {
            var anime = _animeList.Single(x => x.Name!.Contains(query, StringComparison.CurrentCultureIgnoreCase));

            return anime;

        }
        catch (InvalidOperationException)
        {
            var animeNames = _animeList.Where(x => x.Name!.Contains(query, StringComparison.CurrentCultureIgnoreCase)).Select(x => x.Name).ToArray();

            if (animeNames.Length == 0)
                throw new ArgumentNullException(query);

            string errMsg = String.Join(",\n", animeNames);

            throw new InvalidOperationException(errMsg);
        }
    }

    private async Task<XmlDocument> GetSubsXML(CancellationToken ct = default)
    {
        XmlDocument xmlDoc = new();

        using HttpRequestMessage subsMessage = new(HttpMethod.Get, _subsPleaseUrl);
        try
        {
            var response = _httpClient.SendAsync(subsMessage, ct).Result;
            response.EnsureSuccessStatusCode();
            string animeXml = await response.Content.ReadAsStringAsync(ct);
            xmlDoc.LoadXml(animeXml);
        }
        catch (Exception ex)
        {
            throw new Exception($"AnimeFeedService.GetSubsXML:\n Error on downloading anime list from SubsPlease!:\n {ex.Message}");
        }
        return xmlDoc;
    }

    private static List<Anime> ParseFromXml(XmlDocument xmlDoc)
    {
        List<Anime> animeList = [];
        XmlNode rootNode = xmlDoc.DocumentElement!;
        XmlNodeList xmlList = rootNode["channel"]!.ChildNodes;
        foreach (XmlNode node in xmlList)
        {
            //filter unnecesary results
            if (node.Name != "item")
                continue;

            var regex = NyaaRegex();
            Match match = regex.Match(node["title"]!.InnerText);

            var idRegex = NyaaIdRegex();
            Match idMatch = idRegex.Match(node["guid"]!.InnerText);

            if (match.Groups.Count < 4)
            {
                // batch release is without episode number, ignore them
                continue;
            }

            Anime anime = new()
            {
                Episode = match.Groups[3].Value,
                Name = match.Groups[2].Value,
                Url = node["guid"]!.InnerText,
                Id = Int32.Parse(idMatch.Groups[1].Value),
                BooruName = match.Groups[2].Value.TrimmedName()
            };

            if (String.IsNullOrWhiteSpace(anime.Name))
                continue;
            animeList.Add(anime);
        }
        return animeList;
    }

    [GeneratedRegex(_nyaaSiiFilter)]
    private static partial Regex NyaaRegex();

    [GeneratedRegex(_nyaaSiiIdFilter)]
    private static partial Regex NyaaIdRegex();
}
