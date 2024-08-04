using DiscordBot.Models;
using DiscordBot.Modules.GuildLogging;
using DiscordBot.Services;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace DiscordBot.Modules.Reminder;

public class ReminderService(
    TimerService timerService,
    ILogger<ReminderService> logger,
    DiscordChatService chatService,
    DiscordSocketClient client,
    GuildLoggingService guildLoggingService) : BackgroundService
{
    private readonly TimerService _timerService = timerService;
    private readonly ILogger<ReminderService> _logger = logger;
    private readonly DiscordChatService _chatService = chatService;
    private readonly DiscordSocketClient _client = client;
    private readonly GuildLoggingService _guildLoggingService = guildLoggingService;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TimerJob checkRemindersJob = new(nameof(checkRemindersJob), 1, TimerJobTiming.NowAndRepeatOnInterval, CheckReminder);
        _timerService.RegisterJob(checkRemindersJob);
        return Task.CompletedTask;
    }

    private void CheckReminder()
    {
        CheckInput();

        CheckCache();
    }

    private void CheckInput()
    {
        var sourcePath = $"input";


        if (!Directory.Exists(sourcePath))
            return;

        foreach (var filePath in Directory.GetFiles(sourcePath, "message*.json"))
        {
            try
            {
                var reminder = ParseReminderFromFile(filePath);

                if (reminder == null)
                    continue;

                ValidateReminder(reminder, out _);

                var targetPath = Path.Combine($"cache", reminder.TriggerDate.ToString("yyyy-MM-dd"));


                foreach (var attachment in reminder.Attachments)
                {
                    var newAttachmentName = attachment;
                    if (File.Exists(Path.Combine(targetPath, attachment)))
                    {
                        var extension = attachment.Split('.').Last();
                        newAttachmentName = Guid.NewGuid().ToString() + (String.IsNullOrEmpty(extension) ? string.Empty : "." + extension);
                    }

                    File.Move(
                        Path.Combine(sourcePath, attachment),
                        Path.Combine(targetPath, newAttachmentName));
                }

                var messageCount = Directory.GetFiles(targetPath, "message*.json").Length;

                var messageFilename = $"message{(messageCount > 0 ? messageCount + 1 : string.Empty)}.json";

                File.Move(
                    filePath,
                    Path.Combine(targetPath, messageFilename)
                    );

                if (reminder.TriggerDate > DateTime.Now)
                    _guildLoggingService.GuildLog(reminder.GuildId, $"Zarejestrowałam nową wiadomość! Wyślę ją o {reminder.TriggerDate} (｡•̀ᴗ-)✧");

                _logger.LogInformation("Message from Input received. Registered for guild [{guildId}], triggerDate: [{triggerDate}]", reminder.GuildId, reminder.TriggerDate);
            }
            catch (Exception ex)
            {
                var faultyFilename = $"{filePath}{DateTime.Now}.json";
                _logger.LogError("CheckInput error for [{filename}]; error: [{error}]", faultyFilename, ex.Message);
            }
        }

    }

    private async void CheckCache()
    {
        var now = DateTime.Now;

        var path = Path.Combine("cache", now.ToString("yyyy-MM-dd"));

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
                    ValidateReminder(reminder, out _);

                    List<FileAttachment> attachments = [];

                    foreach (var attachmentName in reminder.Attachments)
                    {
                        var attachmentPath = Path.Combine(path, attachmentName);
                        if (File.Exists(attachmentPath))
                            attachments.Add(new FileAttachment(attachmentPath));
                    }

                    await _chatService.SendFiles(reminder.ChannelId, reminder.Message, [.. attachments]);
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error while handling reminder [{file}]; \n error: {error}", filePath, ex.Message);
                    File.Move(filePath, filePath + ".error");
                }
            }
        }
    }

    private void ValidateReminder(Reminder reminder, out SocketGuild? targetGuild)
    {
        // check if bot still exists in guild, otherwise return
        targetGuild = _client.Guilds.Where(x => x.Id == reminder.GuildId).FirstOrDefault();

        if (targetGuild is null)
        {
            _logger.LogError("Reminder[{name}] triggered for a guild [{guild}] that does not exist anymore", reminder.TriggerDate, reminder.GuildId);
            throw new Exception("Reminder triggered for a guild that does not exist anymore");
        }

        if (targetGuild.Channels.Any(x => x.Id == reminder.ChannelId) == false)
        {
            _logger.LogError("Reminder[{name}] triggered for a guild [{guild}] chanel [{channel}]that does not exist anymore", reminder.TriggerDate, reminder.GuildId, reminder.ChannelId);
            throw new Exception("Reminder triggered for a guild chanel that does not exist anymore");
        }
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
