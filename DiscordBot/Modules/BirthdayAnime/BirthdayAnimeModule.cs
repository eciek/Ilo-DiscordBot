using DiscordBot.Models;
using DiscordBot.Modules.AnimeBirthdays;
using DiscordBot.Modules.AnimeBirthdays.Models;
using DiscordBot.Modules.GuildLogging;
using DiscordBot.Services;
using System.Security.Cryptography;
using System.Text;

namespace DiscordBot.Modules.GuildConfig
{
    public class BirthdayAnimeModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly BirthdayAnimeService _birthdayAnimeService;
        private readonly TimerService _timerService;        

        public BirthdayAnimeModule(
            BirthdayAnimeService birthdayAnimeService, 
            TimerService timerService)
        {
            _birthdayAnimeService = birthdayAnimeService;
            _timerService = timerService;

            TimerJob birthdaySendMessageJob = new(nameof(birthdaySendMessageJob), 0, TimerJobTiming.TriggerDailyAtSetMinute, BirthdayUpdate);
            _timerService.RegisterJob(birthdaySendMessageJob);
        }

        private void BirthdayUpdate()
        {
            try
            {
                _birthdayAnimeService.SendBirthdayMessage();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


    }
}