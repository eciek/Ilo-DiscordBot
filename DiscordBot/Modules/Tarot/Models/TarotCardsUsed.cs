namespace DiscordBot.Modules.Tarot.Models
{
    public class TarotCardsUsed
    {
        public string Id { get; set; }
        public string Card { get; set; }
        public int UsedTime { get; set; }
        public List<BotMessageId> BotMessagesId { get; set; }
    }
}