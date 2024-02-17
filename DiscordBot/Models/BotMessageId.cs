using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models
{
    public class BotMessageId
    {
        public ulong guildId {  get; set; }
        public ulong messageId { get; set; }
        public ulong channelId { get; set; }
    }
}
