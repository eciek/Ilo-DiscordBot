using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Modules.GuildConfig;

namespace DiscordBot.Modules;

public abstract class ConfigInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    protected readonly GuildConfigService _guildConfigService;

    protected ConfigInteractionModule(GuildConfigService guildConfigService)
    {
        _guildConfigService = guildConfigService;
    }

    protected ComponentBuilder BuildConfig(SocketInteractionContext context)
    {
        var builder = new ComponentBuilder();
        // Ensure ActionRows list is initialized before components add rows
        if (builder.ActionRows == null)
            builder.ActionRows = new List<ActionRowBuilder>();

        foreach (var component in _guildConfigService.Components)
        {
            try
            {
                builder = component.Invoke(builder, context);
            }
            catch (Exception)
            {
                // Swallow component build errors to avoid breaking the whole config UI
                continue;
            }
        }

        return builder;
    }
}
