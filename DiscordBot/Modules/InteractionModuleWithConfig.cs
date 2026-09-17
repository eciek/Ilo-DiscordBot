using DiscordBot.Helpers;
using Newtonsoft.Json;

namespace DiscordBot.Modules;

public abstract class ServiceWithJsonData<T>
{
    protected const string _dataRoot = "data";
    protected abstract string ModulePath { get; }
    protected abstract string ModuleJson { get; }
    protected readonly Dictionary<ulong, List<T>> moduleData;

    protected ServiceWithJsonData()
    {
        string[] registeredGuilds;
        moduleData = new Dictionary<ulong, List<T>>();

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
            // registeredGuild is a path like "data\<guildId>"; TrimLetters is expected to extract digits
            moduleData.Add(ulong.Parse(registeredGuild.TrimLetters()), new List<T>());
        }
        LoadModuleDataFromJson();
    }

    protected List<T> GetGuildData(ulong guildId)
    {
        if (!moduleData.TryGetValue(guildId, out var list))
        {
            list = new List<T>();
            moduleData.Add(guildId, list);
        }
        return list;
    }

    public List<ulong> GetAllGuilds()
        => moduleData.Keys.ToList();

    private void LoadModuleDataFromJson()
    {
        Console.WriteLine($"Loading {ModulePath}...");
        foreach (var guildsData in moduleData.ToList())
        {
            var filePath = Path.Combine(_dataRoot, guildsData.Key.ToString(), ModulePath, ModuleJson);
            string json = string.Empty;

            if (!File.Exists(filePath))
            {
                moduleData[guildsData.Key] = new List<T>();
                continue;
            }

            using (var stream = new StreamReader(filePath))
            {
                try
                {
                    json = stream.ReadToEnd();
                }
                catch (FileNotFoundException)
                {
                    json = string.Empty;
                }
            }

            if (string.IsNullOrEmpty(json))
            {
                moduleData[guildsData.Key] = new List<T>();
                continue;
            }

            moduleData[guildsData.Key] = JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
        }
    }
    protected void SynchronizeJson()
    {
        Console.WriteLine($"Saving {ModuleJson}...");
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