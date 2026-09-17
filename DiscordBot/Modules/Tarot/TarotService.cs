using System.Security.Cryptography;
using DiscordBot.Models;
using DiscordBot.Modules.Tarot.Models;
using DiscordBot.Services;
using Newtonsoft.Json;

namespace DiscordBot.Modules.Tarot;

public class TarotService : ServiceWithJsonData<TarotDraw>
{
    private readonly List<TarotCard> _cards;
    private const string _tarotCardsPath = "Modules/Tarot/JsonFiles/tarotcards.json";

    protected override string ModulePath => "tarot";

    protected override string ModuleJson => "cardsDrawn.json";

    private readonly ILogger<TarotService> _logger;

    public TarotService(TimerService timerService, ILogger<TarotService> logger) : base()
    {
        _logger = logger;
        _cards = GetCardsFromJson();

        try
        {
            var clearJob = new TimerJob(nameof(ClearDraws), 0, TimerJobTiming.TriggerDailyAtSetMinute, ClearDraws);
            timerService.RegisterJob(clearJob);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register Tarot timer job");
        }
    }

    public TarotCard GetRandomCard()
        => _cards[RandomNumberGenerator.GetInt32(_cards.Count)];

    public static string GetRandomCardPhotoPath(TarotCard card)
        => $"Modules/Tarot/tarotphotos/{card.Name}.png";

    public TarotDraw? GetUserDrawInfo(ulong userId, ulong guildId)
        => GetGuildData(guildId).Where(x => x.UserId == userId).FirstOrDefault();

    public void SaveCardToUser(ulong userId, ulong messageId, ulong guildId, ulong channelId)
    {
        var usedCards = GetGuildData(guildId);

        if (usedCards.Where(x => x.UserId == userId).Any())
        {
            return;
        }

        var draw = new TarotDraw()
        {
            UserId = userId,
            Message = new()
            {
                MessageId = messageId,
                ChannelId = channelId
            }
        };

        moduleData[guildId].Add(draw);
        SynchronizeJson();
    }

    public void ClearDraws()
    {
        foreach (var guildDraws in moduleData.Values)
        {
            guildDraws.Clear();
        }
        SynchronizeJson();
    }

    private static List<TarotCard> GetCardsFromJson()
    {
        var cards = new List<TarotCard>();
        using (var s = new StreamReader(_tarotCardsPath))
        {
            var jsonString = s.ReadToEnd();
            try
            {
                cards = JsonConvert.DeserializeObject<List<TarotCard>>(jsonString) ?? throw new Exception();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to read tarotcards.json! \n" + ex.Message);
            }
            if (cards == null || cards.Count == 0)
                throw new Exception("Failed to read tarotcards.json!");
        }
        return cards;
    }

}
