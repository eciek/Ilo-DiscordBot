using DiscordBot.Helpers;
using DiscordBot.Models;
using DiscordBot.Modules.Tarot.Models;
using DiscordBot.Services;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace DiscordBot.Modules.Tarot
{
    public class TarotService : ServiceWithJsonData<TarotDraw>
    {
        //readonly List<TarotCard> _cards;
        private readonly TimerService _timerService;

        private readonly List<TarotCard> _cards;
        private const string _tarotCardsPath = "Modules/Tarot/JsonFiles/tarotcards.json";

        protected override string ModulePath => "tarot";

        protected override string ModuleJson => "cardsDrawn.json";

        public TarotService(
            TimerService timerService) : base()
        {
            _cards = GetCardsFromJson();
            _timerService = timerService;

            TimerJob ClearDrawJob = new(nameof(ClearDrawJob), 0, TimerJobTiming.TriggerDailyAtSetMinute, ClearDraws);
            _timerService.RegisterJob(ClearDrawJob);
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
}