using DiscordBot.Modules.AnimeFeed.Models;
using DiscordBot.Modules.GuildConfig;
using DiscordBot.Modules.GuildConfig.Models;
using DiscordBot.Modules.GuildLogging;
using DiscordBot.Services;

namespace DiscordBot.Modules.AnimeFeed;

public class AnimeListService : ServiceWithJsonData<Anime>
{
    private readonly DiscordChatService _chatService;
    private readonly GuildConfigService _guildConfigService;
    private readonly GuildLoggingService _guildLoggingService;

    private bool _contentChanged = false;

    public AnimeListService(
            DiscordChatService chatService,
            GuildConfigService guildConfigService,
            GuildLoggingService guildLoggingService)
    {
        _chatService = chatService;
        _guildConfigService = guildConfigService;
        _guildLoggingService = guildLoggingService;

        _guildConfigService.Components.Add(BuildConfig);
    }

    protected override string ModulePath => "animeFeed";

    protected override string ModuleJson => "animeList.json";

    public async Task UpdateAnimeList(IEnumerable<Anime> animeList)
    {
        foreach (var guild in moduleData)
        {
            try
            {
                ulong weebChannelId = ulong.Parse((string?) _guildConfigService.GetGuildConfigValue(guild.Key, ModulePath) ?? "0") ;


                foreach (var guildAnime in guild.Value)
                {
                    var anime = animeList.Where(x => x.Equals(guildAnime)).FirstOrDefault();

                    if (anime is null || guildAnime.Episode == anime.Episode)
                        continue;

                    guildAnime.Episode = anime.Episode;
                    guildAnime.Url = anime.Url;
                    guildAnime.Id = anime.Id;

                    if ((guildAnime.Subscribers ?? []).Count > 0 && weebChannelId > 0)
                        await _chatService.SendMessage(weebChannelId, guildAnime.GetUpdateMessage());
                    _contentChanged = true;
                }
            }
            catch (Exception ex)
            {
                if (ex is IloException iloEx)
                {
                    Console.WriteLine(guild.Key + ":" + ex.Message);
                    _guildLoggingService.GuildLog(guild.Key, iloEx.Message, iloEx.ToEmbed());
                }
                else
                {
                    Console.WriteLine(guild.Key + ":" + ex.Message);
                    _guildLoggingService.GuildLog(guild.Key, ex.Message);
                }
            }
        }

        if (_contentChanged)
            SynchronizeJson();
    }

    public void AddAnimeSubscriber(ulong guildId, ulong userId, Anime anime)
    {
        List<Anime> guildAnimeList = GetGuildData(guildId);

        var listEntry = guildAnimeList.Where(x => x.Equals(anime)).FirstOrDefault();

        if (listEntry is null)
        {
            moduleData[guildId].Add(anime);
            listEntry = anime;
        }

        listEntry.Subscribers ??= [];

        // check if user is already on the list
        if (!listEntry.Subscribers.Contains(userId))
        {
            listEntry.Subscribers.Add(userId);
            _contentChanged = true;
        }
    }

    public void RemoveAnimeSubscriber(ulong guildId, ulong userId, Anime anime)
    {
        var listEntry = GetGuildData(guildId)
                        .Where(x => x.Equals(anime))
                        .FirstOrDefault();

        if (listEntry is null ||
            listEntry.Subscribers is null ||
            !listEntry.Subscribers.Contains(userId))
            return;

        listEntry.Subscribers.Remove(userId);
        _contentChanged = true;
    }

    public IEnumerable<string?> GetUserAnimeList(ulong guildId, ulong userId)
    {
        List<Anime> guildAnimeList = GetGuildData(guildId);

        var userAnimeList = guildAnimeList.Where(x => x.Subscribers is not null
                                           && x.Subscribers.Contains(userId))
                                          .Select(x => x.Name)
                                          .ToList()
                                          .DefaultIfEmpty();

        return userAnimeList is null
            ? throw new Exception("Ty Pajacu! Nie obserwujesz żadnego anime!")
            : userAnimeList;
    }

    private static ComponentBuilder BuildConfig(ComponentBuilder builder, SocketInteractionContext context)
    {
        var menuBuilder = new SelectMenuBuilder()
        .WithPlaceholder("Przypominajka anime - wybierz kanał")
        .WithCustomId("animeFeed")
        .WithMinValues(1)
        .WithMaxValues(1);

        IReadOnlyCollection<SocketGuildChannel> guildChannels = context.Guild.Channels;

        foreach (SocketGuildChannel channel in guildChannels)
        {
            if (channel.GetChannelType() is ChannelType.Text)
            {
                menuBuilder.AddOption($"{channel.Name}", $"{channel.Id}");
            }
        }
        menuBuilder.AddOption("Wyłącz", "0", "Wyłącza funkcje");
        return builder.WithSelectMenu(menuBuilder);
    }
}
