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
    private readonly ILogger<AnimeListService> _logger;

    private bool _contentChanged = false;

    public AnimeListService(
            DiscordChatService chatService,
            GuildConfigService guildConfigService,
            GuildLoggingService guildLoggingService,
            ILogger<AnimeListService> logger)
    {
        _chatService = chatService;
        _guildConfigService = guildConfigService;
        _guildLoggingService = guildLoggingService;
        _logger = logger;

        _guildConfigService.Components.Add(BuildConfig);
    }

    protected override string ModulePath => "animeFeed";

    protected override string ModuleJson => "animeList.json";

    public async Task UpdateAnimeList(IEnumerable<Anime> animeList)
    {
        foreach (var guildData in moduleData)
        {
            try
            {
                ulong weebChannelId = _guildConfigService.GetGuildConfigValueAsUlong(guildData.Key, ModulePath);
                if (weebChannelId == 0)
                    { continue; }

                foreach (var guildAnimeList in guildData.Value)
                {
                    var anime = animeList.Where(x => x.Equals(guildAnimeList)).FirstOrDefault();

                    if (anime is null || guildAnimeList.Episode == anime.Episode)
                        continue;

                    guildAnimeList.Episode = anime.Episode;
                    guildAnimeList.Url = anime.Url;
                    guildAnimeList.Id = anime.Id;

                    if ((guildAnimeList.Subscribers ?? []).Count > 0 && weebChannelId > 0)
                        await _chatService.SendMessage(weebChannelId, guildAnimeList.GetUpdateMessage());
                    _contentChanged = true;
                }
            }
            catch (Exception ex)
            {   
                _logger.LogError("AnimeListService.UpdateAnimeList: Unhandled excepption: \n {ex}", ex.Message);
                _guildLoggingService.GuildLog(guildData.Key, $"AnimeListService.UpdateAnimeList: Unhandled exception \n {ex.Message}");               
            }
        }

        if (_contentChanged)
            SynchronizeJson();
    }

    public void AddAnimeSubscriber(ulong guildId, ulong userId, Anime anime)
    {
        List<Anime> guildAnimeList = GetGuildData(guildId);

        var listEntry = guildAnimeList
            .Where(x => x.Equals(anime))
            .FirstOrDefault();

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

    public void RemoveAnimeSubscriber(ulong guildId, ulong userId, Anime? anime = null)
    {
        foreach (var listEntry in GetGuildData(guildId)
            .Where(x => anime is not null && x.Equals(anime)))
        {
            if (listEntry is null ||
                listEntry.Subscribers is null ||
                !listEntry.Subscribers.Contains(userId))
                continue;

            listEntry.Subscribers.Remove(userId);
            _contentChanged = true;
        }
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
