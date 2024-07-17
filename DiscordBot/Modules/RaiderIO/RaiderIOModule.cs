using Discord.Commands;
using DiscordBot.Modules.AntiSpam;
using DiscordBot.Modules.RaiderIO.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBot.Modules.RaiderIO
{
    public class RaiderIOModule : InteractionModuleBase<SocketInteractionContext>
    {
        public RaiderIOService RaiderIOService { get; set; }
        public AntiSpamService AntiSpamService { get; set; }

        //[SlashCommand("score", "Pokaż score danej postaci")]
        public async Task ScoreSend([Name("Nazwapostaci")][MinLength(2)] string charName, [Name("Nazwaserwera")][MinLength(4)] string realmName)
        {
            if (AntiSpamService._blockedUsers.Contains((long)Context.User.Id))
            {
                await RespondAsync($"Proszeee, zostaw mnie w spokoju, nyaa~! ✨💖🌸", ephemeral: true);
            }
            else
            {
                AntiSpamService._blockedUsers.Add((long)Context.User.Id);
                realmName.Replace(' ', '-').ToLower();
                CharacterRIO character = await RaiderIOService.GetData(charName, realmName);
                if (character == null)
                {
                    await RespondAsync("Spróbuj ponownie później!");
                }
                else
                {
                    await RespondWithFileAsync(filePath: Directory.GetCurrentDirectory() + $"/Sheets/{character.Name}.jpg");
                    File.Delete(Directory.GetCurrentDirectory() + $"/Sheets/{character.Name}.jpg");
                }
            }
        }

        //[SlashCommand("say", "Say")]
        public async Task Say([Name("Message")][MinLength(2)] string message)
        {
            if (Context.User.Id == 394196664388157440)
                await RespondAsync($"eciu: {message}");
            else
                await RespondAsync($"Proszeee, zostaw mnie w spokoju, nyaa~! ✨💖🌸", ephemeral: true);
        }
    }
}
