using DiscordBot.Models;
using DiscordBot.Modules.AnimeBirthdays.Models;
using DiscordBot.Modules.GuildConfig;
using DiscordBot.Services;
using Newtonsoft.Json;

namespace DiscordBot.Modules.AnimeBirthdays;

public class BirthdayAnimeService
{
    readonly List<BirthdayAnime> _animeBirthdays;
    private readonly GuildConfigService _guildConfigService;

    private const string _configId = "animeBirthday";

    public BirthdayAnimeService(GuildConfigService configBotService)
    {
        _guildConfigService = configBotService;

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
        => _animeBirthdays.Where(x => x.Date == date).Select(x => x.Characters).FirstOrDefault() ?? [];

    public ulong GetBirthdayChannel(ulong guildId)
        => _guildConfigService
        .GetGuildConfig(guildId)
        .Where(x => x.Key == _configId)
        .Select(x => (ulong)x.Value)
        .First();

    public ulong[] GetUnlockedGuilds()
        => _guildConfigService
        .GetAllGuildsData()
        .Where(x =>
            x.Value.Any(y =>
            y.Key == _configId &&
            (ulong)y.Value > 0))
        .Select(x => x.Key)
        .ToArray();

    private static ComponentBuilder BuildConfig(ComponentBuilder builder, SocketInteractionContext context)
    {
        var menuBuilder = new SelectMenuBuilder()
        .WithPlaceholder("Urodziny Anime - wybierz kanał")
        .WithCustomId(_configId)
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