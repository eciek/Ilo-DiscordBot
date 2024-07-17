namespace DiscordBot.Modules.GuildLogging;

public class IloException : Exception
{
    public IloException() : base()
    {
        DiscordActionData = new();
    }

    public IloException(string message) : base(message)
    {
        DiscordActionData = new();
    }

    public DiscordActionData DiscordActionData { get; set; }

    public Embed ToEmbed()
    {
        EmbedBuilder embed = new()
        {
            Color = Color.Red,
            Title = DiscordActionData.CommandName,
            Description = Message
        };


        return embed.Build();
    }
}

public class DiscordActionData
{
    public ulong? UserId { get; private set; }
    public string? Username { get; private set; }
    public ulong? ChannelId { get; private set; }
    public string? CommandName { get; private set; }
    public string? ModuleInfo { get; private set; }
}