using Newtonsoft.Json;

namespace IloApi.Reminder;

public static class ReminderHandler
{
    private static readonly string _dataPath = Path.Combine("..", "data");
    private static readonly string _inputPath = Path.Combine("..", "input");
    public static void HandleReminder(ReminderModel reminder)
    {
        if (!Directory.Exists(_dataPath))
            throw new Exception("Ilo-chan has no guilds saved!");

        var registeredGuilds = Directory.GetDirectories(_dataPath).Select(x => x.Split(['/', '\\']).Last()) ?? [];

        if (registeredGuilds.Contains(reminder.GuildId.ToString()) == false)
            throw new Exception("Ilo-chan doesnt know that guild! If she's on that guild try to execute some commands with her first!");

        var messageCount = Directory.GetFiles(_inputPath, "message*.json").Length;

        var messageFilename = $"message{(messageCount > 0 ? messageCount + 1 : string.Empty)}.json";

        List<string> downloadedAttachments = [];

        foreach (var attachment in reminder.Attachments)
        {
            var attachmentLocalPath = DownloadAttachment(attachment).Result;
            downloadedAttachments.Add(attachmentLocalPath);
        }

        reminder.Attachments = downloadedAttachments;
        string messageJson = JsonConvert.SerializeObject(reminder, Formatting.Indented);

        File.WriteAllText(Path.Combine(_inputPath, messageFilename), messageJson);
    }

    private static async Task<string> DownloadAttachment(string attachmentUrl)
    {
        HttpClient client = new();

        string attachmentName = attachmentUrl
            .Split(['/', '\\'])
            .Last()
            .Split('?')
            .First();

        if (File.Exists(Path.Combine(_inputPath, attachmentName)))
        {
            var extension = attachmentName.Split('.').Last();
            attachmentName = Guid.NewGuid().ToString() + (String.IsNullOrEmpty(extension) ? string.Empty : "." + extension);
        }
        string imagePath = Path.Combine(_inputPath, attachmentName);

        var resp = await client.GetAsync(attachmentUrl);
        var imageBytes = await resp.Content.ReadAsByteArrayAsync();
        if (imageBytes.Length < 50)
            throw new Exception($"failed to download file from {attachmentUrl}!");

        await File.WriteAllBytesAsync(imagePath, imageBytes);

        return attachmentName;
    }
}
