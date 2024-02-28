using Discord.Commands;
using DiscordBot.Modules.AnimeFeed.Models;
using System.Runtime.InteropServices;

namespace DiscordBot.Modules.AnimeFeed;

public class AnimeFeedModule(
    AnimeFeedService animeFeedService,
    AnimeListService animeListService) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly AnimeFeedService _animeFeedService = animeFeedService;
    private readonly AnimeListService _animeListService = animeListService;

    [SlashCommand("anime-dodaj", "Dodaj do listy wołania na nowy odcinek anime")]
    public async Task AnimeAdd([Name("Nazwa Anime")] [MinLength(4)]string anime)
    {
        Anime foundAnime;
        try
        {
            foundAnime = await _animeFeedService.MatchAnime(anime);
        }
        catch (Exception ex) 
        {
            await RespondAsync(ex.Message);
            return;
        }

        _animeListService.AddAnimeSubscriber(Context.Guild.Id, Context.User.Id, foundAnime);

        await RespondAsync($"Dałam Cię do listy {foundAnime.Name}!");
    }

    [Discord.Commands.RequireUserPermission(GuildPermission.Administrator)]
    [SlashCommand("anime-usuń", "Usuń z listy wołania na nowy odcinek anime")]
    public async Task AnimeDelete([Optional] [Name("Nazwa Anime")] [MinLength(4)] string anime)
    {
        Anime foundAnime;
        try
        {
            foundAnime = await _animeFeedService.MatchAnime(anime);
        }
        catch (Exception)
        {
            try
            {
                var userAnimeList = _animeListService.GetUserAnimeList(Context.Guild.Id, Context.User.Id);

                string errmsg = "O to anime które obserwujesz: \n"
                    + string.Join(",\n", userAnimeList)
                    + ".\n\n" + "Wybierz właściwą nazwę!";
                await RespondAsync(errmsg);
                return;
            }
            // This exception covers GetUserAnimeList messages
            catch (Exception ex)
            {
                await RespondAsync(ex.Message);
                return;
            }
        }

        _animeListService.RemoveAnimeSubscriber(Context.Guild.Id, Context.User.Id, foundAnime);
        await RespondAsync($"Już nie obserwujesz {foundAnime.Name}. \n" +
            $"Co Ci się nie spodobało w tym anime?");
        return;
    }
}
