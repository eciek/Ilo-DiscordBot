using DiscordBot.Models;
using DiscordBot.Modules.AnimeBirthdays;
using DiscordBot.Services;

namespace DiscordBot.Modules.GuildConfig;

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
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    [SlashCommand("urodziny", "testowe wywolanie daily urodzin")]
    [RequireOwner]
    public async Task TestBirthday()
    {
        await RespondAsync("OK!", ephemeral: true);
        BirthdayUpdate();
    }
}