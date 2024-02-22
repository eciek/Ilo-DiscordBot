namespace DiscordBot.Modules.Timer
{
    public class TimerService
    {
        public static async Task Timer()
        {
            DateTime now = DateTime.Now;
            DateTime tomorrow = now.AddDays(1).Date;
            TimeSpan delay = tomorrow - now;
            Console.WriteLine($"Timer start {now}, {tomorrow}, {delay}");
            await Task.Delay(delay);
            Birthday bd = new Birthday();
            bd.SendBirthday(tomorrow);
            await ClearUsers();
        }

        public static async Task ClearUsers()
        {
            Console.WriteLine("Cleared");
            string jsonstring = "[{\"id\":\"null\",\"card\":\"null\",\"usedTime\":0,\"botMessagesId\":[{\"guildId\":1,\"messageId\":1,\"channelId\":1}]}]";
            File.WriteAllText("Modules/Tarot/JsonFiles/tarotcardsused.json", jsonstring);
            await Timer();
        }
    }
}
