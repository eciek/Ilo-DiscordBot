namespace DiscordBot.Modules.GuildConfig;

public class GuildConfigModule : ConfigInteractionModule
{
    private readonly GuildConfigService _guildConfigService;

    public GuildConfigModule(GuildConfigService guildConfigService) : base(guildConfigService)
    {
        _guildConfigService = guildConfigService;
    }

    [SlashCommand("config", "Admin: Konfiguracja funkcji bota")]
    [RequireUserPermission(guildPermission: GuildPermission.Administrator)]
    public async Task ConfigBot()
    {
        var builder = BuildConfig(Context);
        await RespondAsync("", ephemeral: true, components: builder.Build());
    }

    private static IReadOnlyCollection<SocketGuildChannel> GetGuildChannels(SocketInteractionContext context)
        => context.Guild.Channels;

}
