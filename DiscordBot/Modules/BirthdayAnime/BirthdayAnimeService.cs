using DiscordBot.Modules.AnimeBirthdays.Models;
using DiscordBot.Modules.GuildConfig;
using DiscordBot.Modules.GuildLogging;
using DiscordBot.Services;
using DiscordBot.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace DiscordBot.Modules.AnimeBirthdays;

public class BirthdayAnimeService
{
    readonly List<BirthdayAnime> _animeBirthdays;
    private readonly GuildConfigService _guildConfigService;
    private readonly BooruService _booruService;
    private readonly DiscordChatService _chatService;
    private readonly GuildLoggingService _loggingService;

    private const string _birthdayChannelConfigId = "animeBirthday";

    public BirthdayAnimeService(
        GuildConfigService configBotService,
        BooruService booruService,
        DiscordChatService chatService,
        GuildLoggingService loggingService)
    {
        _guildConfigService = configBotService;
        _booruService = booruService;
        _chatService = chatService;
        _loggingService = loggingService;

        _guildConfigService.AddConfigComponent(BirthdayAnimeBuilder);

        using var s = new StreamReader("Modules/BirthdayAnime/JsonFiles/birthdayanime.json");
        var jsonString = s.ReadToEnd();
        try
        {
            _animeBirthdays = JsonConvert.DeserializeObject<List<BirthdayAnime>>(jsonString) ?? throw new Exception();
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to read birthdayanime.json! \n" + ex.Message);
        }
        if (_animeBirthdays == null)
            throw new Exception("Failed to read birthdayanime.json!");
    }

    public BirthdayAnimeService(GuildConfigService configBotService,
        BooruService booruService,
        DiscordChatService chatService,
        GuildLoggingService loggingService,
        TimerService timerService,
        ILogger<BirthdayAnimeService> logger) : this(configBotService, booruService, chatService, loggingService)
    {
        try
        {
            var birthdaySendMessageJob = new TimerJob(nameof(SendBirthdayMessage), 0, TimerJobTiming.TriggerDailyAtSetMinute, () => SendBirthdayMessage().GetAwaiter().GetResult());
            timerService.RegisterJob(birthdaySendMessageJob);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to register BirthdayAnime timer job");
        }
    }

    public List<BirthdayAnimeCharacter> GetAnimeCharacters(string date)
        => _animeBirthdays
                .Where(x => x.Date == date)
                .Select(x => x.Characters)
                .FirstOrDefault()
                ?? new List<BirthdayAnimeCharacter>();

    public async Task SendBirthdayMessage()
    {
        var characters = GetAnimeCharacters(DateTime.Now.ToString("dd.MM"));
        var msgBuilder = new StringBuilder();
        msgBuilder.AppendLine("Dzisiaj urodzili się:");

        bool hasError = false;

        List<FileAttachment> attachments = new List<FileAttachment>();

        var errMsgBuilder = new StringBuilder().AppendLine("BirthdayAnimeService:");

        if (characters.Count == 0)
        {
            msgBuilder.Clear();
            msgBuilder.AppendLine($"Zapomniałam kto się dzisiaj urodził :(");
            errMsgBuilder.AppendLine("No characters found for " + DateTime.Today.ToString("yyyy-MM-dd"));
            hasError = true;
        }

        foreach (var character in characters)
        {
            List<string> charImages = new List<string>();

            msgBuilder.AppendLine(character.ToString());
            try
            {
                var imgs1 = await _booruService.GetBooruImageAsync(character.ToNameSurnameBooruSlug(), 3);
                if (imgs1 != null)
                    charImages.AddRange(imgs1.ToList());
                if (!String.IsNullOrEmpty(character.Surname))
                {
                    var imgs2 = await _booruService.GetBooruImageAsync(character.ToSurnameNameBooruSlug(), 3);
                    if (imgs2 != null)
                        charImages.AddRange(imgs2.ToList());
                }
                if (String.IsNullOrEmpty(character.Surname))
                {
                    var imgs3 = await _booruService.GetBooruImageAsync(character.ToNameSeriesBooruSlug(), 3);
                    if (imgs3 != null)
                        charImages.AddRange(imgs3.ToList());
                }
            }
            catch (Exception e)
            {
                errMsgBuilder.AppendLine($"GetImages:\n{character.ToNameSurnameBooruSlug()}\n"
                + e.Message);
                hasError = true;
                continue;
            }

            if (charImages.Count == 0)
            {
                errMsgBuilder.AppendLine($"No images found for {character.ToNameSurnameBooruSlug()}");

                continue;
            }
            Random rnd = new();
            var selectedItem = charImages[rnd.Next(charImages.Count)];

            attachments.Add(new FileAttachment(selectedItem));
        }

        foreach (var guildId in GetUnlockedGuilds())
        {
            bool hasGuildError = hasError;
            var guildErrMsgBuilder = errMsgBuilder;
            try
            {
                var channelId = GetBirthdayChannel(guildId);

                await _chatService.SendFiles(channelId, msgBuilder.ToString(), attachments.ToArray());
            }
            catch (Exception e)
            {
                guildErrMsgBuilder.AppendLine($"SendMessage:\n{msgBuilder}\n"
                + e.Message);
                hasGuildError = true;
            }
            if (hasGuildError)
                _loggingService.GuildLog(guildId, guildErrMsgBuilder.ToString());
        }

        foreach(var attachment in attachments)
        {
            attachment.Dispose();
        }
    }

    private ulong GetBirthdayChannel(ulong guildId)
        => _guildConfigService.GetGuildConfigValueAsUlong(guildId, _birthdayChannelConfigId);

    private ulong[] GetUnlockedGuilds()
        => _guildConfigService
        .GetAllGuildsData()
        .Where(x =>
            x.Value.Any(y =>
            y.Key == _birthdayChannelConfigId &&
            !String.IsNullOrEmpty((string)y.Value)
            ))
        .Select(x => x.Key)
        .ToArray();

    private static ComponentBuilder BirthdayAnimeBuilder(ComponentBuilder builder, SocketInteractionContext context)
    {
        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Urodziny Anime - wybierz kanał")
            .WithCustomId(_birthdayChannelConfigId)
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
        actionRow.Components ??= new List<IMessageComponent>();
        actionRow.Components.Add(menuBuilder.Build());
        builder.ActionRows.Add(actionRow);

        return builder;
    }
}