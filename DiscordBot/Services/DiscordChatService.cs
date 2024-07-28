namespace DiscordBot.Services;

public class DiscordChatService(DiscordSocketClient socketClient)
{
    private readonly DiscordSocketClient _socketClient = socketClient;

    public async Task SendMessage(ulong channelId, string message, Embed? embed = null, Embed[]? embeds = null, MessageReference? messageReference = null)
    {
        if (_socketClient.GetChannel(channelId) is not SocketTextChannel channel)
        {
            throw new ArgumentException($"Could not find text channel with given ID:[{channelId}]");
        }
        await channel.SendMessageAsync(text: message, embed: embed, embeds: embeds, messageReference: messageReference);
    }

    public async Task SendFiles(ulong channelId, string message, FileAttachment[]? attachments, MessageReference? messageReference = null)
    {
        if (_socketClient.GetChannel(channelId) is not SocketTextChannel channel)
        {
            throw new ArgumentException($"Could not find text channel with given ID:[{channelId}]");
        }
        await channel.SendFilesAsync(attachments, text: message, messageReference: messageReference);
    }
}
