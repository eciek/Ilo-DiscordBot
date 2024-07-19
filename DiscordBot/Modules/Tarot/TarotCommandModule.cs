using Discord.Net;
using DiscordBot.Models;
using DiscordBot.Modules.Tarot.Models;
using DiscordBot.Services;
using System.Security.Cryptography;

namespace DiscordBot.Modules.Tarot;

public class TarotCommandModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly TarotService _tarotService;
    private readonly TimerService _timerService;
    private readonly ILogger<TarotCommandModule> _logger;


    private const long _schizoID = 792728812730449941;
    private const long _alcoholicID = 1192192605941932093;

    public TarotCommandModule(TarotService tarotService,TimerService timerService, ILogger<TarotCommandModule> logger)
    {
        _tarotService = tarotService;
        _timerService = timerService;
        _logger = logger;

        TimerJob ClearDrawJob = new(nameof(ClearDrawJob), 0, TimerJobTiming.TriggerDailyAtSetMinute, _tarotService.ClearDraws);
        _timerService.RegisterJob(ClearDrawJob);
    }

    [SlashCommand("kartadnia", "Losuje karte dnia tarota")]
    public async Task TarotCard()
    {

        if (Context.Guild.Id == 606253518281768983)
        {
            if (RandomNumberGenerator.GetInt32(1000) == 0)
            {
                await RespondAsync("Otrzymałeś kulkę w głowę w rosyjskiej ruletce...");
                try
                {
                    await Context.User.SendMessageAsync("Niestety przegrałeś w rosyjską ruletce, oto twoja kulka w głowę\nhttps://cdn.discordapp.com/attachments/1036702964754165901/1242471907953999966/IMG_1507.png?ex=664df5a2&is=664ca422&hm=ef422c38d8edb1fec05f074f5222cd2223d8a36a35b391ab980dd5c4f54021cf&");

                }
                catch (HttpException)
                {
                    _logger.Log(LogLevel.Information, "{Username} doesn't allow DM's from random people",Context.User.Username);
                    await FollowupAsync("... ale masz zablokowane DMy. Jednak masz dzisiaj szczęście!");
                }
                return;
            }
        }

        if (Context.User.Id == _schizoID)
            await RespondAsync("Przepraszam ale nie mam schizofrenii i nie rozmawiam sama ze sobą");
        else if (Context.User.Id == _alcoholicID)
            await RespondAsync("Przepraszam ale nie mam problemu z alkoholem, nie dam ci nic na kreske, nie mam żadnego benzo i proszę nie wysyłaj więcej zdjęć swojego przyrodzenia...");
        else
            await SendTarotCard();
    }

    private async Task SendTarotCard()
    {
        var userDraw = _tarotService.GetUserDrawInfo(Context.User.Id, Context.Guild.Id);
        if (userDraw != null)
        {
            await RespondAsync($"<@{userDraw.UserId}> https://discord.com/channels/{Context.Guild.Id}/{userDraw.Message.ChannelId}/{userDraw.Message.MessageId}");
            return;
        }

        TarotCard card = _tarotService.GetRandomCard();

        string desc = String.Format("**{0}**{1}\r\n```{2}```",
            card.Name,
            String.IsNullOrEmpty(card.Quote) ? null : "\r\n> " + card.Quote,
            card.Description);
        await base.RespondWithFileAsync(filePath: TarotService.GetRandomCardPhotoPath(card), text: desc);
        IUserMessage botResponse = await GetOriginalResponseAsync();

        _tarotService.SaveCardToUser(Context.User.Id, botResponse.Id, base.Context.Guild.Id, base.Context.Channel.Id);
    }
}