using DiscordBot.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class TarotService : InteractionModuleBase<SocketInteractionContext>
    {
        private List<TarotCard> LoadJson()
        {
            List<TarotCard> cards = new List<TarotCard>();
            using (StreamReader r = new StreamReader("JsonFiles/tarotcards.json"))
            {
                var json = r.ReadToEnd();
                var jarray = JArray.Parse(json);
                foreach (var item in jarray)
                {
                    TarotCard card = item.ToObject<TarotCard>();
                    cards.Add(card);
                }
            }

            return cards;
        }

        public TarotCard GetRandomCard()
        {
            TarotCard card = new TarotCard();
            List<TarotCard> cards = LoadJson();

            Random random = new Random();

            int r = random.Next(cards.Count);

            Console.WriteLine(cards[r].name);

            return cards[r];
        }

        public TarotCard GetCard(string name)
        {
            TarotCard card = new TarotCard();
            List<TarotCard> cards = LoadJson();

            foreach (TarotCard item in cards)
            {
                if (item.name == name)
                    return item;
            }
            return card;
        }

        public string GetRandomCardPhotoPath(TarotCard card)
        {
            string path = $"{System.IO.Directory.GetCurrentDirectory()}/tarotphotos/{card.name}.jpeg";
            return path;
        }

        public List<TarotCardsUsed> GetAllUsers()
        {
            List<TarotCardsUsed> usedCards = new List<TarotCardsUsed>();
            using (StreamReader r = new StreamReader("JsonFiles/tarotcardsused.json"))
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
            TarotCardsUsed foundObject = usedCards.Find(obj => obj.id == id);
            if (foundObject != null)
            {
                foundObject.usedTime += 1;
                foreach (BotMessageId bot in foundObject.botMessagesId)
                {
                    if (bot.guildId == guildId)
                    {
                        return;
                    }
                }
                BotMessageId botAdd = new BotMessageId();
                botAdd.guildId = guildId;
                botAdd.messageId = botMessageId;
                botAdd.channelId = channelId;
                if (foundObject.botMessagesId != null)
                {
                    user.botMessagesId = foundObject.botMessagesId;
                    user.botMessagesId.Add(botAdd);
                }

            }
            else
            {
                user.id = id;
                user.card = card;
                user.usedTime = usedTime;
                BotMessageId userIds = new BotMessageId();
                userIds.guildId = guildId;
                userIds.messageId = botMessageId;
                userIds.channelId = channelId;
                if (user.botMessagesId != null)
                {
                    user.botMessagesId.Add(userIds);
                    Console.Write("not null");
                }
                else
                {
                    user.botMessagesId = new List<BotMessageId>();
                    user.botMessagesId.Add(userIds);
                    Console.Write("null");
                }

                usedCards.Add(user);
            }

            // save all users
            string jsonf = JsonConvert.SerializeObject(usedCards.ToArray());
            System.IO.File.WriteAllText("JsonFiles/tarotcardsused.json", jsonf);
        }

        public TarotCardsUsed CheckIfUserUsedCard(string userId)
        {
            List<TarotCardsUsed> usedCards = GetAllUsers();

            foreach (TarotCardsUsed item in usedCards)
            {
                if (item.id == userId)
                    return item;
            }
            return null;
        }

        public void SaveTimeTarotCardUsed(string userId, ulong botMessageId, ulong guildId, ulong channelId)
        {
            List<TarotCardsUsed> usedCards = GetAllUsers();

            foreach (TarotCardsUsed item in usedCards)
            {
                if (item.id == userId)
                {
                    if (item.botMessagesId != null)
                    {
                        foreach (BotMessageId bot in item.botMessagesId)
                        {
                            if (bot.guildId != guildId)
                            {
                                SaveCardToUser(item.id, item.card, item.usedTime, botMessageId, guildId, channelId);
                                return;
                            }
                        }
                    }
                    item.usedTime += 1;
                    SaveCardToUser(item.id, item.card, item.usedTime, botMessageId, guildId, channelId);
                }

            }
        }
    }
}