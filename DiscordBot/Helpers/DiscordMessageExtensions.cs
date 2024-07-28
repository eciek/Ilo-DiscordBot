namespace DiscordBot.Helpers;

public static partial class DiscordMessageExtensions
{
    public static IMessage? GetReferencedMessage(this SocketMessage msg)
    {
        if (msg.Reference is null)
            return null;

        var referenceId = msg.Reference.MessageId;

        if (referenceId.IsSpecified)
        {
            var reference = msg.Channel.GetMessageAsync(referenceId.Value).Result;
            return reference;
        }
        return null;
    }
}
