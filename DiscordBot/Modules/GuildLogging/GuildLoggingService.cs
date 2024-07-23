using DiscordBot.Modules.GuildConfig;
using DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules.GuildLogging;
public class GuildLoggingService : InteractionModuleBase<SocketInteractionContext>
{
    private readonly GuildConfigService _guildConfigService;
    private readonly DiscordChatService _chatService;

    private const string _logChannelConfigId = "loggingChannel";

    public GuildLoggingService(GuildConfigService configBotService, DiscordChatService chatService)
    {
        _guildConfigService = configBotService;
        _chatService = chatService;

        
        if (!_guildConfigService.Components.Contains(LogBuilder))
            _guildConfigService.Components.Add(LogBuilder);
    }

    public void GuildLog(ulong guildId, string msg, Embed? embed = null)
    {
        var logchannelId = ulong.Parse(
            (string?)_guildConfigService
            .GetGuildConfigValue(guildId, _logChannelConfigId)
            ?? "0");
        if (logchannelId > 0)
            _ = _chatService.SendMessage(logchannelId, msg, embed);
    }

    private static ComponentBuilder LogBuilder(ComponentBuilder builder, SocketInteractionContext context)
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

        var actionRow = new ActionRowBuilder();
        actionRow.Components.Add(menuBuilder.Build());
        builder.ActionRows.Add(actionRow);

        return builder;
    }
    //
    //[ComponentInteraction("loggingChannel")]
    //public static void AnimeFeed()
    //{
    //
    //}
}
