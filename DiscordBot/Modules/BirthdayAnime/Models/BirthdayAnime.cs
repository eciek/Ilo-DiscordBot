namespace DiscordBot.Modules.AnimeBirthdays.Models;

public class BirthdayAnime
{
    public string? Date { get; set; }
    public List<BirthdayAnimeCharacter> Characters { get; set; } = [];
}
