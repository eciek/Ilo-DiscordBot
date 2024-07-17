using DiscordBot.Modules.GuildConfig.Models;

namespace DiscordBot.Modules.GuildConfig
{
    public class GuildConfigService : ServiceWithJsonData<GuildConfigRecord>
    {
        public List<Func<ComponentBuilder, SocketInteractionContext, ComponentBuilder>> Components { get; set; } = [];

        protected override string ModulePath => "!config";

        protected override string ModuleJson => "guildConfig";

        private List<GuildConfigRecord> GetGuildConfig(ulong guildId)
            => GetGuildData(guildId);

        public object? GetGuildConfigValue(ulong guildId, string configId)
        {
            var guildConfig = GetGuildConfig(guildId)
                                .Where(x => x.Key == configId)
                                .FirstOrDefault()
                                ?? null;

            if (guildConfig is null)
                return null;

            return guildConfig.Value;            
        }

        public Dictionary<ulong, List<GuildConfigRecord>> GetAllGuildsData()
        => moduleData;
        
        public void SaveConfig(ulong guildId, GuildConfigRecord configRecord)
        {
            if (!moduleData.ContainsKey(guildId))
                moduleData.Add(guildId, []);

            moduleData[guildId].Add(configRecord);
            SynchronizeJson();
        }
    }
}