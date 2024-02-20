using DiscordBot.Modules.AnimeFeed.Models;
using System.Text.RegularExpressions;
using System.Xml;

namespace DiscordBot.Modules.AnimeFeed
{
    public partial class AnimeFeedService
    {
        readonly HttpClient _httpClient;

        const string _subsPleaseUrl = @"https://nyaa.si/?page=rss&q=%5BSubsPlease%5D+1080&c=1_2&f=0";
        const string _nyaaSiiFilter = @"\[(.*)\] (.*) - (.\d) \(1080p\)";

        public AnimeFeedService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<Anime>> GetAnimeFromRSSAsync(CancellationToken ct = default)
        {
            var nyaaXml = await GetSubsXML(ct);

            var animeList = ParseFromXml(nyaaXml);

            return animeList;
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
                Console.WriteLine(ex.Message);
                throw;
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
                Anime anime = new()
                {
                    Episode = match.Groups[3].Value,
                    Name = match.Groups[2].Value,
                    Url = node["link"]!.InnerText
                };
                animeList.Add(anime);
            }
            return animeList;
        }

        [GeneratedRegex(_nyaaSiiFilter)]
        private static partial Regex NyaaRegex();
    }
}
