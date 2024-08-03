namespace DiscordBot.Modules.Reminder;

public class Reminder
{
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }
    public DateTime TriggerDate { get; set; }

    public string Message { get; set; } = string.Empty;
    public List<string> Attachments { get; set; } = [];
}
