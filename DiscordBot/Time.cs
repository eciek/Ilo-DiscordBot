using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    public class Time
    {
        public async Task Timer()
        {
            DateTime now = DateTime.Now;
            DateTime tomorrow = now.AddDays(1).Date;
            TimeSpan delay = tomorrow - now;

            Console.WriteLine("Timer start");

            await Task.Delay(delay);
            await ClearUsers();
        }

        public async Task ClearUsers()
        {
            Console.WriteLine("Cleared");
            string jsonstring = "[{\"id\":\"null\",\"card\":\"null\",\"usedTime\":0,\"botMessagesId\":[{\"guildId\":1,\"messageId\":1}]}]";
            System.IO.File.WriteAllText("C:\\Users\\Paweł\\Desktop\\Hepii\\DiscordBot\\DiscordBot\\JsonFiles\\tarotcardsused.json", jsonstring);

            await Timer();
        }
    }
}
