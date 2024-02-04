using Discord.WebSocket;
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
            using (StreamReader r = new StreamReader("C:\\Users\\Paweł\\Desktop\\Hepii\\DiscordBot\\DiscordBot\\tarotcards.json"))
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

        public string GetRandomCardPhotoPath(TarotCard card)
        {
            string path = $"{System.IO.Directory.GetCurrentDirectory()}\\tarotphotos\\{card.name}.jpg";
            return path;
        }
    }
}
