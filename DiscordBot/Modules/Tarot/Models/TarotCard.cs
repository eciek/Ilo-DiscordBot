namespace DiscordBot.Modules.Tarot.Models
{
    public class TarotCard
    {
        public string? Name { get; set; }
        public string? Quote { get; set; }
        public string? Description { get; set; }


        public static TarotCard AlcoholicCard()
            => new()
            {
                Name = "Alkoholik",
                Quote = "Trzeba piwo jebnąć na śniadanie",
                Description = "Osiągnąłeś w życiu wszystko, teraz możesz odpocząć. Nic lepszego w życiu od alkoholu cie nie spotka, dlatego śmiało możesz odpalić kolejnego browara. To jest najważniejsze, by w łapie zawsze coś było. Dlatego nie przejmuj się hejterami i żyj tak jak lubisz, po swojemu! ",
            };
        

    }
}