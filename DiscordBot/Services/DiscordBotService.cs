using DiscordBot.Models;
using Microsoft.Extensions.Hosting;

namespace DiscordBot.Services;

public class DiscordBotService(
        DiscordSocketClient client,
        InteractionService interactions,
        ILogger<DiscordBotService> logger,
        InteractionHandler interactionHandler) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken ct)
    {
        var sconfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json",optional:false,reloadOnChange:true)
            .Build();
        BotConfig botConfig = new()
        { token = sconfig.GetValue<string>("BotConfig:token") ?? throw new Exception("appsettings.json is not specified propely!") };

        if (botConfig.token == null)
            throw new Exception("BotConfig:token is missing!");

        client.Ready += ClientReady;

        client.Log += LogAsync;
        interactions.Log += LogAsync;
        Task task1 = Task.Run(TimerService.Timer, ct);

        return interactionHandler.InitializeAsync()
            .ContinueWith(t => client.LoginAsync(TokenType.Bot, botConfig.token), ct)
            .ContinueWith(t => client.StartAsync(), ct);
    }

    public override Task StopAsync(CancellationToken ct)
    {
        if (ExecuteTask is null)
            return Task.CompletedTask;

        base.StopAsync(ct);
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
}