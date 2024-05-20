using Discord.Commands;
using DiscordBot.Modules.RaiderIO.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBot.Modules.RaiderIO
{
    public class RaiderIOModule : InteractionModuleBase<SocketInteractionContext>
    {
        public RaiderIOService RaiderIOService { get; set; }

        [SlashCommand("score", "Pokaż score danej postaci")]
        public async Task ScoreSend([Name("Nazwapostaci")] [MinLength(2)]string charName, [Name("Nazwaserwera")] [MinLength(4)]string realmName)
        {
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
}
