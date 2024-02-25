using Discord.Commands;
namespace DiscordBot.Modules.AnimeFeed;

public class AnimeFeedModule(
    AnimeFeedService animeFeedService) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly AnimeFeedService _animeFeedService = animeFeedService;

    //[SlashCommand("anime-dodaj", "Dodaj do listy wołania na nowy odcinek anime")]
    //public async Task AnimeAdd([Name("Nazwa Anime")]string animeName)
    //{
    //    //Na chwilę obecną zwraca najnowszą bajkę dodaną na Nyaa xD
    //    CancellationToken ct = new();
    //    var animeList = await _animeFeedService.GetAnimeFromRSSAsync(ct);
    //    await RespondAsync(animeList[0].Name).ConfigureAwait(false);
    //}

    //[Discord.Commands.RequireUserPermission(GuildPermission.Administrator)]
    //[SlashCommand("anime-usuń", "Usuń z listy wołania na nowy odcinek anime")]
    //public async Task AnimeDelete([Name("Nazwa Anime")]string animeName)
    //{
    //    throw new NotImplementedException();
    //}

}
