using DiscordBot.Models;
using DiscordBot.Services;
using DiscordBot.Modules.Tarot;
using DiscordBot.Modules.BirthdayAnime;

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