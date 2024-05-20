using Newtonsoft.Json;

namespace DiscordBot.Modules.RaiderIO.Models;

public partial class CharacterRIO
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("race")]
    public string Race { get; set; }

    [JsonProperty("class")]
    public string Class { get; set; }

    [JsonProperty("active_spec_name")]
    public string ActiveSpecName { get; set; }

    [JsonProperty("active_spec_role")]
    public string ActiveSpecRole { get; set; }

    [JsonProperty("gender")]
    public string Gender { get; set; }

    [JsonProperty("faction")]
    public string Faction { get; set; }

    [JsonProperty("achievement_points")]
    public long AchievementPoints { get; set; }

    [JsonProperty("honorable_kills")]
    public long HonorableKills { get; set; }

    [JsonProperty("thumbnail_url")]
    public Uri ThumbnailUrl { get; set; }

    [JsonProperty("region")]
    public string Region { get; set; }

    [JsonProperty("realm")]
    public string Realm { get; set; }

    [JsonProperty("last_crawled_at")]
    public DateTimeOffset LastCrawledAt { get; set; }

    [JsonProperty("profile_url")]
    public Uri ProfileUrl { get; set; }

    [JsonProperty("profile_banner")]
    public string ProfileBanner { get; set; }

    [JsonProperty("mythic_plus_scores_by_season")]
    public MythicPlusScoresBySeason[] MythicPlusScoresBySeason { get; set; }

    [JsonProperty("gear")]
    public Gear Gear { get; set; }

    [JsonProperty("raid_progression")]
    public RaidProgression RaidProgression { get; set; }

    [JsonProperty("guild")]
    public Guild Guild { get; set; }
}

public partial class Gear
{
    [JsonProperty("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonProperty("item_level_equipped")]
    public double ItemLevelEquipped { get; set; }

    [JsonProperty("artifact_traits")]
    public long ArtifactTraits { get; set; }

    [JsonProperty("corruption")]
    public GearCorruption Corruption { get; set; }

    [JsonProperty("items")]
    public Dictionary<string, Item> Items { get; set; }
}

public partial class GearCorruption
{
    [JsonProperty("added")]
    public long Added { get; set; }

    [JsonProperty("resisted")]
    public long Resisted { get; set; }

    [JsonProperty("total")]
    public long Total { get; set; }

    [JsonProperty("cloakRank")]
    public long CloakRank { get; set; }

    [JsonProperty("spells")]
    public object[] Spells { get; set; }
}

public partial class Item
{
    [JsonProperty("item_id")]
    public long ItemId { get; set; }

    [JsonProperty("item_level")]
    public long ItemLevel { get; set; }

    [JsonProperty("icon")]
    public string Icon { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("item_quality")]
    public long ItemQuality { get; set; }

    [JsonProperty("is_legendary")]
    public bool IsLegendary { get; set; }

    [JsonProperty("is_azerite_armor")]
    public bool IsAzeriteArmor { get; set; }

    [JsonProperty("azerite_powers")]
    public AzeritePower[] AzeritePowers { get; set; }

    [JsonProperty("corruption")]
    public ItemCorruption Corruption { get; set; }

    [JsonProperty("domination_shards")]
    public object[] DominationShards { get; set; }

    [JsonProperty("gems")]
    public long[] Gems { get; set; }

    [JsonProperty("bonuses")]
    public long[] Bonuses { get; set; }

    [JsonProperty("tier", NullValueHandling = NullValueHandling.Ignore)]
    public string Tier { get; set; }
}

public partial class AzeritePower
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("spell")]
    public Spell Spell { get; set; }

    [JsonProperty("tier")]
    public long Tier { get; set; }
}

public partial class Spell
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("school")]
    public long School { get; set; }

    [JsonProperty("icon")]
    public string Icon { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("rank")]
    public object Rank { get; set; }
}

public partial class ItemCorruption
{
    [JsonProperty("added")]
    public long Added { get; set; }

    [JsonProperty("resisted")]
    public long Resisted { get; set; }

    [JsonProperty("total")]
    public long Total { get; set; }
}

public partial class Guild
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("realm")]
    public string Realm { get; set; }
}

public partial class MythicPlusScoresBySeason
{
    [JsonProperty("season")]
    public string Season { get; set; }

    [JsonProperty("scores")]
    public Scores Scores { get; set; }

    [JsonProperty("segments")]
    public Segments Segments { get; set; }
}

public partial class Scores
{
    [JsonProperty("all")]
    public long All { get; set; }

    [JsonProperty("dps")]
    public long Dps { get; set; }

    [JsonProperty("healer")]
    public long Healer { get; set; }

    [JsonProperty("tank")]
    public long Tank { get; set; }

    [JsonProperty("spec_0")]
    public long Spec0 { get; set; }

    [JsonProperty("spec_1")]
    public long Spec1 { get; set; }

    [JsonProperty("spec_2")]
    public long Spec2 { get; set; }

    [JsonProperty("spec_3")]
    public long Spec3 { get; set; }
}

public partial class Segments
{
    [JsonProperty("all")]
    public All All { get; set; }

    [JsonProperty("dps")]
    public All Dps { get; set; }

    [JsonProperty("healer")]
    public All Healer { get; set; }

    [JsonProperty("tank")]
    public All Tank { get; set; }

    [JsonProperty("spec_0")]
    public All Spec0 { get; set; }

    [JsonProperty("spec_1")]
    public All Spec1 { get; set; }

    [JsonProperty("spec_2")]
    public All Spec2 { get; set; }

    [JsonProperty("spec_3")]
    public All Spec3 { get; set; }
}

public partial class All
{
    [JsonProperty("score")]
    public long Score { get; set; }

    [JsonProperty("color")]
    public string Color { get; set; }
}

public partial class RaidProgression
{
    [JsonProperty("aberrus-the-shadowed-crucible")]
    public RaidOverall AberrusTheShadowedCrucible { get; set; }

    [JsonProperty("amirdrassil-the-dreams-hope")]
    public RaidOverall AmirdrassilTheDreamsHope { get; set; }

    [JsonProperty("awakened-aberrus-the-shadowed-crucible")]
    public RaidOverall AwakenedAberrusTheShadowedCrucible { get; set; }

    [JsonProperty("awakened-amirdrassil-the-dreams-hope")]
    public RaidOverall AwakenedAmirdrassilTheDreamsHope { get; set; }

    [JsonProperty("awakened-vault-of-the-incarnates")]
    public RaidOverall AwakenedVaultOfTheIncarnates { get; set; }

    [JsonProperty("vault-of-the-incarnates")]
    public RaidOverall VaultOfTheIncarnates { get; set; }
}

public partial class RaidOverall
{
    [JsonProperty("summary")]
    public string Summary { get; set; }

    [JsonProperty("total_bosses")]
    public long TotalBosses { get; set; }

    [JsonProperty("normal_bosses_killed")]
    public long NormalBossesKilled { get; set; }

    [JsonProperty("heroic_bosses_killed")]
    public long HeroicBossesKilled { get; set; }

    [JsonProperty("mythic_bosses_killed")]
    public long MythicBossesKilled { get; set; }
}
