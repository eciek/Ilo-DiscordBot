namespace DiscordBot.Modules.GuildConfig
{
    public class GuildConfigModule(GuildConfigService guildConfigService) : InteractionModuleBase<SocketInteractionContext>
    {
        public GuildConfigService GuildConfigService { get; set; } = guildConfigService;

        //[RequireUserPermission(GuildPermission.Administrator)]
        [RequireOwner()]
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
            var builder = new ComponentBuilder
            {
                ActionRows = []
            };

            foreach (var component in GuildConfigService.Components)
            {
                Console.WriteLine("Adding config for: " + component.Method.Name);
                builder = component.Invoke(builder, context);
            }
            return builder;
        } 

    }
}