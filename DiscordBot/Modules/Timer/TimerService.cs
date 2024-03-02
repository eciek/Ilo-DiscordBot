using DiscordBot.Modules.BirthdayAnime;
using DiscordBot.Modules.BirthdayAnime.Models;
using DiscordBot.Modules.Config;
using DiscordBot.Modules.Config.Models;
using DiscordBot.Services;

namespace DiscordBot.Modules.Timer
{
    public class TimerService(
        BirthdayAnimeService birthdayAnimeService) : InteractionModuleBase<SocketInteractionContext>
    {
        public async Task Timer()
        {
            DateTime now = DateTime.Now;
            DateTime tomorrow = now.AddDays(1).Date;
            
            TimeSpan delay = tomorrow - now;
            Console.WriteLine(delay);
            Console.WriteLine($"Timer start {now}, {tomorrow}, {delay}");
            BirthdayAnimeModel? characters = birthdayAnimeService.CheckBirthday(tomorrow.ToString("dd.MM"));
            
            await Task.Delay(delay);
            await birthdayAnimeService.SendMessage(characters);
            await ClearUsers();
        }

        public async Task ClearUsers()
        {
            Console.WriteLine("Cleared");
            string jsonstring = "[{\"id\":\"null\",\"card\":\"null\",\"usedTime\":0,\"botMessagesId\":[{\"guildId\":1,\"messageId\":1,\"channelId\":1}]}]";
            File.WriteAllText("Modules/Tarot/JsonFiles/tarotcardsused.json", jsonstring);
            Thread.Sleep(60000);
            await Timer();
        }
    }
}
