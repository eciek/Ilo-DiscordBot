using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class ConfigBotService : InteractionModuleBase<SocketInteractionContext>
    {
        public IReadOnlyCollection<SocketGuildChannel> GetAllChannels(SocketInteractionContext context)
        {
            IReadOnlyCollection<SocketGuildChannel> channels = new List<SocketGuildChannel>();
            channels = context.Guild.Channels;
            
            return channels;
        }

        public ComponentBuilder Build(SocketInteractionContext context)
        {
            var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Wybierz opcje")
            .WithCustomId("configMenu")
            .WithMinValues(1)
            .WithMaxValues(1);

            IReadOnlyCollection<SocketGuildChannel> guildChannels = GetAllChannels(context);

            foreach (SocketGuildChannel item in guildChannels)
            {
                if (item.GetChannelType() is ChannelType.Text)
                {
                    menuBuilder.AddOption($"{item.Name}", $"{item.Id}", "x");
                }

            }


            var builder = new ComponentBuilder().WithSelectMenu(menuBuilder);
            return builder;
        }
    }
}
