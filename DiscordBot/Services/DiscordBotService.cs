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

    private ulong _iloUserId;

    protected override Task ExecuteAsync(CancellationToken ct)
    {
        var sconfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        var token = sconfig.GetValue<string>("BotConfig:token") ?? throw new Exception("appsettings.json is not specified properly!");
        if (String.IsNullOrEmpty(token))
            throw new Exception("BotConfig:token is missing!");

        client.Ready += ClientReady;
        client.SetGameAsync("Highschool of the Burning Onion");
        client.Log += LogAsync;
        client.ButtonExecuted += ConfigHandler;
        client.SelectMenuExecuted += ConfigHandler;
        client.LeftGuild += OnGuildLeave;
        client.MessageReceived += MessageReceived;

        interactions.Log += LogAsync;

        return interactionHandler.InitializeAsync()
            .ContinueWith(t => client.LoginAsync(TokenType.Bot, token), ct)
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
        _iloUserId = client.CurrentUser.Id;

        logger.LogInformation("Logged as {User}", client.CurrentUser);
        await interactions.RegisterCommandsGloballyAsync(deleteMissing: true);
        //await interactions.RegisterCommandsToGuildAsync(1209180343714971739, true);

        // clear config from discarded guilds
        foreach (var configId in configBotService.GetAllGuilds())
        {
            if (!client.Guilds.Any(x => x.Id == configId))
                configBotService.RemoveGuildConfig(configId);
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

        logger.Log(severity, "DiscordChatService Exception {Exception}\nMessage: {message}", msg.Exception, msg.Message);
        return Task.CompletedTask;
    }

    private Task OnGuildLeave(SocketGuild guild)
    {
        configBotService.RemoveGuildConfig(guild.Id);
        return Task.CompletedTask;
    }

    private async Task ConfigHandler(SocketMessageComponent component)
    {
        var guildId = component.GuildId is null
            ? 0
            : component.GuildId.Value;

        var value = component.Data.Values.FirstOrDefault() ?? "0";

        var record = new GuildConfigRecord(component.Data.CustomId, value);
        configBotService.SaveConfig(guildId, record);
        await component.RespondAsync("Zapisano!", ephemeral: true);
    }

    private async Task MessageReceived(SocketMessage message)
    {
        if(message.Author.IsBot)
        {
            return;
        }

        if(message.MentionedUsers.Any(x=> x.Id == _iloUserId)) 
        {

        }
    }
}