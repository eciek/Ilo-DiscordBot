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

    public string ToNameSurnameBooruSlug()
        => $"{Name}_{Surname}".Trim().Replace(" ", "_") + $" ({Series})";

    public string ToSurnameNameBooruSlug()
        => $"{Surname}_{Name}".Trim().Replace(" ", "_") + $" ({Series})";
}
