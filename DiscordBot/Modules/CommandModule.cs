using DiscordBot.Models;
using DiscordBot.Services;

namespace DiscordBot.Modules;

public class CommandModule : InteractionModuleBase<SocketInteractionContext>
{
    public TarotService TarotService { get; set; }
    private readonly ILogger<CommandModule> _logger;

    public CommandModule(ILogger<CommandModule> logger)
    {
        _logger = logger;
    }
}