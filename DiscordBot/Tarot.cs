using Discord.WebSocket;
using DiscordBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DiscordBot
{
    public class Tarot
    {
        
        

        private List<TarotCard> LoadJson()
        {
            List<TarotCard> cards = new List<TarotCard>();
            using (StreamReader r = new StreamReader("C:\\Users\\Paweł\\Desktop\\Hepii\\DiscordBot\\DiscordBot\\JsonFiles\\tarotcards.json"))
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
            string path = $"{System.IO.Directory.GetCurrentDirectory()}\\tarotphotos\\{card.name}.jpg";
            return path;
        }

        public List<TarotCardsUsed> GetAllUsers()
        {
            List<TarotCardsUsed> usedCards = new List<TarotCardsUsed>();
            using (StreamReader r = new StreamReader("C:\\Users\\Paweł\\Desktop\\Hepii\\DiscordBot\\DiscordBot\\JsonFiles\\tarotcardsused.json"))
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

        public void SaveCardToUser(string id, string card)
        {
            TarotCardsUsed user = new TarotCardsUsed();
            List<TarotCardsUsed> usedCards = GetAllUsers();
            user.id = id;
            user.card = card;

            // get all users

            usedCards.Add(user);
            // save all users
            string jsonf = JsonConvert.SerializeObject(usedCards.ToArray());
            System.IO.File.WriteAllText("C:\\Users\\Paweł\\Desktop\\Hepii\\DiscordBot\\DiscordBot\\JsonFiles\\tarotcardsused.json", jsonf);
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
    }
}
