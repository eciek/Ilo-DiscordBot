using DiscordBot.Helpers;
using Newtonsoft.Json;

namespace DiscordBot.Modules;

public abstract class ServiceWithJsonData<T>
{
    protected const string _dataRoot = "data";
    protected abstract string ModulePath { get; }
    protected abstract string ModuleJson { get; }

    protected readonly Dictionary<ulong, List<T>> moduleData = [];

    protected ServiceWithJsonData()
    {
        string[] registeredGuilds;
        moduleData = [];

        try
        {
            registeredGuilds = Directory.GetDirectories(_dataRoot);
        }
        catch (DirectoryNotFoundException)
        {
            Directory.CreateDirectory(_dataRoot);
            registeredGuilds = Directory.GetDirectories(_dataRoot);
        }

        foreach (var registeredGuild in registeredGuilds)
        {
            moduleData.Add(ulong.Parse(registeredGuild.TrimLetters()), []);
        }
        LoadModuleDataFromJson();
    }

    protected List<T> GetGuildData(ulong guildId)
    {
        if (!moduleData.TryGetValue(guildId, out _))
        {
            moduleData.Add(guildId, []);
        }
        return moduleData[guildId];
    }

    public List<ulong> GetAllGuilds()
    => [.. moduleData.Keys];

    private async void LoadModuleDataFromJson()
    {
        Console.WriteLine($"Loading {ModulePath}...");
        foreach (var guildsData in moduleData)
        {
            var filePath = Path.Combine(_dataRoot, guildsData.Key.ToString(), ModulePath, ModuleJson);
            string json;

            if (File.Exists(filePath) == false)
                return;

            using (var stream = new StreamReader(filePath))
            {
                try
                {
                    json = await stream.ReadToEndAsync();
                }
                catch (FileNotFoundException)
                {
                    json = string.Empty;
                }
            }

            if (string.IsNullOrEmpty(json))
            {
                moduleData[guildsData.Key] ??= [];
                continue;
            }

            moduleData[guildsData.Key] = JsonConvert.DeserializeObject<List<T>>(json)!;
        }
    }
    protected void SynchronizeJson()
    {
        Console.WriteLine($"Saving {ModuleJson}.json...");
        foreach (var guildData in moduleData)
        {
            var path = Path.Combine(_dataRoot, guildData.Key.ToString(), ModulePath);
            var filePath = Path.Combine(path, ModuleJson);
            string json = JsonConvert.SerializeObject(guildData.Value, Formatting.Indented);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            using var stream = new StreamWriter(new FileStream(filePath, FileMode.Create));
            stream.Write(json);
        }
    }


}