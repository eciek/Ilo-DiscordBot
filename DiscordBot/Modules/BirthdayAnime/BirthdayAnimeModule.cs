using DiscordBot.Models;
using DiscordBot.Modules.AnimeBirthdays;
using DiscordBot.Services;

namespace DiscordBot.Modules.GuildConfig;

public class BirthdayAnimeModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly BirthdayAnimeService _birthdayAnimeService;

    public BirthdayAnimeModule(
        BirthdayAnimeService birthdayAnimeService)
    {
        _birthdayAnimeService = birthdayAnimeService;
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

    [SlashCommand("urodziny", "Eciu: testowe wywolanie daily urodzin")]
    [RequireOwner]
    public async Task TestBirthday()
    {
        await RespondAsync("OK!", ephemeral: true);
        BirthdayUpdate();
    }
}