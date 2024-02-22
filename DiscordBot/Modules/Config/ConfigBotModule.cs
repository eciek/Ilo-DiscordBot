namespace DiscordBot.Modules.Config
{
    public class ConfigBotModule : InteractionModuleBase<SocketInteractionContext>
    {
        public ConfigBotService ConfigBotService { get; set; }

        [RequireUserPermission(GuildPermission.Administrator)]
        [SlashCommand("config", "Konfiguracja funkcji bota")]
        public async Task ConfigBot()
        {
            var builder = ConfigBotService.Build(Context);
            await RespondAsync("", ephemeral: true, components: builder.Build());
        }

        // Ta funkcja jest tutaj tylko po to żeby nie wywalało błędu xD
        [ComponentInteraction("configMenu")]
        public async Task ConfigMenu()
        {
        }
    }
}