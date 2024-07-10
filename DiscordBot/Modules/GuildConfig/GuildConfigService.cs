using DiscordBot.Modules.GuildConfig.Models;

namespace DiscordBot.Modules.GuildConfig
{
    public class GuildConfigService : ServiceWithJsonData<GuildConfigRecord>
    {
        public List<Func<ComponentBuilder, SocketInteractionContext, ComponentBuilder>> Components { get; set; } = [];

        protected override string ModulePath => "!config";

        protected override string ModuleJson => "guildConfig";

        public List<GuildConfigRecord> GetGuildConfig(ulong guildId)
            => GetGuildData(guildId);


        public void SaveConfig(ulong guildId, GuildConfigRecord configRecord)
        {
            var configRecords = GetGuildData(guildId);
            SynchronizeJson();
            
        }
    }
}