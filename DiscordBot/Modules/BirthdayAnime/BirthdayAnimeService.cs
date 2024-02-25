using DiscordBot.Modules.BirthdayAnime.Models;
using DiscordBot.Modules.Config.Models;
using DiscordBot.Modules.Config;
using DiscordBot.Modules.Tarot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.Services;
using System.Threading.Channels;

namespace DiscordBot.Modules.BirthdayAnime
{
    public class BirthdayAnimeService : InteractionModuleBase<SocketInteractionContext>
    {
        List<BirthdayAnimeModel> _models;
        DiscordSocketClient _socketClient;

        public BirthdayAnimeService(
            DiscordSocketClient client)
        {
            _socketClient = client;
            using (var s = new StreamReader("Modules/BirthdayAnime/JsonFiles/birthdayanime.json"))
            {
                var jsonString = s.ReadToEnd();
                try
                {
                    _models = JsonConvert.DeserializeObject<List<BirthdayAnimeModel>>(jsonString) ?? throw new Exception();
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to read birthdayanime.json! \n" + ex.Message);
                }
                if (_models == null)
                    throw new Exception("Failed to read birthdayanime.json!");
            }
        }

        public BirthdayAnimeModel? CheckBirthday(string date)
            => _models.Where(x => x.Date == date).FirstOrDefault();

        public async Task SendMessage(BirthdayAnimeModel anime)
        {
            ConfigBotService configBotService = new ConfigBotService();
            List<ConfigModel> models = configBotService.GetConfigModels();
            foreach (ConfigModel model in models)
            {
                if (model.BirthdayChannelId != 1)
                {
                    var channel = (SocketTextChannel)_socketClient.GetChannel(model.BirthdayChannelId);
                    channel.SendMessageAsync($"Dzisiaj urodzili się: {anime.Characters}");
                }
            }
        }
    }
}
