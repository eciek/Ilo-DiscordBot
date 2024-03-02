namespace DiscordBot.Modules.AnimeFeed.Models;

public class Anime
{
    public string? Name { get; set; }
    public string? Episode { get; set; }
    public string? Url { get; set; }
    public int? Id { get; set; }
    public List<ulong>? Subscribers { get; set; }

    public bool Equals(Anime? other)
        {
            if (other == null) return false;

            return Name == other.Name;
        }

    public override string ToString()
    {
        return $"{Name} - {Episode}";
    }
}
