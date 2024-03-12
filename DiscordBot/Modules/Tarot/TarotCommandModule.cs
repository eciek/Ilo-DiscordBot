using DiscordBot.Modules.Tarot.Models;

namespace DiscordBot.Modules.Tarot
{
    public class TarotCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        public TarotService TarotService { get; set; }
        private readonly ILogger<CommandModule> _logger;

        private const long _schizoID = 792728812730449941;

        [SlashCommand("kartadnia", "Losuje karte dnia tarota")]
        public async Task TarotCard()
        {
            if (Context.User.Id == _schizoID)
                await RespondAsync("Przepraszam ale nie mam schizofrenii i nie rozmawiam sama ze sobą");
            else
                await SendTarotCard(Context);
        }

        public async Task SendTarotCard(SocketInteractionContext message)
        {
            TarotCardsUsed user = TarotService.CheckIfUserUsedCard(message.User.Id.ToString());
            TarotCard card = new TarotCard();
            IUserMessage botMessage = null;

            if (user != null)
                card = TarotService.GetCard(user.Card);
            else
                card = TarotService.GetRandomCard();

            if (user != null && user.UsedTime >= 1)
            {
                if (message.Channel is SocketGuildChannel guildChannel)
                {
                    ulong guild = guildChannel.Guild.Id;

                    foreach (BotMessageId item in user.BotMessagesId)
                    {
                        if (item.GuildId == guild)
                        {
                            await RespondAsync($"<@{user.Id}> https://discord.com/channels/{item.GuildId}/{item.ChannelId}/{item.MessageId}");
                            return;
                        }
                    }
                    string desc = $"**{card.Name}**```{card.Description}```";
                    await RespondWithFileAsync(filePath: TarotService.GetRandomCardPhotoPath(card), text: desc);
                    IUserMessage userx = await GetOriginalResponseAsync();
                    ulong guildId = Context.Guild.Id;
                    ulong channelId = Context.Channel.Id;

                    TarotService.SaveTimeTarotCardUsed(user.Id, userx.Id, guildId, channelId);
                }
            }
            else
            {
                string desc = $"**{card.Name}**```{card.Description}```";
                await RespondWithFileAsync(filePath: TarotService.GetRandomCardPhotoPath(card), text: desc);
                IUserMessage userx = await GetOriginalResponseAsync();

                ulong guildId = Context.Guild.Id;
                ulong channelId = Context.Channel.Id;
                ulong testId = userx.Id;
                TarotService.SaveCardToUser(message.User.Id.ToString(), card.Name, 1, testId, guildId, channelId);
            }
        }
    }
}