using System.Text.RegularExpressions;

namespace DiscordBot.Services;
public class BooruService
{
    private readonly HttpClient _httpClient;
    private readonly string apiKey;
    private readonly string apiUser;

    private static string CachePath => $"cache/{DateTime.Now:yyyy-MM-dd}/";

    private const string _urlFormat = @"https://danbooru.donmai.us/posts.json?limit={0}&tags=age:<3year {1} ";
    //{0} {1}  "

    private static string GetQueryUrl(string tags, int imageCount)
        => string.Format(_urlFormat, imageCount, tags);

    public BooruService()
    {
        _httpClient = new HttpClient();

        var sconfig = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .Build();

        apiUser = sconfig.GetValue<string>("BotConfig:booru:user")
        ?? throw new Exception("appsettings.json is not specified properly!");
        apiKey = sconfig.GetValue<string>("BotConfig:booru:key")
            ?? throw new Exception("appsettings.json is not specified properly!");

        var auth = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{apiUser}:{apiKey}"));

        _httpClient.DefaultRequestHeaders.Add("Authorization", auth);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.39.0");
    }

    public async Task<IEnumerable<string>> GetBooruImageAsync(string tags, int imageCount = 5, bool isNsfw = false)
    {
        string jsonResp;

        if (String.IsNullOrEmpty(apiKey))
            throw new Exception("booruKey is missing!");

        if (String.IsNullOrEmpty(apiUser))
            throw new Exception("apiUser is missing");

        var queryString = GetQueryUrl(tags, imageCount) + (isNsfw ? "is:nsfw" : "is:sfw");
        using HttpRequestMessage request = new(HttpMethod.Get, queryString);

        try
        {

            var response = _httpClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();

            jsonResp = response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }

        var imageUrls = GetImageUrlsFromJson(jsonResp);

        List<string> localImages = [];

        foreach (var imageUrl in imageUrls)
        {
            var img = await SaveImage(imageUrl);
            localImages.Add(img);
        }

        return localImages;
    }

    private async Task<string> SaveImage(string imageUrl)
    {
        string imagePath = Path.Combine(CachePath, imageUrl.Split('/').Last());

        var resp = await _httpClient.GetAsync(imageUrl);
        var imageBytes = await resp.Content.ReadAsByteArrayAsync();
        if (!Directory.Exists(CachePath))
            Directory.CreateDirectory(CachePath);
        await File.WriteAllBytesAsync(imagePath, imageBytes);

        return imagePath;
    }

    private static List<string> GetImageUrlsFromJson(string jsonResp)
    {
        List<string> result = [];

        string regexFilter = @"""type"":""original"",""url*"":""([^""]*)""";

        Regex regex = new(regexFilter, RegexOptions.Compiled);

        foreach (Match match in regex.Matches(jsonResp).Cast<Match>())
        {
            result.Add(match.Groups[1].Value);
            ;
        }

        return result;
    }
}
