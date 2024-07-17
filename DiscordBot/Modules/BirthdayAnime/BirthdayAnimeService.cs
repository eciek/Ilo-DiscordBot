using DiscordBot.Modules.AnimeBirthdays.Models;
using DiscordBot.Modules.GuildConfig;
using DiscordBot.Modules.GuildLogging;
using DiscordBot.Services;
using Newtonsoft.Json;
using System.Security.Cryptography;
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

        _guildConfigService.Components.Add(BuildConfig);

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

    public List<BirthdayAnimeCharacter> GetAnimeCharacters(string date)
        => _animeBirthdays
                .Where(x => x.Date == date)
                .Select(x => x.Characters)
                .FirstOrDefault()
                ?? [];

    public void SendBirthdayMessage()
    {
        var characters = GetAnimeCharacters(DateTime.Now.ToString("dd.MM"));
        var msgBuilder = new StringBuilder();
        msgBuilder.AppendLine("Dzisiaj urodzili się:");

        bool hasError = false;

        List<Embed> embeds = [];

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
            List<string> charImages = [];

            msgBuilder.AppendLine(character.ToString());            
            try
            {
                charImages.AddRange(_booruService.GetBooruImageAsync(character.ToNameSurnameBooruSlug(), 3).Result.ToList());
                if (!String.IsNullOrEmpty(character.Surname))
                    charImages.AddRange(_booruService.GetBooruImageAsync(character.ToSurnameNameBooruSlug(), 3).Result.ToList());
            }
            catch (Exception e)
            {
                errMsgBuilder.AppendLine($"GetImages:\n{ character.ToNameSurnameBooruSlug()}\n"
                + e.Message);
                hasError = true;
                continue;
            }

            if (charImages.Count == 0)
            {
                errMsgBuilder.AppendLine($"No images found for {character.ToNameSurnameBooruSlug()}");
                
                continue;
            }

            var selectedItem = charImages[RandomNumberGenerator.GetInt32(charImages.Count - 1)];

            if (selectedItem.EndsWith(".mp4"))
            {
                var emb = new EmbedBuilder().WithUrl($"attachment://{selectedItem}").Build();
                embeds.Add(emb);
            }
            else
            {
                var emb = new EmbedBuilder().WithImageUrl($"attachment://{selectedItem}").Build();
                embeds.Add(emb);
            }
        }

        foreach (var guildId in GetUnlockedGuilds())
        {
            bool hasGuildError = hasError;
            try
            {
                var channelId = GetBirthdayChannel(guildId);
                _ = _chatService.SendMessage(channelId, msgBuilder.ToString(), embeds: [.. embeds]);
            }
            catch (Exception e)
            {
                errMsgBuilder.AppendLine($"SendMessage:\n{msgBuilder.ToString()}\n"
                + e.Message);
                hasGuildError = true;
            }
            if (hasGuildError)
                _loggingService.GuildLog(guildId, errMsgBuilder.ToString());
        }
    }

    private ulong GetBirthdayChannel(ulong guildId)
        => (ulong?)_guildConfigService
        .GetGuildConfigValue(guildId, _birthdayChannelConfigId)
        ?? 0;

    private ulong[] GetUnlockedGuilds()
        => _guildConfigService
        .GetAllGuildsData()
        .Where(x =>
            x.Value.Any(y =>
            y.Key == _birthdayChannelConfigId &&
            y.Value is not null &&
            (ulong)y.Value > 0))
        .Select(x => x.Key)
        .ToArray();

    private static ComponentBuilder BuildConfig(ComponentBuilder builder, SocketInteractionContext context)
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
        return builder.WithSelectMenu(menuBuilder);
    }
}