using DiscordBot.Models;
using DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules.AntiSpam
{
    public class AntiSpamService : InteractionModuleBase<SocketInteractionContext>
    {
        public List<long> _blockedUsers;
        TimerService _timerService;
        private const int _jobInterval = 1;

        public AntiSpamService(TimerService timerService)
        {
            _blockedUsers = new List<long>();
            _timerService = timerService;

            TimerJob antiSpamJob = new(nameof(antiSpamJob), _jobInterval, TimerJobTiming.NowAndRepeatOnInterval, ClearUsers);
            _timerService.RegisterJob(antiSpamJob);
        }

        void ClearUsers()
        {
            _blockedUsers = [];
            Console.WriteLine("ClearedBlockedUsers!");
        }
    }
}
