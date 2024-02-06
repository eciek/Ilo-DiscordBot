using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models
{
    public class TarotCardsUsed
    {
        public string id { get;set; }
        public string card { get;set; }
        public int usedTime { get;set; }
        public ulong botMessageId { get;set; }
    }
}
