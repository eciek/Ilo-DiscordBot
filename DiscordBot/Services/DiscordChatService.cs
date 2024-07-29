namespace DiscordBot.Services;

public class DiscordChatService(DiscordSocketClient socketClient)
{
    private readonly DiscordSocketClient _socketClient = socketClient;

    public Task SendMessage(ulong channelId, string message, Embed? embed = null, Embed[]? embeds = null, MessageReference? messageReference = null)
    {
        if (_socketClient.GetChannel(channelId) is not SocketTextChannel channel)
        {
            throw new ArgumentException($"Could not find text channel with given ID:[{channelId}]");
        }
        return channel.SendMessageAsync(text: message, embed: embed, embeds: embeds, messageReference: messageReference);
    }

    public Task SendFiles(ulong channelId, string message, FileAttachment[]? attachments, MessageReference? messageReference = null)
    {
        if (_socketClient.GetChannel(channelId) is not SocketTextChannel channel)
        {
            throw new ArgumentException($"Could not find text channel with given ID:[{channelId}]");
        }
        return channel.SendFilesAsync(attachments, text: message, messageReference: messageReference);
    }

    public Task DeleteMessage(ulong channelId, MessageReference? message)
    {
        if (message is null || message.MessageId.IsSpecified == false)
        {
            throw new ArgumentNullException(nameof(message));
        }
        return DeleteMessage(channelId, message.MessageId.Value);
    }


    public Task DeleteMessage(ulong channelId, IMessage message)
        => DeleteMessage(channelId, message.Id);


    public Task DeleteMessage(ulong channelId, ulong messageId)
    {
        if (_socketClient.GetChannel(channelId) is not SocketTextChannel channel)
        {
            throw new ArgumentException($"Could not find text channel with given ID:[{channelId}]");
        }

        return channel.DeleteMessageAsync(messageId);
    }

}
