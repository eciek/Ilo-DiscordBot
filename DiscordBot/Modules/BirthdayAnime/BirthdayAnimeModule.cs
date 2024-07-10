using DiscordBot.Modules.BirthdayAnime;
using DiscordBot.Modules.BirthdayAnime.Models;

namespace DiscordBot.Modules.GuildConfig
{
    public class BirthdayAnimeModuke : InteractionModuleBase<SocketInteractionContext>
    {
        public BirthdayAnimeService BirthdayAnimeService { get; set; }

        [SlashCommand("urodziny", "Urodziny postaci z anime")]
        public async Task SendMessage()
        {
            BirthdayAnimeModel? model = BirthdayAnimeService.CheckBirthday(DateTime.Now.ToString("dd.MM"));
            await RespondAsync($"Dzisiaj urodzili się: {model.Characters}", ephemeral:true);
        }
    }
}