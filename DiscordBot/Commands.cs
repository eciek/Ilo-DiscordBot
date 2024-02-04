using Discord.WebSocket;
using DiscordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    public class Commands
    {
        Tarot tarot = new Tarot();

        // Commands
        public async void CheckCommand(SocketMessage message)
        {
            if (message.Content == "!ping")
                await message.Channel.SendMessageAsync("test");

            if (message.Content == "!kartadnia")
                SendTarotCard(message);
        }

        async void SendTarotCard(SocketMessage message)
        {
            TarotCardsUsed user = tarot.CheckIfUserUsedCard(message.Author.Id.ToString());
            TarotCard card = new TarotCard();
            if (user != null)
            {
                card = tarot.GetCard(user.card);
            }
            else
            {
                card = tarot.GetRandomCard();
                tarot.SaveCardToUser(message.Author.Id.ToString(), card.name);
            }

            string desc = $"**{card.name}**```{card.description}```";
            await message.Channel.SendFileAsync(tarot.GetRandomCardPhotoPath(card), desc, messageReference: new Discord.MessageReference(message.Id));

            //save card to user

        }
    }
}
