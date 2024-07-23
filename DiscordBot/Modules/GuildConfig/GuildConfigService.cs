using DiscordBot.Modules.GuildConfig.Models;

namespace DiscordBot.Modules.GuildConfig
{
    public class GuildConfigService : ServiceWithJsonData<GuildConfigRecord>
    {
        public List<Func<ComponentBuilder, SocketInteractionContext, ComponentBuilder>> Components { get; private set; } = [];

        protected override string ModulePath => "!config";

        protected override string ModuleJson => "guildConfig.json";

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
            if (guildRecord is null)
            {
                moduleData[guildId].Add(configRecord);
            }

            SynchronizeJson();
        }
    }
}