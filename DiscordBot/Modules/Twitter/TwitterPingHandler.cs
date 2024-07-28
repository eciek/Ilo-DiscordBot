using DiscordBot.Helpers;
using DiscordBot.Models;
using DiscordBot.Modules.Twitter;
using DiscordBot.Services;

namespace DisordBot.Modules.Twitter;

public class TwitterPingHandler : IPingHandler
{
    private readonly DiscordChatService _chatService;
    public string Name { get; } = "FxUrlFix";
    public uint Priority => 10;

    public PingHandlerConfig Config { get; init; }

    public TwitterPingHandler(DiscordChatService chatService)
    {
        _chatService = chatService;
        Config = new PingHandlerConfig()
        {
            FinalHandler = true,
            ReplyRequirements = HandlerReplyRequirements.AllowReplies,
        };
    }

    public bool CheckCustomCondition(SocketMessage message)
    {
        // Check if in message or in reply there was an x/twitter link to convert.
        var text = GetCleanTextFromMessageAndReference(message);

        return text
            .Replace(@"https://twitter.com/", @"https://x.com/")
            .Contains(@"https://x.com/");
    }

    public Task HandlePing(SocketMessage message)
    {
        var text = GetCleanTextFromMessageAndReference(message);

        var fixedUrl = TwitterService.FixupUrl(text);

        if (String.IsNullOrEmpty(fixedUrl))
            return Task.CompletedTask;

        return _chatService.SendMessage(message.Channel.Id, fixedUrl, messageReference: message.Reference ?? new MessageReference(message.Id, message.Channel.Id));

    }

    private static string GetCleanTextFromMessageAndReference(SocketMessage message)
    {
        var text = message.CleanContent;

        var refMessage = message.GetReferencedMessage();
        if (refMessage != null)
        {
            text += "\n" + refMessage.CleanContent;
        }
        return text;
    }

}
