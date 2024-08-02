using DiscordBot.Models;
using DiscordBot.Services;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace DiscordBot.Modules.Reminder;

public class ReminderService : BackgroundService
{
    private readonly TimerService _timerService;
    private readonly ILogger<ReminderService> _logger;
    private readonly DiscordChatService _chatService;
    private readonly DiscordSocketClient _client;

    public ReminderService(
        TimerService timerService,
        ILogger<ReminderService> logger,
        DiscordChatService chatService,
        DiscordSocketClient client)
    {
        _timerService = timerService;
        _logger = logger;
        _chatService = chatService;
        _client = client;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TimerJob checkRemindersJob = new(nameof(checkRemindersJob), 1, TimerJobTiming.NowAndRepeatOnInterval, CheckReminderCache);
        _timerService.RegisterJob(checkRemindersJob);
        return Task.CompletedTask;
    }

    private void CheckReminderCache()
    {
        var now = DateTime.Now;

        var path = $"cache/{now:yyyy-MM-dd}";

        if (!Directory.Exists(path))
            return;

        foreach (var filePath in Directory.GetFiles(path, "message*.json"))
        {
            var reminder = ParseReminderFromFile(filePath);

            if (reminder == null)
                continue;

            if (reminder.TriggerDate <= now)
            {
                try
                {
                    HandleReminder(reminder);
                }
                catch
                {
                    File.Copy(filePath, filePath + ".error");
                    
                }
                File.Delete(filePath); 
            }
        }
    }

    private async void HandleReminder(Reminder reminder)
    {
        // check if bot still exists in guild, otherwise return
        SocketGuild? targetGuild = _client.Guilds.Where(x => x.Id == reminder.GuildId).FirstOrDefault();
        
        if (targetGuild is null)
        {
            _logger.LogError("Reminder[{name}] triggered for a guild [{guild}] that does not exist anymore",reminder.TriggerDate, reminder.GuildId);
            throw new Exception("Reminder triggered for a guild that does not exist anymore");
        }

        if(targetGuild.Channels.Any(x=>x.Id == reminder.ChannelId) == false)
        {
            _logger.LogError("Reminder[{name}] triggered for a guild [{guild}] chanel [{channel}]that does not exist anymore", reminder.TriggerDate, reminder.GuildId, reminder.ChannelId);
            throw new Exception("Reminder triggered for a guild chanel that does not exist anymore");
        }

        List<FileAttachment> attachments = new List<FileAttachment>();

        foreach (var attachmentPath in reminder.Attachments)
        {
            if(File.Exists(attachmentPath))
                attachments.Add(new FileAttachment(attachmentPath));
        }

        await _chatService.SendFiles(reminder.ChannelId, reminder.Message, [.. attachments]);
    }

    private void DeleteReminder(Reminder reminder)
    {
        throw new NotImplementedException();
    }

    private Reminder? ParseReminderFromFile(string reminderFile)
    {
        var json = File.ReadAllText(reminderFile);
        Reminder? reminder;

        try
        {
            reminder = JsonConvert.DeserializeObject<Reminder>(json);
        }

        catch (Exception ex)
        {
            _logger.LogError("Error while parsing reminder {reminderFile}. error: {ex}", reminderFile, ex);
            return null;
        }

        if (reminder == null)
        {
            _logger.LogError("Error while parsing reminder {reminderFile}. File is null", reminderFile);
            return null;
        }

        return reminder;

    }
}
