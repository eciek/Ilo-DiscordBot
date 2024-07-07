using DiscordBot.Helpers;
using DiscordBot.Models;
using DiscordBot.Modules.Tarot.Models;
using DiscordBot.Services;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace DiscordBot.Modules.Tarot
{
    public class TarotService : InteractionModuleBase<SocketInteractionContext>
    {
        //readonly List<TarotCard> _cards;
        private readonly TimerService _timerService;

        private readonly List<TarotCard> _cards;
        private const string _tarotCardsPath = "Modules/Tarot/JsonFiles/tarotcards.json";

        private const string _dataRoot = "data";
        private const string _modulePath = "tarot";
        // 0 - guildId
        private const string _cardsDrawnJson = "cardsDrawn.json";
        private readonly Dictionary<ulong, List<TarotDraw>> _cardsDrawn;

        public TarotService(
            TimerService timerService)
        {
            string[] registeredGuilds;
            _cardsDrawn = [];
            _timerService = timerService;
            _cards = GetCardsFromJson();

            try
            {
                registeredGuilds = Directory.GetDirectories(_dataRoot);
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(_dataRoot);
                registeredGuilds = Directory.GetDirectories(_dataRoot);
            }

            foreach (var registeredGuild in registeredGuilds)
            {
                _cardsDrawn.Add(ulong.Parse(registeredGuild.TrimLetters()), []);
            }
            LoadDrawsFromJson();

            TimerJob ClearDrawJob = new(nameof(ClearDrawJob), 0, TimerJobTiming.TriggerDailyAtSetMinute, ClearDraws);
            _timerService.RegisterJob(ClearDrawJob);
        }

        public TarotCard GetRandomCard()
            => _cards[RandomNumberGenerator.GetInt32(_cards.Count)];

        public static string GetRandomCardPhotoPath(TarotCard card)
            => $"Modules/Tarot/tarotphotos/{card.Name}.png";

        private List<TarotDraw> GetAllUsers(ulong guildId)
            => _cardsDrawn.TryGetValue(guildId, out var draws) ? draws : [];

        public TarotDraw? GetUserDrawInfo(ulong userId, ulong guildId)
        => GetAllUsers(guildId).Where(x => x.UserId == userId).FirstOrDefault();

        public void SaveCardToUser(ulong userId, ulong messageId, ulong guildId, ulong channelId)
        {
            var usedCards = GetAllUsers(guildId);

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

            if (!_cardsDrawn.TryGetValue(guildId, out var list))
            {
                _cardsDrawn.Add(guildId, []);
            }
            _cardsDrawn[guildId].Add(draw);
            SynchronizeJson();
        }

        public void ClearDraws()
        {

            foreach (var guildDraws in _cardsDrawn.Values)
            {
                guildDraws.Clear();
            }

            SynchronizeJson();
        }

        private void SynchronizeJson()
        {
            Console.WriteLine("Saving TarotDraw.json...");
            foreach (var guildCardDraws in _cardsDrawn)
            {
                var path = Path.Combine(_dataRoot, guildCardDraws.Key.ToString(), _modulePath);
                var filePath = Path.Combine(path, _cardsDrawnJson);
                string json = JsonConvert.SerializeObject(guildCardDraws.Value, Formatting.Indented);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                using var stream = new StreamWriter(new FileStream(filePath, FileMode.Create));
                stream.Write(json);
            }
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

        private async void LoadDrawsFromJson()
        {
            Console.WriteLine($"Loading {_cardsDrawnJson}...");
            foreach (var cardsDrawn in _cardsDrawn)
            {
                var filePath = Path.Combine(_dataRoot, cardsDrawn.Key.ToString(), _modulePath, _cardsDrawnJson);
                string json;

                if (File.Exists(filePath) == false)
                    return;

                using (var stream = new StreamReader(filePath))
                {
                    try
                    {
                        json = await stream.ReadToEndAsync();
                    }
                    catch (FileNotFoundException)
                    {
                        json = string.Empty;
                    }
                }

                if (string.IsNullOrEmpty(json))
                {
                    _cardsDrawn[cardsDrawn.Key] ??= [];
                    continue;
                }

                _cardsDrawn[cardsDrawn.Key] = JsonConvert.DeserializeObject<List<TarotDraw>>(json)!;
            }
        }
    }
}