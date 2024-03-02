using DiscordBot.Models;
using DiscordBot.Modules.BirthdayAnime.Models;
using DiscordBot.Modules.Config;
using DiscordBot.Modules.Config.Models;
using DiscordBot.Services;
using Newtonsoft.Json;

namespace DiscordBot.Modules.BirthdayAnime;

public class BirthdayAnimeService : InteractionModuleBase<SocketInteractionContext>
{
    List<BirthdayAnimeModel> _models;
    DiscordSocketClient _socketClient;
    ConfigBotService _configBotService;
    TimerService _timerService;

    public BirthdayAnimeService(
        DiscordSocketClient client,
        ConfigBotService configBotService,
        TimerService timerService)
    {
        _socketClient = client;
        _configBotService = configBotService;
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

        _timerService = timerService;

        TimerJob birthdaySendMessageJob = new(nameof(birthdaySendMessageJob), 0, TimerJobTiming.TriggerDailyAtSetHour, SendMessage);
        _timerService.RegisterJob(birthdaySendMessageJob);
    }

    public BirthdayAnimeModel? CheckBirthday(string date)
        => _models.Where(x => x.Date == date).FirstOrDefault();

    public async void SendMessage()
    {
        BirthdayAnimeModel? characters = CheckBirthday(DateTime.Now.ToString("dd.MM"));
        //List<ConfigModel> models = new ConfigBotService().GetConfigModels();
        foreach (ConfigModel model in _configBotService.GetConfigModels().Where(x => x.BirthdayChannelId != 0))
        {
            var channel = (SocketTextChannel)_socketClient.GetChannel(model.BirthdayChannelId);
            channel.SendMessageAsync($"Dzisiaj urodzili się: {characters.Characters}");
            channel = null;
        }
    }
}