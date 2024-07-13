using Discord.Commands;
using DiscordBot.Models;
using DiscordBot.Modules.AnimeFeed.Models;
using DiscordBot.Modules.GuildConfig;
using DiscordBot.Services;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DiscordBot.Modules.AnimeFeed;

public class AnimeFeedModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly AnimeFeedService _animeFeedService;
    private readonly AnimeListService _animeListService;
    private readonly TimerService _timerService;
    private const int _jobInterval = 1;

    public AnimeFeedModule(
        AnimeFeedService animeFeedService,
        AnimeListService animeListService,
        TimerService timerService)
    {
        _animeFeedService = animeFeedService;
        _animeListService = animeListService;
        _timerService = timerService;

        TimerJob animeFeedJob = new(nameof(animeFeedJob), _jobInterval, TimerJobTiming.NowAndRepeatOnInterval, Update);
        _timerService.RegisterJob(animeFeedJob);
    }

    [SlashCommand("anime-dodaj", "Dodaj do listy wołania na nowy odcinek anime")]
    public async Task AnimeAdd([Name("Nazwa Anime")][MinLength(4)] string anime)
    {
        Anime foundAnime;
        try
        {
            foundAnime = await _animeFeedService.MatchAnime(anime);
        }
        catch (Exception ex)
        {
            await RespondAsync(ex.Message, ephemeral: true);
            return;
        }

        _animeListService.AddAnimeSubscriber(Context.Guild.Id, Context.User.Id, foundAnime);

        await RespondAsync($"Dodałam Cię do listy {foundAnime.Name}!", ephemeral: true);
    }

    [SlashCommand("anime-usuń", "Usuń z listy wołania na nowy odcinek anime")]
    public async Task AnimeDelete([Optional][Name("Nazwa Anime")][MinLength(4)] string anime)
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
                await RespondAsync(errmsg, ephemeral: true);
                return;
            }
            // This exception covers GetUserAnimeList messages
            catch (Exception ex)
            {
                await RespondAsync(ex.Message, ephemeral: true);
                return;
            }
        }

        _animeListService.RemoveAnimeSubscriber(Context.Guild.Id, Context.User.Id, foundAnime);
        await RespondAsync($"Już nie obserwujesz {foundAnime.Name}. \n" +
            $"Co Ci się nie spodobało w tym anime?", ephemeral: true);
        return;
    }

    private async void Update()
    {
        await _animeFeedService.UpdateAnimeFeedAsync();
        var animeList = _animeFeedService.GetAnimeList();

        await _animeListService.UpdateAnimeList(animeList);
    }

    [ComponentInteraction("animeFeed")]
    public static void AnimeFeed()
    {
        
    }
}
