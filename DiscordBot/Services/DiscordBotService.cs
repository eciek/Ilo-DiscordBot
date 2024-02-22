using DiscordBot.Models;
using DiscordBot.Modules.Config;
using DiscordBot.Modules.Timer;
using Microsoft.Extensions.Hosting;

namespace DiscordBot.Services;

public class DiscordBotService(
        DiscordSocketClient client,
        InteractionService interactions,
        ILogger<DiscordBotService> logger,
        InteractionHandler interactionHandler,
        ConfigBotService configBotService) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken ct)
    {
        var sconfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json",optional:false,reloadOnChange:true)
            .Build();
        BotConfig botConfig = new()
        { token = sconfig.GetValue<string>("BotConfig:token") ?? throw new Exception("appsettings.json is not specified properly!") };

        if (String.IsNullOrEmpty(botConfig.token))
            throw new Exception("BotConfig:token is missing!");

        client.Ready += ClientReady;
        client.SetGameAsync("Highschool of the Burning Onion");
        client.Log += LogAsync;
        client.ButtonExecuted += ButtonHandler;
        client.SelectMenuExecuted += SelectMenuHandler;
        
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

    public async Task ButtonHandler(SocketMessageComponent component)
    {
        switch (component.Data.CustomId)
        {
            case "saveButton":
                await component.RespondAsync("Zapisano!", ephemeral: true);
            break;
        }
    }



    public async Task SelectMenuHandler(SocketMessageComponent component)
    {
        switch (component.Data.CustomId)
        {
            case "configMenu":
                configBotService.SaveConfig(Convert.ToUInt64(component.Data.Values.First()), component.GuildId);
                await component.RespondAsync("Zapisano!", ephemeral: true);
            break;
        }
    }
}