using Discord;
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
            IUserMessage botMessage = null;

            if (user != null)
                card = tarot.GetCard(user.card);
            else
                card = tarot.GetRandomCard();

            if (user != null && user.usedTime >= 1)
            {
                if (message.Channel is SocketGuildChannel guildChannel)
                {
                    ulong guild = guildChannel.Guild.Id;

                    foreach (BotMessageId item in user.botMessagesId)
                    {
                        if (item.guildId == guild)
                        {
                            botMessage = await message.Channel.SendMessageAsync($"<@{user.id}>", messageReference: new Discord.MessageReference(item.messageId));
                            return;
                        }
                    }
                    string desc = $"**{card.name}**```{card.description}```";
                    botMessage = await message.Channel.SendFileAsync(tarot.GetRandomCardPhotoPath(card), desc, messageReference: new Discord.MessageReference(message.Id));
                    var channel = botMessage.Channel as SocketGuildChannel;
                    ulong guildId = channel.Guild.Id;
                    tarot.SaveTimeTarotCardUsed(user.id, botMessage.Id, guildId);
                }
            }
            else
            {
                string desc = $"**{card.name}**```{card.description}```";
                botMessage = await message.Channel.SendFileAsync(tarot.GetRandomCardPhotoPath(card), desc, messageReference: new Discord.MessageReference(message.Id));

                var channel = botMessage.Channel as SocketGuildChannel;
                ulong guildId = channel.Guild.Id;

                tarot.SaveCardToUser(message.Author.Id.ToString(), card.name, 1, botMessage.Id, guildId);
            }

            



                




            //save card to user

        }
    }
}
