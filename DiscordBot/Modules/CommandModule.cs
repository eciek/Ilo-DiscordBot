using DiscordBot.Models;

namespace DiscordBot.Modules;

public class CommandModule : InteractionModuleBase<SocketInteractionContext>
{
    Tarot tarot = new Tarot();
    private readonly ILogger<CommandModule> _logger;

    public CommandModule(ILogger<CommandModule> logger)
    {
        _logger = logger;
    }

    [SlashCommand("test", "xd")]
    public async Task TestCommand()
        => await RespondAsync("testestset");

    [SlashCommand("kartadnia", "XDDDD")]
    public async Task TarotCard()
    {
        SendTarotCard(Context);
    }

    [SlashCommand("cd", "do usuniecia")]
    public async Task TestCmd()
    {
        await RespondAsync("dsadasd");
    }


    async void SendTarotCard(SocketInteractionContext message)
    {
        TarotCardsUsed user = tarot.CheckIfUserUsedCard(message.User.Id.ToString());
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
                        await RespondAsync($"<@{user.id}> https://discord.com/channels/{item.guildId}/{item.channelId}/{item.messageId}");
                        return;
                    }
                }
                string desc = $"**{card.name}**```{card.description}```";
                await RespondWithFileAsync(filePath: tarot.GetRandomCardPhotoPath(card), text: desc);
                IUserMessage userx = await GetOriginalResponseAsync();
                ulong guildId = Context.Guild.Id;
                ulong channelId = Context.Channel.Id;

                tarot.SaveTimeTarotCardUsed(user.id, userx.Id, guildId, channelId);
            }
        }
        else
        {
            string desc = $"**{card.name}**```{card.description}```";
            await RespondWithFileAsync(filePath: tarot.GetRandomCardPhotoPath(card), text: desc);
            IUserMessage userx = await GetOriginalResponseAsync();
            
            ulong guildId = Context.Guild.Id;
            ulong channelId = Context.Channel.Id;
            ulong testId = userx.Id;
            tarot.SaveCardToUser(message.User.Id.ToString(), card.name, 1, testId, guildId, channelId);
        }
    }
}