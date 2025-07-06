using DiscordBot.Helpers;
using DiscordBot.Models;
using DiscordBot.Modules.Twitter;
using DiscordBot.Services;

namespace DisordBot.Modules.Twitter;

public class TwitterMessageHandler : IMessageHandler
{
    private readonly DiscordChatService _chatService;
    public string Name { get; } = "FxUrlFix";
    public uint Priority => 10;

    public PingHandlerConfig Config { get; init; }

    public TwitterMessageHandler(DiscordChatService chatService)
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
        var text = message.CleanContent;

        var refMessage = message.GetReferencedMessage();
        if (refMessage != null)
        {
            text += "\n" + refMessage.CleanContent;
        }

        return text
            .Replace(@"https://twitter.com/", @"https://x.com/")
            .Contains(@"https://x.com/");
    }

    public async Task HandlePing(SocketMessage message)
    {
        var text = message.CleanContent;
        ulong author = message.Author.Id;

        var refMessage = message.GetReferencedMessage();
        if (refMessage != null)
        {
            text += "\n" + refMessage.CleanContent;
            author = refMessage.Author.Id;
        }

        var fixedUrl = TwitterService.FixupUrl(text);

        if (String.IsNullOrEmpty(fixedUrl))
            return;

        string msg = $"-# <@{author}>: \n {fixedUrl}";
        _ = _chatService.SendMessage(message.Channel.Id, msg);
        await _chatService.DeleteMessage(message.Channel.Id, message.Id);
        if (message.Reference is not null)
            await _chatService.DeleteMessage(message.Channel.Id, message.Reference);
    }
}
