namespace DiscordBot.Services;

public class DiscordChatService
{
    private readonly DiscordSocketClient _socketClient;

    public DiscordChatService(DiscordSocketClient socketClient)
    {
        _socketClient = socketClient;
    }

    public async Task SendMessage(ulong channelId, string message, Embed? embed = null, Embed[]? embeds = null)
    {
        if (_socketClient.GetChannel(channelId) is not SocketTextChannel channel)
        {
            throw new ArgumentException($"Could not find TestChannel with given ID:[{channelId}]");
        }
        await channel.SendMessageAsync(text: message, embed: embed, embeds: embeds);
    }
}
