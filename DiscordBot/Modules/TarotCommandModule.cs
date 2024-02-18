using DiscordBot.Services;
using DiscordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class TarotCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        public TarotService TarotService { get; set; }
        private readonly ILogger<CommandModule> _logger;


        [SlashCommand("kartadnia", "XDDDD")]
        public async Task TarotCard()
        {
            await SendTarotCard(Context);
        }

        public async Task SendTarotCard(SocketInteractionContext message)
        {
            TarotCardsUsed user = TarotService.CheckIfUserUsedCard(message.User.Id.ToString());
            TarotCard card = new TarotCard();
            IUserMessage botMessage = null;

            if (user != null)
                card = TarotService.GetCard(user.card);
            else
                card = TarotService.GetRandomCard();

            if (user != null && user.usedTime >= 1)
            {
                if (message.Channel is SocketGuildChannel guildChannel)
                {
                    ulong guild = guildChannel.Guild.Id;

                    foreach (BotMessageId item in user.botMessagesId)
                    {
                        if (item.guildId == guild)
                        {
                            await RespondAsync($"<@{user.id}> https://discord.com/channels/{item.guildId}/{item.channelId}/{item.messageId}");
                            return;
                        }
                    }
                    string desc = $"**{card.name}**```{card.description}```";
                    await RespondWithFileAsync(filePath: TarotService.GetRandomCardPhotoPath(card), text: desc);
                    IUserMessage userx = await GetOriginalResponseAsync();
                    ulong guildId = Context.Guild.Id;
                    ulong channelId = Context.Channel.Id;

                    TarotService.SaveTimeTarotCardUsed(user.id, userx.Id, guildId, channelId);
                }
            }
            else
            {
                string desc = $"**{card.name}**```{card.description}```";
                await RespondWithFileAsync(filePath: TarotService.GetRandomCardPhotoPath(card), text: desc);
                IUserMessage userx = await GetOriginalResponseAsync();

                ulong guildId = Context.Guild.Id;
                ulong channelId = Context.Channel.Id;
                ulong testId = userx.Id;
                TarotService.SaveCardToUser(message.User.Id.ToString(), card.name, 1, testId, guildId, channelId);
            }
        }
    }
}
