using Microsoft.Extensions.Configuration;
using Discord;
using Discord.WebSocket;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace DiscordBot;
class Program
{
    private static DiscordSocketClient _client;
    private static Commands _commands = new Commands();
    private static Config _config = new Config();

    public static async Task Main(string[] args)
    {

        // setup bot
        var configBot = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };

        _client = new DiscordSocketClient(configBot);

        // adding func to bot
        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.MessageReceived += MessageReceivedAsync;
        _client.InteractionCreated += InteractionCreatedAsync;



        // token
        await _client.LoginAsync(TokenType.Bot, _config.token);
        await _client.StartAsync();
        await Timer();
        await Task.Delay(Timeout.Infinite);
        
        

    }

    private static Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    private static Task ReadyAsync()
    {
        Console.WriteLine($"{_client.CurrentUser} is connected!");

        return Task.CompletedTask;
    }

    private static async Task MessageReceivedAsync(SocketMessage message)
    {
        // bot not responding to itself
        if (message.Author.Id == _client.CurrentUser.Id)
            return;

        Console.WriteLine($"{message.Author.Id}: {message.Content}");

        _commands.CheckCommand(message);
    }

    // no idea how this is working yet
    private static async Task InteractionCreatedAsync(SocketInteraction interaction)
    {
        if (interaction is SocketMessageComponent component)
        {
            if (component.Data.CustomId == "unique-id")
                await interaction.RespondAsync("Thank u for clicking button");

            else
                Console.WriteLine("An ID has been received that has no handler!");
        }
    }

    public static async Task Timer()
    {
        DateTime now = DateTime.Now;
        DateTime tomorrow = now.AddDays(1).Date;
        TimeSpan delay = tomorrow - now;

        Console.WriteLine($"Timer start {now}, {tomorrow}, {delay}");

        await Task.Delay(delay);
        Birthday bd = new Birthday();
        bd.SendBirthday(tomorrow);
        await ClearUsers();
    }

    public static async Task ClearUsers()
    {
        Console.WriteLine("Cleared");
        string jsonstring = "[{\"id\":\"null\",\"card\":\"null\",\"usedTime\":0,\"botMessagesId\":[{\"guildId\":1,\"messageId\":1}]}]";
        System.IO.File.WriteAllText("JsonFiles/tarotcardsused.json", jsonstring);

        await Timer();
    }

}
