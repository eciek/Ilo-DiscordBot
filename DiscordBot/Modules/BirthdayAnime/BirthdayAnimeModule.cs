using DiscordBot.Models;
using DiscordBot.Modules.AnimeBirthdays;
using DiscordBot.Modules.AnimeBirthdays.Models;
using DiscordBot.Services;
using System.Security.Cryptography;
using System.Text;

namespace DiscordBot.Modules.GuildConfig
{
    public class BirthdayAnimeModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly BirthdayAnimeService _birthdayAnimeService;
        private readonly BooruService _booruService;
        private readonly TimerService _timerService;


        public BirthdayAnimeModule(
            BirthdayAnimeService birthdayAnimeService, 
            TimerService timerService,
            BooruService booruService)
        {
            _birthdayAnimeService = birthdayAnimeService;
            _timerService = timerService;
            _booruService = booruService;

            TimerJob birthdaySendMessageJob = new(nameof(birthdaySendMessageJob), 0, TimerJobTiming.TriggerDailyAtSetMinute, SendMessageAdapter);
            _timerService.RegisterJob(birthdaySendMessageJob);
        }

        private async void SendMessageAdapter()
        {
            await SendMessage();
        }

        [SlashCommand("urodziny", "Urodziny postaci z anime")]
        public async Task SendMessage()
        {
            var characters = _birthdayAnimeService.GetAnimeCharacters(DateTime.Now.ToString("dd.MM"));
            var msgBuilder = new StringBuilder();
            msgBuilder.AppendLine("Dzisiaj urodzili się:");
            
            List<Embed> embeds = new List<Embed>();

            if (characters.Count == 0)
            {
                await RespondAsync($"Zapomniałam kto się dzisiaj urodził :(", ephemeral: true);
                return;
            }

            foreach (var character in characters)
            {
                msgBuilder.AppendLine(character.ToString());

                var charImages = _booruService.GetBooruImage(character.ToNameSurnameBooruSlug(),3).ToList();
                charImages.AddRange(_booruService.GetBooruImage(character.ToSurnameNameBooruSlug(),3).ToList());
                
                var selectedItem = charImages[RandomNumberGenerator.GetInt32(charImages.Count - 1)];

                if (selectedItem.EndsWith(".mp4"))
                {
                    var emb = new EmbedBuilder().WithUrl($"attachment://{selectedItem}").Build();
                    embeds.Add(emb);
                }
                else
                {
                    var emb = new EmbedBuilder().WithImageUrl($"attachment://{selectedItem}").Build();
                    embeds.Add(emb);
                }
            }

            await RespondAsync(msgBuilder.ToString(),embeds: embeds.ToArray(), ephemeral: true);
        }
    }
}