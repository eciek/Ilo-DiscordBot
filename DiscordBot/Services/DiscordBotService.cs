using DiscordBot.Modules.GuildConfig;
using DiscordBot.Modules.GuildConfig.Models;
using Microsoft.Extensions.Hosting;

namespace DiscordBot.Services;

public class DiscordBotService(
        DiscordSocketClient client,
        InteractionService interactions,
        ILogger<DiscordBotService> logger,
        InteractionHandler interactionHandler,
        GuildConfigService configBotService) : BackgroundService
{
    private readonly DiscordSocketClient _client = client;
    private readonly InteractionService _interactions = interactions;
    private readonly ILogger<DiscordBotService> _logger = logger;
    private readonly InteractionHandler _interactionHandler = interactionHandler;
    private readonly GuildConfigService _configBotService = configBotService;

    private ulong _iloUserId;

    protected override Task ExecuteAsync(CancellationToken ct)
    {
        var sconfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        var token = sconfig.GetValue<string>("BotConfig:token") ?? throw new Exception("appsettings.json is not specified properly!");
        if (String.IsNullOrEmpty(token))
            throw new Exception("BotConfig:token is missing!");

        _client.Ready += ClientReady;
        _client.SetGameAsync("Highschool of the Burning Onion");
        _client.Log += LogAsync;
        _client.ButtonExecuted += ConfigHandler;
        _client.SelectMenuExecuted += ConfigHandler;
        _client.LeftGuild += OnGuildLeave;
        _client.MessageReceived += MessageReceived;

        _interactions.Log += LogAsync;

        return _interactionHandler.InitializeAsync()
            .ContinueWith(t => _client.LoginAsync(TokenType.Bot, token), ct)
            .ContinueWith(t => _client.StartAsync(), ct);
    }

    public override Task StopAsync(CancellationToken ct)
    {
        if (ExecuteTask is null)
            return Task.CompletedTask;

        base.StopAsync(ct);
        return _client.StopAsync();
    }

    private async Task ClientReady()
    {
        _iloUserId = _client.CurrentUser.Id;

        _logger.LogInformation("Logged as {User}", _client.CurrentUser);
        await _interactions.RegisterCommandsGloballyAsync(deleteMissing: true);
        //await interactions.RegisterCommandsToGuildAsync(1209180343714971739, true);

        // clear config from discarded guilds
        foreach (var configId in _configBotService.GetAllGuilds())
        {
            if (!_client.Guilds.Any(x => x.Id == configId))
                _configBotService.RemoveGuildConfig(configId);
        }
    }


    private Task LogAsync(LogMessage msg)
    {
        var severity = msg.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Trace,
            LogSeverity.Debug => LogLevel.Debug,
            _ => LogLevel.Information
        };

        _logger.Log(severity, "DiscordChatService Exception {Exception}\nMessage: {message}", msg.Exception, msg.Message);
        return Task.CompletedTask;
    }

    private Task OnGuildLeave(SocketGuild guild)
    {
        _configBotService.RemoveGuildConfig(guild.Id);
        return Task.CompletedTask;
    }

    private async Task ConfigHandler(SocketMessageComponent component)
    {
        var guildId = component.GuildId is null
            ? 0
            : component.GuildId.Value;

        var value = component.Data.Values.FirstOrDefault() ?? "0";

        var record = new GuildConfigRecord(component.Data.CustomId, value);
        _configBotService.SaveConfig(guildId, record);
        await component.RespondAsync("Zapisano!", ephemeral: true);
    }

    private Task MessageReceived(SocketMessage message)
    {
        if (message.Author.IsBot)
        {
            return Task.CompletedTask;
        }

        if (message.MentionedUsers.Any(x => x.Id == _iloUserId))
        {
            foreach (var pingInteraction in _interactionHandler.PingHandlers)
            {
                if (pingInteraction.Config.CheckConditions(message) &&
                    pingInteraction.CheckCustomCondition(message))
                {
                    _= pingInteraction.HandlePing(message);
                    if (pingInteraction.Config.FinalHandler)
                        break;
                }
            }
        }

        return Task.CompletedTask;
    }
}