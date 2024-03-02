using DiscordBot.Modules.Tarot.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Security.Cryptography;
using DiscordBot.Models;
using DiscordBot.Services;

namespace DiscordBot.Modules.Tarot
{
    public class TarotService : InteractionModuleBase<SocketInteractionContext>
    {
        List<TarotCard> _cards;
        TimerService _timerService;
        DateTime _today;

        public TarotService(
            TimerService timerService) 
        {
            using (var s = new StreamReader("Modules/Tarot/JsonFiles/tarotcards.json"))
            {
                var jsonString = s.ReadToEnd();
                try
                {
                    _cards = JsonConvert.DeserializeObject<List<TarotCard>>(jsonString) ?? throw new Exception();
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to read tarotcards.json! \n" + ex.Message);
                }
                if (_cards == null)
                    throw new Exception("Failed to read tarotcards.json!");
            }

            _today = DateTime.Now;
            _timerService = timerService;
            

            TimerJob tarotClearJob = new(nameof(tarotClearJob), 0, TimerJobTiming.TriggerDailyAtSetHour, ClearUsers);
            _timerService.RegisterJob(tarotClearJob);
        }

        public TarotCard GetRandomCard()
            => _cards[RandomNumberGenerator.GetInt32(_cards.Count)];

        public TarotCard GetCard(string name)
        {
            TarotCard card = new TarotCard();
            List<TarotCard> cards = _cards;

            foreach (TarotCard item in cards)
            {
                if (item.Name == name)
                    return item;
            }
            return card;
        }

        public string GetRandomCardPhotoPath(TarotCard card)
        {
            string path = $"{System.IO.Directory.GetCurrentDirectory()}/tarotphotos/{card.Name}.jpeg";
            return path;
        }

        public List<TarotCardsUsed> GetAllUsers()
        {
            List<TarotCardsUsed> usedCards = new List<TarotCardsUsed>();
            using (StreamReader r = new StreamReader("Modules/Tarot/JsonFiles/tarotcardsused.json"))
            {
                var json = r.ReadToEnd();
                var jarray = JArray.Parse(json);
                foreach (var item in jarray)
                {
                    TarotCardsUsed usedCard = item.ToObject<TarotCardsUsed>();
                    usedCards.Add(usedCard);
                }
            }
            return usedCards;
        }

        public void SaveCardToUser(string id, string card, int usedTime, ulong botMessageId, ulong guildId, ulong channelId)
        {
            TarotCardsUsed user = new TarotCardsUsed();
            List<TarotCardsUsed> usedCards = GetAllUsers();
            TarotCardsUsed foundObject = usedCards.Find(obj => obj.Id == id);
            if (foundObject != null)
            {
                foundObject.UsedTime += 1;
                foreach (BotMessageId bot in foundObject.BotMessagesId)
                {
                    if (bot.GuildId == guildId)
                    {
                        return;
                    }
                }
                BotMessageId botAdd = new BotMessageId();
                botAdd.GuildId = guildId;
                botAdd.MessageId = botMessageId;
                botAdd.ChannelId = channelId;
                if (foundObject.BotMessagesId != null)
                {
                    user.BotMessagesId = foundObject.BotMessagesId;
                    user.BotMessagesId.Add(botAdd);
                }
            }
            else
            {
                user.Id = id;
                user.Card = card;
                user.UsedTime = usedTime;
                BotMessageId userIds = new BotMessageId();
                userIds.GuildId = guildId;
                userIds.MessageId = botMessageId;
                userIds.ChannelId = channelId;
                if (user.BotMessagesId != null)
                {
                    user.BotMessagesId.Add(userIds);
                    Console.Write("not null");
                }
                else
                {
                    user.BotMessagesId = new List<BotMessageId>();
                    user.BotMessagesId.Add(userIds);
                    Console.Write("null");
                }
                usedCards.Add(user);
            }
            // save all users
            string jsonf = JsonConvert.SerializeObject(usedCards.ToArray());
            System.IO.File.WriteAllText("Modules/Tarot/JsonFiles/tarotcardsused.json", jsonf);
        }

        public TarotCardsUsed? CheckIfUserUsedCard(string userId)
            => GetAllUsers().Where(x => x.Id == userId).FirstOrDefault();

        public void SaveTimeTarotCardUsed(string userId, ulong botMessageId, ulong guildId, ulong channelId)
        {
            List<TarotCardsUsed> usedCards = GetAllUsers();

            foreach (TarotCardsUsed item in usedCards)
            {
                if (item.Id == userId)
                {
                    if (item.BotMessagesId != null)
                    {
                        foreach (BotMessageId bot in item.BotMessagesId)
                        {
                            if (bot.GuildId != guildId)
                            {
                                SaveCardToUser(item.Id, item.Card, item.UsedTime, botMessageId, guildId, channelId);
                                return;
                            }
                        }
                    }
                    item.UsedTime += 1;
                    SaveCardToUser(item.Id, item.Card, item.UsedTime, botMessageId, guildId, channelId);
                }
            }
        }

        public async void ClearUsers()
        {
            Console.WriteLine("Cleared");
            string jsonstring = "[{\"id\":\"null\",\"card\":\"null\",\"usedTime\":0,\"botMessagesId\":[{\"guildId\":1,\"messageId\":1,\"channelId\":1}]}]";
            File.WriteAllText("Modules/Tarot/JsonFiles/tarotcardsused.json", jsonstring);
        }
    }
}