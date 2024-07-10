namespace DiscordBot.Modules.GuildConfig
{
    public class GuildConfigModule : InteractionModuleBase<SocketInteractionContext>
    {
        public GuildConfigService GuildConfigService { get; set; }

        public GuildConfigModule(GuildConfigService guildConfigService)
        {
            GuildConfigService = guildConfigService;
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [SlashCommand("config", "Konfiguracja funkcji bota")]
        public async Task ConfigBot()
        {
            var builder = BuildConfig(Context);
            await RespondAsync("", ephemeral: true, components: builder.Build());
        }
        private static IReadOnlyCollection<SocketGuildChannel> GetGuildChannels(SocketInteractionContext context)
            => context.Guild.Channels;

        private ComponentBuilder BuildConfig(SocketInteractionContext context)
        {
            var builder = new ComponentBuilder();
            foreach (var component in GuildConfigService.Components)
            {
                builder = component.Invoke(builder, context);
            }
            return builder;
        }
    }
}