namespace DiscordBot.Modules.Tarot.Models;

public class TarotDraw
{
    public ulong UserId { get; set; }
    public BotMessage Message { get; set; } = new BotMessage();
}