using DiscordBot.Helpers;
using DiscordBot.Modules.AnimeFeed.Models;
using DiscordBot.Modules.GuildConfig;
using DiscordBot.Modules.GuildLogging;
using DiscordBot.Services;

namespace DiscordBot.Modules.AnimeFeed;

public class AnimeListService : ServiceWithJsonData<Anime>
{
    private readonly DiscordChatService _chatService;
    private readonly GuildConfigService _guildConfigService;
    private readonly GuildLoggingService _guildLoggingService;
    private readonly BooruService _booruService;
    private readonly ILogger<AnimeListService> _logger;

    private bool _contentChanged = false;

    public AnimeListService(
            DiscordChatService chatService,
            GuildConfigService guildConfigService,
            GuildLoggingService guildLoggingService,
            BooruService booruService,
            ILogger<AnimeListService> logger)
    {
        _chatService = chatService;
        _guildConfigService = guildConfigService;
        _guildLoggingService = guildLoggingService;
        _booruService = booruService;
        _logger = logger;

        _guildConfigService.AddConfigComponent(AnimeListBuilder);
        _guildConfigService.AddConfigComponent(AnimeArtSensitivityBuilder);
    }

    protected override string ModulePath => "animeFeed";
    private const string animeArtSensitivity = "animeArtSensitivity";

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

                string animeImageRating = _guildConfigService.GetGuildConfigValue(guildData.Key, animeArtSensitivity) ?? "g,s";
                foreach (var guildAnimeList in guildData.Value)
                {
                    var anime = animeList.Where(x => x.Equals(guildAnimeList)).FirstOrDefault();

                    if (anime is null || guildAnimeList.Name is null || guildAnimeList.Episode == anime.Episode)
                        continue;

                    guildAnimeList.Episode = anime.Episode;
                    guildAnimeList.Url = anime.Url;
                    guildAnimeList.Id = anime.Id;

                    if ((guildAnimeList.Subscribers ?? []).Count > 0 && weebChannelId > 0)
                    {
                        List<FileAttachment> attachments = [];
                        string animeNameSlug = guildAnimeList.BooruName.ToBooruSlug();
                        var animeImages = await _booruService.GetBooruImageAsync(animeNameSlug, rating: animeImageRating);

                        if (!animeImages.Any())
                        {
                            _guildLoggingService.GuildLog(guildData.Key, $"Nie znaleziono żadnych artów dla anime {guildAnimeList.Name}. Czy to anime posiada alternatywny tytuł?");
                        }
                        else
                        {
                            Random rnd = new();
                            attachments.AddRange(
                                animeImages
                                .OrderBy(x => rnd.Next(animeImages.Count()))
                                .Take(2)
                                .Select(x => new FileAttachment(x))
                                .ToArray());
                        }

                        await _chatService.SendFiles(weebChannelId, guildAnimeList.GetUpdateMessage(), [.. attachments]);
                    }
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

    public void AddAnimeSubscriber(ulong guildId, ulong userId, Anime anime, string? note = null)
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
        listEntry.Notes ??= [];

        // check if user is already on the list
        if (!listEntry.Subscribers.Contains(userId))
        {
            if (!String.IsNullOrWhiteSpace(note))
            {
                listEntry.Notes.Add(note);
            }

            listEntry.Subscribers.Add(userId);
            _contentChanged = true;
        }
    }

    public void SetAlternateNameForAnime(Anime anime, string alternateName)
    {
        foreach (var guildData in moduleData.Values)
        {
            var guildAnime = guildData.Where(x => x.Name == anime.Name).FirstOrDefault();

            if (guildAnime is not null)
                guildAnime.BooruName = alternateName;
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

    private static ComponentBuilder AnimeListBuilder(ComponentBuilder builder, SocketInteractionContext context)
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

        var actionRow = new ActionRowBuilder();
        actionRow.Components ??= [];
        actionRow.Components.Add(menuBuilder.Build());
        builder.ActionRows.Add(actionRow);

        return builder;
    }

    private static ComponentBuilder AnimeArtSensitivityBuilder(ComponentBuilder builder, SocketInteractionContext context)
    {
        var menuBuilder = new SelectMenuBuilder()
        .WithPlaceholder("anime arty - wybierz opcje NFSW obrazów. ")
        .WithCustomId(nameof(animeArtSensitivity))
        .WithMinValues(1)
        .WithMaxValues(1);

        IReadOnlyCollection<SocketGuildChannel> guildChannels = context.Guild.Channels;

        
        menuBuilder.AddOption("General", "g", "General");
        menuBuilder.AddOption("Sensitive", "s,g", "Wyzywające");
        menuBuilder.AddOption("Questionable", "q,s,g", "softy, lekkie nudle");
        menuBuilder.AddOption("Explicit", "e,q,s,g", "Twarde 18+");

        var actionRow = new ActionRowBuilder();
        actionRow.Components ??= [];
        actionRow.Components.Add(menuBuilder.Build());
        builder.ActionRows.Add(actionRow);

        return builder;
    }


}
