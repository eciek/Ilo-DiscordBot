using Discord;
using Discord.Net;
using DiscordBot.Modules.AntiSpam;
using DiscordBot.Modules.RaiderIO.Models;
using DiscordBot.Modules.Tarot.Models;
using System.Security.Cryptography;

namespace DiscordBot.Modules.Tarot
{
    public class TarotCommandModule(TarotService tarotService) : InteractionModuleBase<SocketInteractionContext>
    {
        private TarotService TarotService { get; set; } = tarotService;

        private const long _schizoID = 792728812730449941;
        private const long _alcoholicID = 1192192605941932093;

        [SlashCommand("kartadnia", "Losuje karte dnia tarota")]
        public async Task TarotCard()
        {

            if (Context.Guild.Id == 606253518281768983)
            {
                if (RandomNumberGenerator.GetInt32(1000) == 0)
                {
                    try
                    {
                        await Context.User.SendMessageAsync("Niestety przegrałeś/aś w rosyjskiej ruletce, oto twoja kulka w głowę\nhttps://cdn.discordapp.com/attachments/1036702964754165901/1242471907953999966/IMG_1507.png?ex=664df5a2&is=664ca422&hm=ef422c38d8edb1fec05f074f5222cd2223d8a36a35b391ab980dd5c4f54021cf&");
                        await RespondAsync($"Otrzymałeś/aś kulkę w głowę w rosyjskiej ruletce...");
                    }
                    catch (HttpException){ }
                    return;
                }
            }

            if (Context.User.Id == _schizoID)
                await RespondAsync("Przepraszam ale nie mam schizofrenii i nie rozmawiam sama ze sobą");
            else if (Context.User.Id == _alcoholicID)
                await RespondAsync("Przepraszam ale nie mam problemu z alkoholem, nie dam ci nic na kreske, nie mam żadnego benzo i proszę nie wysyłaj więcej zdjęć swojego przyrodzenia...");
            else
                await SendTarotCard();
        }

        private async Task SendTarotCard()
        {
            var userDraw = TarotService.GetUserDrawInfo(Context.User.Id, Context.Guild.Id);
            if (userDraw != null)
            {
                await RespondAsync($"<@{userDraw.UserId}> https://discord.com/channels/{Context.Guild.Id}/{userDraw.Message.ChannelId}/{userDraw.Message.MessageId}");
                return;
            }

            TarotCard card = TarotService.GetRandomCard();

            string desc = String.Format("**{0}**{1}\r\n```{2}```",
                card.Name,
                String.IsNullOrEmpty(card.Quote) ? null : "\r\n> " + card.Quote,
                card.Description);
            await base.RespondWithFileAsync(filePath: TarotService.GetRandomCardPhotoPath(card), text: desc);
            IUserMessage botResponse = await GetOriginalResponseAsync();

            TarotService.SaveCardToUser(Context.User.Id, botResponse.Id, base.Context.Guild.Id, base.Context.Channel.Id);
        }
    }
}