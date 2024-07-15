namespace DiscordBot.Modules.AnimeBirthdays.Models;

public class BirthdayAnimeCharacter
{
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Series { get; set; }

    public override string ToString()
    {
        if (Surname == null)
            return $"{Name} ({Series})";
        else
            return $"{Name} {Surname} ({Series})";
    }

    private string GetSeriesSlug()
        => (Series ?? string.Empty).Trim().Replace(" ", "_");


    public string ToNameSurnameBooruSlug()
        => $"{Name}_{Surname}".Trim().Replace(" ", "_") + $" ({GetSeriesSlug()})";

    public string ToSurnameNameBooruSlug()
        => $"{Surname}_{Name}".Trim().Replace(" ", "_") + $" ({GetSeriesSlug()})";
}
