using DiscordBot.Modules.GuildConfig.Models;
using System.Text;
using System.Text.Json;

namespace DiscordBot.Modules.GuildConfig
{
    public class GuildConfigService : ServiceWithJsonData<GuildConfigRecord>
    {
        public List<Func<ComponentBuilder, SocketInteractionContext, ComponentBuilder>> Components { get; private set; } = [];

        protected override string ModulePath => "!config";

        protected override string ModuleJson => "guildConfig.json";

        private const string GuildToken = nameof(GuildToken);

        private List<GuildConfigRecord> GetGuildConfig(ulong guildId)
            => GetGuildData(guildId);

        public void AddConfigComponent(Func<ComponentBuilder, SocketInteractionContext, ComponentBuilder> builder)
        {
            if (!Components.Contains(builder))
                Components.Add(builder);
        }

        public string? GetGuildConfigValue(ulong guildId, string configId)
        {
            var guildConfig = GetGuildConfig(guildId)
                                .Where(x => x.Key == configId)
                                .FirstOrDefault()
                                ?? null;

            if (guildConfig is null)
                return null;

            return (string)guildConfig.Value;
        }

        /// <summary>
        /// returns 0 when not found
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="configId"></param>
        /// <returns></returns>
        public ulong GetGuildConfigValueAsUlong(ulong guildId, string configId)
            => ulong.Parse(GetGuildConfigValue(guildId, configId) ?? "0");


        public Dictionary<ulong, List<GuildConfigRecord>> GetAllGuildsData()
        => moduleData;

        public void SaveConfig(ulong guildId, GuildConfigRecord configRecord)
        {
            if (!moduleData.ContainsKey(guildId))
                moduleData.Add(guildId, []);

            var guildRecord = moduleData[guildId].Where(x => x.Key == configRecord.Key).FirstOrDefault();
            if (guildRecord is not null)
            {
                moduleData[guildId].Remove(guildRecord);
                
            }
            moduleData[guildId].Add(configRecord);

            SynchronizeJson();
        }

        public void RemoveGuildConfig(ulong guildId)
        {
            moduleData.Remove(guildId);
            SynchronizeJson();
            Directory.Delete(Path.Combine(_dataRoot, guildId.ToString()), true);
        }

        public string CreateAccessToken(SocketGuild guild)
        {
            string token = string.Empty;
            
            var data = new
            {
                GuildId = guild.Id,
                GuildName = guild.Name,
                Channels = guild.Channels
                    .Select(c => new KeyValuePair<ulong,string>(c.Id, c.Name ))
                    .ToArray(),
                Timestamp = DateTime.UtcNow.Ticks
            };
            
            string json = JsonSerializer.Serialize(data);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            // Use a fixed key for XOR obfuscation
            byte[] key = Encoding.UTF8.GetBytes("ilo123");
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ key[i % key.Length]);
            }

            token = Convert.ToBase64String(bytes);
            SaveConfig(guild.Id, new GuildConfigRecord(GuildToken, token));
            return token;
        }
    }
}