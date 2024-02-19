using DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class ConfigBotModule : InteractionModuleBase<SocketInteractionContext>
    {
        public ConfigBotService ConfigBotService { get; set; }

        [SlashCommand("config", "Konfiguracja funkcji bota")]
        public async Task ConfigBot()
        {

            var builder = ConfigBotService.Build(Context);
            await RespondAsync("", ephemeral: true, components: builder.Build());

            
        }
    }
}
