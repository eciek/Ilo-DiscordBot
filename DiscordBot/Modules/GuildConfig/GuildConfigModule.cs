namespace DiscordBot.Modules.GuildConfig;

public class GuildConfigModule(GuildConfigService guildConfigService) : InteractionModuleBase<SocketInteractionContext>
{
    private GuildConfigService _guildConfigService { get; set; } = guildConfigService;

    
    [SlashCommand("config", "Admin: Konfiguracja funkcji bota")]
    [RequireUserPermission(guildPermission: GuildPermission.Administrator)]
    public async Task ConfigBot()
    {
        var builder = BuildConfig(Context);
        await RespondAsync("", ephemeral: true, components: builder.Build());
    }

    [SlashCommand("klucz-dostępu", "Admin: Wygeneruj zewnętrzny token dostępu")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task GenerateAccessCode()
    {

        await RespondAsync("Uwaga, zaraz wygeneruję token do Discorda! Trzymaj go w sekrecie, by nie wpadł w złe ręce! (>ᴗ•)", ephemeral: true);
        var token = _guildConfigService.CreateAccessToken(Context.Guild);

        await FollowupAsync(token, ephemeral: true);
    }

    private static IReadOnlyCollection<SocketGuildChannel> GetGuildChannels(SocketInteractionContext context)
        => context.Guild.Channels;

    private ComponentBuilder BuildConfig(SocketInteractionContext context)
    {
        var builder = new ComponentBuilder
        {
            ActionRows = []
        };

        foreach (var component in _guildConfigService.Components)
        {
            Console.WriteLine("Adding config for: " + component.Method.Name);
            builder = component.Invoke(builder, context);
        }
        return builder;
    }

}