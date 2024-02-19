using DiscordBot;
using Microsoft.Extensions.Hosting;

namespace DiscordBot.Services;

public class DiscordBotService(DiscordSocketClient client, InteractionService interactions, IConfiguration config, ILogger<DiscordBotService> logger,
    InteractionHandler interactionHandler) : BackgroundService
{
    Config _config = new Config();
    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        client.Ready += ClientReady;

        client.Log += LogAsync;
        client.ButtonExecuted += ButtonHandler;
        interactions.Log += LogAsync;
        Task task1 = Task.Run(async () => await TimerService.Timer());

        return interactionHandler.InitializeAsync()
            .ContinueWith(t => client.LoginAsync(TokenType.Bot, _config.token), cancellationToken)
            .ContinueWith(t => client.StartAsync(), cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        if (ExecuteTask is null)
            return Task.CompletedTask;

        base.StopAsync(cancellationToken);
        return client.StopAsync();
    }

    private async Task ClientReady()
    {
        logger.LogInformation("Logged as {User}", client.CurrentUser);
        await interactions.RegisterCommandsGloballyAsync(deleteMissing: true);
    }


    public Task LogAsync(LogMessage msg)
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

        logger.Log(severity, msg.Exception, msg.Message);
        return Task.CompletedTask;
    }

    public async Task ButtonHandler(SocketMessageComponent component)
    {
        switch (component.Data.CustomId)
        {
            case "saveButton":
                await component.RespondAsync("Zapisano!", ephemeral: true);
            break;
        }
    }
}