using DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
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
