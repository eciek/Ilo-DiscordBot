using System.Security.Cryptography.X509Certificates;

namespace DiscordBot.Models;

public class PingHandlerConfig
{
    /// <summary>
    /// Sets whenever after handling this command, loop should be stopped.
    /// </summary>
    public bool FinalHandler { get; set; } = true;

    /// <summary>
    /// Sets reply req for handler.
    /// </summary>
    public HandlerReplyRequirements ReplyRequirements { get; set; } = HandlerReplyRequirements.AllowReplies;

    public IEntity<ulong>? RoleRequired { get; set; } = null;

    public IEntity<ulong>? GuildId { get; set; } = null;

    public bool CheckConditions(SocketMessage message)
    {
        if (message.Author is not SocketGuildUser user)
            return false;

        bool isReply = message.Reference is not null;

        if (ReplyRequirements == HandlerReplyRequirements.DisallowReplies && isReply)
            return false;
        if (ReplyRequirements == HandlerReplyRequirements.RequireReplies && !isReply) 
            return false;

        if (RoleRequired != null && !user.Roles.Any(x => x.Id == RoleRequired.Id))
            return false;

        if (GuildId != null && user.Guild.Id == GuildId.Id)
            return false;

        return true;
    }
}

public enum HandlerReplyRequirements
{
    AllowReplies = 1,
    RequireReplies = 2,
    DisallowReplies = 3,
}