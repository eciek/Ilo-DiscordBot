using DiscordBot.Modules.AnimeFeed.Models;
using DiscordBot.Services;
using Newtonsoft.Json;

namespace DiscordBot.Modules.AnimeFeed;

public class AnimeListService
{
    private readonly DiscordChatService _chatService;
    private readonly TimerService _timerService;
    private const string _dataRoot = "data";
    // 0 - guildId
    private const string _animeListPath = _dataRoot + "/{0}/animeFeed/animeList.json";
    private readonly Dictionary<ulong, List<Anime>> _animeList;

    public AnimeListService(
            DiscordChatService chatService,
            TimerService timerService
            ) {
        _chatService = chatService;
        _timerService = timerService;

        string[] registeredGuilds;
        _animeList = [];

        try
        {
            registeredGuilds = Directory.GetDirectories(_dataRoot);
        }
        catch (DirectoryNotFoundException)
        {
            Directory.CreateDirectory(_dataRoot);
            registeredGuilds = Directory.GetDirectories(_dataRoot);
        }

        foreach (var registeredGuild in registeredGuilds)
            _animeList.Add(ulong.Parse(registeredGuild), []);
        LoadFromJson();
    }

    public async Task UpdateAnimeList(List<Anime> animeList) 
    {
        foreach(var guild in _animeList) 
        {
            // temporal measures
            ulong weebChannelId = 1211407711414124635;

            foreach (var guildAnime in guild.Value) 
            {
               var anime = animeList.Where(x=> x.Equals(guildAnime)).FirstOrDefault();

                if (anime is null || guildAnime.Episode == anime.Episode)
                    continue;

                guildAnime.Episode = anime.Episode;
                guildAnime.Url = anime.Url;
                guildAnime.Id = anime.Id;

                await _chatService.SendMessage(weebChannelId, $"Jest nowy odcinek {anime.Name}!! \n {anime.Url}");
            }
        }
    }

    public void AddAnimeSubscriber(ulong guildId, ulong userId, Anime anime)
    {
        List<Anime> guildAnimeList;
        try
        {
            guildAnimeList = _animeList[guildId];
        }
        catch (KeyNotFoundException)
        {
            AddNewGuild(guildId);
            guildAnimeList = _animeList[guildId];
        }

        var listEntry = guildAnimeList.Where(x=> x.Equals(anime)).FirstOrDefault();

        if (listEntry is null)
        {
            _animeList[guildId].Add(anime);
            listEntry = anime;
        }

        listEntry.Subscribers ??= [];

        // check if user is already on the list
        if ( !listEntry.Subscribers.Contains(userId))
        {
            listEntry.Subscribers.Add(userId);
        }
    }

    public void RemoveAnimeSubscriber(ulong guildId, ulong userId, Anime anime)
    {
        var listEntry = _animeList[guildId].Where(x => x.Equals(anime)).FirstOrDefault();
        if (listEntry is null || listEntry.Subscribers is null || listEntry.Subscribers.Contains(userId))
            return;

        listEntry.Subscribers.Remove(userId);
    }

    private void LoadFromJson()
    {
        foreach (var anime in _animeList)
        {
            var path = String.Format(_animeListPath, anime.Key);
            string json;

            try
            {
                json = File.ReadAllText(path);
            }
            catch (FileNotFoundException) 
            {
                File.Create(path);
                json = string.Empty;
            }

            if (!string.IsNullOrEmpty(json))
                continue;

            _animeList[anime.Key] = JsonConvert.DeserializeObject<List<Anime>>(json)!;
        }
    }

    private void SynchronizeJson()
    {
        foreach (var anime in _animeList)
        {
            var path = String.Format(_animeListPath, anime.Key);
            string json = JsonConvert.SerializeObject(anime);
            File.WriteAllText(path,json);
        }
    }

    private void AddNewGuild(ulong guildId)
        => _animeList.Add(guildId, []);

    public IEnumerable<string?> GetUserAnimeList(ulong guildId, ulong userId)
    {
        List<Anime> guildAnimeList;
        try
        {
            guildAnimeList = _animeList[guildId];
            if (guildAnimeList is null || guildAnimeList.Count == 0)
                throw new Exception();
        }
        catch (KeyNotFoundException)
        {
            throw new Exception("Ty Pajacu! Nie obserwujesz żadnego anime!");
        }

        var userAnimeList = guildAnimeList.Where(x => x.Subscribers is not null && x.Subscribers.Contains(userId))
                                          .Select(x => x.Name)
                                          .ToList()
                                          .DefaultIfEmpty();

        return userAnimeList is null 
            ? throw new Exception("Ty Pajacu! Nie obserwujesz żadnego anime!")
            : userAnimeList;
    }
}
