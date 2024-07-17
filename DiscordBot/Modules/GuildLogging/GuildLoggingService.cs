using DiscordBot.Modules.GuildConfig;
using DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules.GuildLogging;
public class GuildLoggingService
{
    private readonly GuildConfigService _guildConfigService;
    private readonly DiscordChatService _chatService;

    private const string _logChannelConfigId = "loggingChannel";

    public GuildLoggingService(GuildConfigService configBotService, DiscordChatService chatService)
    {
        _guildConfigService = configBotService;
        _chatService = chatService;

        _guildConfigService.Components.Add(BuildConfig);
    }

    public void GuildLog(ulong guildId, string msg, Embed? embed = null)
    {
        var logchannelId = ulong.Parse(
            (string?) _guildConfigService
            .GetGuildConfigValue(guildId, _logChannelConfigId) 
            ?? "0");
        if (logchannelId > 0)
            _ = _chatService.SendMessage(logchannelId, msg, embed);
    }

    private static ComponentBuilder BuildConfig(ComponentBuilder builder, SocketInteractionContext context)
    {
        var menuBuilder = new SelectMenuBuilder()
        .WithPlaceholder("Kanał na logi - wybierz kanał")
        .WithCustomId(_logChannelConfigId)
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
