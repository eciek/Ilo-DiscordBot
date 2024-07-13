using SixLabors.ImageSharp;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace DiscordBot.Services;
public class BooruService
{
    private readonly HttpClient _httpClient;
    private readonly string apiKey;
    private readonly string apiUser;

    private static string CachePath => $"cache/{DateTime.Now:yyyy-MM-dd}/";

    private const string _urlFormat = @"https://danbooru.donmai.us/posts.json?limit={0}&tags=age:<3year {1}";
    //{0} {1}  "

    private string GetQueryUrl(string tags, int imageCount)
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
    }

    public IEnumerable<string> GetBooruImage(string tags, int imageCount = 5, bool isNsfw = false)
    {
        var jsonResp = string.Empty;

        if (String.IsNullOrEmpty(apiKey))
            throw new Exception("booruKey is missing!");

        if (String.IsNullOrEmpty(apiUser))
            throw new Exception("apiUser is missing");

        var queryString = GetQueryUrl(tags, imageCount) + (isNsfw ? "is:sfw" : "is:nsfw");

        using HttpRequestMessage request = new(HttpMethod.Get, queryString);
        
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
            System.Text.Encoding.ASCII.GetBytes($"{apiUser}:{apiKey}")));

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

        var images = imageUrls.Select(x => SaveImage(x).Result);

        return images;
    }

    private static async Task<string> SaveImage(string imageUrl)
    {
        using HttpClient client = new();

        string imagePath = Path.Combine(CachePath, imageUrl.Split('/').Last());

        var imageBytes = await client.GetByteArrayAsync(imageUrl);
        await File.WriteAllBytesAsync(imagePath, imageBytes);

        return imagePath;
    }

    private IEnumerable<string> GetImageUrlsFromJson(string jsonResp)
    {
        List<string> result = [];

        [
    {
            "id": 7793755,
        "created_at": "2024-06-30T19:41:17.940-04:00",
        "uploader_id": 1199268,
        "score": 3,
        "source": "file://AddText_06-30-06.07.22-01.jpeg",
        "md5": "06958b57fab5f378dbdf2771a54d6b6f",
        "last_comment_bumped_at": null,
        "rating": "e",
        "image_width": 2048,
        "image_height": 2353,
        "tag_string": "1boy 1girl ai-generated barefoot breasts brown_hair cardcaptor_sakura covered_nipples cum cum_in_pussy feet foot_focus footjob foreshortening green_eyes highres kinomoto_sakura non-web_source short_hair small_breasts soles toes",
        "fav_count": 5,
        "file_ext": "jpg",
        "last_noted_at": null,
        "parent_id": null,
        "has_children": false,
        "approver_id": null,
        "tag_count_general": 17,
        "tag_count_artist": 0,
        "tag_count_character": 1,
        "tag_count_copyright": 1,
        "file_size": 1001973,
        "up_score": 5,
        "down_score": -2,
        "is_pending": false,
        "is_flagged": false,
        "is_deleted": true,
        "tag_count": 22,
        "updated_at": "2024-07-01T07:40:43.185-04:00",
        "is_banned": false,
        "pixiv_id": null,
        "last_commented_at": null,
        "has_active_children": false,
        "bit_flags": 0,
        "tag_count_meta": 3,
        "has_large": true,
        "has_visible_children": false,
        "media_asset": {
                "id": 21908533,
            "created_at": "2024-06-30T19:35:17.221-04:00",
            "updated_at": "2024-06-30T19:35:26.858-04:00",
            "md5": "06958b57fab5f378dbdf2771a54d6b6f",
            "file_ext": "jpg",
            "file_size": 1001973,
            "image_width": 2048,
            "image_height": 2353,
            "duration": null,
            "status": "active",
            "file_key": "0p42oYwRa",
            "is_public": true,
            "pixel_hash": "eea442c5038d3bbec953d808a35fb25a",
            "variants": [
                {
                    "type": "180x180",
                    "url": "https://cdn.donmai.us/180x180/06/95/06958b57fab5f378dbdf2771a54d6b6f.jpg",
                    "width": 157,
                    "height": 180,
                    "file_ext": "jpg"
                },
                {
                    "type": "360x360",
                    "url": "https://cdn.donmai.us/360x360/06/95/06958b57fab5f378dbdf2771a54d6b6f.jpg",
                    "width": 313,
                    "height": 360,
                    "file_ext": "jpg"
                },
                {
                    "type": "720x720",
                    "url": "https://cdn.donmai.us/720x720/06/95/06958b57fab5f378dbdf2771a54d6b6f.webp",
                    "width": 627,
                    "height": 720,
                    "file_ext": "webp"
                },
                {
                    "type": "sample",
                    "url": "https://cdn.donmai.us/sample/06/95/sample-06958b57fab5f378dbdf2771a54d6b6f.jpg",
                    "width": 850,
                    "height": 977,
                    "file_ext": "jpg"
                },
                {
                    "type": "original",
                    "url": "https://cdn.donmai.us/original/06/95/06958b57fab5f378dbdf2771a54d6b6f.jpg",
                    "width": 2048,
                    "height": 2353,
                    "file_ext": "jpg"
                }
            ]
        },
        "tag_string_general": "1boy 1girl barefoot breasts brown_hair covered_nipples cum cum_in_pussy feet foot_focus footjob foreshortening green_eyes short_hair small_breasts soles toes",
        "tag_string_character": "kinomoto_sakura",
        "tag_string_copyright": "cardcaptor_sakura",
        "tag_string_artist": "",
        "tag_string_meta": "ai-generated highres non-web_source",
        "file_url": "https://cdn.donmai.us/original/06/95/06958b57fab5f378dbdf2771a54d6b6f.jpg",
        "large_file_url": "https://cdn.donmai.us/sample/06/95/sample-06958b57fab5f378dbdf2771a54d6b6f.jpg",
        "preview_file_url": "https://cdn.donmai.us/180x180/06/95/06958b57fab5f378dbdf2771a54d6b6f.jpg"
    },
    {
            "id": 7772931,
        "created_at": "2024-06-26T14:01:50.904-04:00",
        "uploader_id": 1085392,
        "score": 7,
        "source": "https://i.pximg.net/img-original/img/2024/05/07/23/33/07/118531315_p2.jpg",
        "last_comment_bumped_at": null,
        "rating": "e",
        "image_width": 883,
        "image_height": 1250,
        "tag_string": "1boy 1girl ajitarou_(setsu) ass brown_hair cardcaptor_sakura censored collar erection hetero highres kinomoto_sakura loli messy_hair mosaic_censoring nude one_eye_closed open_mouth red_collar short_hair testicles variant_set",
        "fav_count": 7,
        "file_ext": "jpg",
        "last_noted_at": null,
        "parent_id": 7772928,
        "has_children": false,
        "approver_id": null,
        "tag_count_general": 17,
        "tag_count_artist": 1,
        "tag_count_character": 1,
        "tag_count_copyright": 1,
        "file_size": 156974,
        "up_score": 8,
        "down_score": -1,
        "is_pending": false,
        "is_flagged": false,
        "is_deleted": true,
        "tag_count": 22,
        "updated_at": "2024-06-29T15:00:00.522-04:00",
        "is_banned": false,
        "pixiv_id": 118531315,
        "last_commented_at": null,
        "has_active_children": false,
        "bit_flags": 0,
        "tag_count_meta": 2,
        "has_large": true,
        "has_visible_children": false,
        "media_asset": {
                "id": 20861599,
            "created_at": "2024-05-07T11:21:29.307-04:00",
            "updated_at": "2024-05-07T11:21:31.013-04:00",
            "file_ext": "jpg",
            "file_size": 156974,
            "image_width": 883,
            "image_height": 1250,
            "duration": null,
            "status": "active",
            "is_public": true,
            "pixel_hash": "c8bfcb67fb7a4a68d99121bcaa041e31"
        },
        "tag_string_general": "1boy 1girl ass brown_hair censored collar erection hetero loli messy_hair mosaic_censoring nude one_eye_closed open_mouth red_collar short_hair testicles",
        "tag_string_character": "kinomoto_sakura",
        "tag_string_copyright": "cardcaptor_sakura",
        "tag_string_artist": "ajitarou_(setsu)",
        "tag_string_meta": "highres variant_set"
    },
    {
            "id": 7772928,
        "created_at": "2024-06-26T13:59:35.802-04:00",
        "uploader_id": 1085392,
        "score": 8,
        "source": "https://i.pximg.net/img-original/img/2024/05/07/23/33/07/118531315_p1.jpg",
        "last_comment_bumped_at": null,
        "rating": "e",
        "image_width": 883,
        "image_height": 1250,
        "tag_string": "1boy 1girl ajitarou_(setsu) ass brown_hair cardcaptor_sakura censored collar erection hetero highres kinomoto_sakura loli messy_hair mosaic_censoring nude open_mouth red_collar short_hair testicles",
        "fav_count": 8,
        "file_ext": "jpg",
        "last_noted_at": null,
        "parent_id": null,
        "has_children": true,
        "approver_id": null,
        "tag_count_general": 16,
        "tag_count_artist": 1,
        "tag_count_character": 1,
        "tag_count_copyright": 1,
        "file_size": 153653,
        "up_score": 9,
        "down_score": -1,
        "is_pending": false,
        "is_flagged": false,
        "is_deleted": true,
        "tag_count": 20,
        "updated_at": "2024-06-29T15:00:00.553-04:00",
        "is_banned": false,
        "pixiv_id": 118531315,
        "last_commented_at": null,
        "has_active_children": false,
        "bit_flags": 0,
        "tag_count_meta": 1,
        "has_large": true,
        "has_visible_children": true,
        "media_asset": {
                "id": 20861597,
            "created_at": "2024-05-07T11:21:29.229-04:00",
            "updated_at": "2024-05-07T11:21:30.724-04:00",
            "file_ext": "jpg",
            "file_size": 153653,
            "image_width": 883,
            "image_height": 1250,
            "duration": null,
            "status": "active",
            "is_public": true,
            "pixel_hash": "35dba51e9f2d6b13c3fd5247a7506385"
        },
        "tag_string_general": "1boy 1girl ass brown_hair censored collar erection hetero loli messy_hair mosaic_censoring nude open_mouth red_collar short_hair testicles",
        "tag_string_character": "kinomoto_sakura",
        "tag_string_copyright": "cardcaptor_sakura",
        "tag_string_artist": "ajitarou_(setsu)",
        "tag_string_meta": "highres"
    }
]

        return result;
    }
}
