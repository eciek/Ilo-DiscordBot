using DiscordBot.Modules.AnimeFeed.Models;
using System.Text.RegularExpressions;
using System.Xml;
using static System.Net.WebRequestMethods;

namespace DiscordBot.Modules.AnimeFeed
{
    public partial class AnimeFeedService
    {
        const string _subsPleaseUrl = @"https://nyaa.si/?page=rss&q=%5BSubsPlease%5D+1080&c=1_2&f=0";
        const string _nyaaSiiFilter = @"\[(.*)\] (.*) - (.\d) \(1080p\)";
        const string _nyaaSiiIdFilter = @"https:\/\/nyaa\.si\/view\/(\d*)";

        readonly HttpClient _httpClient;
        private List<Anime> _animeList;

        public AnimeFeedService()
        {
            _httpClient = new HttpClient();
            _animeList = [];
        }

        public async Task UpdateAnimeFeedAsync( CancellationToken ct = default)
        {
            var nyaaXml = await GetSubsXML(ct);
            _animeList = [.. ParseFromXml(nyaaXml).OrderByDescending(x => x.Id).DistinctBy(x=>x.Name)];
        }

        public IEnumerable<Anime> GetAnimeList() =>
            _animeList;

        public async Task<Anime> MatchAnime(string query)
        {
            try
            {
                var anime = _animeList.SingleOrDefault(x => x.Name!.Contains(query)) ?? throw new Exception();

                return anime;

            }
            catch(InvalidOperationException)
            {
                var animeNames = _animeList.Where(x => x.Name!.Contains(query)).Select(x=> x.Name).ToArray();

                string errMsg = "Znalazłam kilka pasujących anime do twojego opisu:\n" +
                    String.Join(",\n", animeNames) + ".\n"+"Które Cie interesuje?";

                throw new Exception(errMsg);
            }
            catch (Exception)
            {
                // update anime list and try again, just to be sure
                await UpdateAnimeFeedAsync();
                var anime = _animeList.FirstOrDefault(x => x.Name!.Contains(query));

                return anime ?? throw new Exception("Nie wiem o które anime Ci chodzi. Czy pojawił się już chociaż jeden odcinek?");
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

                var idRegex = NyaaIdRegex();
                Match idMatch = idRegex.Match(node["guid"]!.InnerText);

                Anime anime = new()
                {
                    Episode = match.Groups[3].Value,
                    Name = match.Groups[2].Value,
                    Url = node["guid"]!.InnerText,
                    Id = Int32.Parse(idMatch.Groups[1].Value),
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
}
