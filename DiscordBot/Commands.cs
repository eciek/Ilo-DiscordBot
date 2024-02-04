using Discord.WebSocket;
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
            TarotCard card = tarot.GetRandomCard();
            string desc = $"**{card.name}**```{card.description}```";
            await message.Channel.SendFileAsync(tarot.GetRandomCardPhotoPath(card), desc, messageReference: new Discord.MessageReference(message.Id));

        }
    }
}
