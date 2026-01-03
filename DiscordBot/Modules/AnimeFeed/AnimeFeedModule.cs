using Discord.Commands;
using DiscordBot.Models;
using DiscordBot.Modules.AnimeFeed.Models;
using DiscordBot.Modules.GuildLogging;
using DiscordBot.Services;

namespace DiscordBot.Modules.AnimeFeed;

public class AnimeFeedModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly AnimeFeedService _animeFeedService;
    private readonly AnimeListService _animeListService;
    private readonly TimerService _timerService;
    private readonly GuildLoggingService _guildLogging;
    private readonly ILogger<AnimeFeedModule> _logger;
    private const int _jobInterval = 1;

    public AnimeFeedModule(
        AnimeFeedService animeFeedService,
        AnimeListService animeListService,
        TimerService timerService,
        GuildLoggingService guildLogging,
        ILogger<AnimeFeedModule> logger
        )
    {
        _animeFeedService = animeFeedService;
        _animeListService = animeListService;
        _timerService = timerService;
        _guildLogging = guildLogging;
        _logger = logger;

        TimerJob animeFeedJob = new(nameof(animeFeedJob), _jobInterval, TimerJobTiming.NowAndRepeatOnInterval, Update);
        _timerService.RegisterJob(animeFeedJob);
    }

    [SlashCommand("anime-dodaj", "Dodaj do listy wołania na nowy odcinek anime")]
    public async Task AnimeAdd([Name("Nazwa Anime")][MinLength(4)] string anime, string note = "", string? alternateName = "")
    {
        Anime foundAnime;
        try
        {
            foundAnime = _animeFeedService.MatchAnime(anime);
            if (!string.IsNullOrEmpty(alternateName))
                foundAnime.BooruName = alternateName;
        }
        catch (InvalidOperationException ex)
        {
            string msg = $"Znalazłam kilka pasujących anime do twojego opisu: \n" +
                $"{ex.Message}" +
                " Które Cie interesuje?";

            _logger.LogInformation("AnimeFeed.AnimeAdd: Query {query} gave out multiple results: \n {anime}", anime, ex.Message);
            await RespondAsync(msg, ephemeral: true);
            return;
        }
        catch (ArgumentNullException)
        {
            string msg = "Nie wiem o które anime Ci chodzi. Czy pojawił się już chociaż jeden odcinek?";

            _logger.LogInformation("AnimeFeed.AnimeAdd: Query {query} gave out no results.", anime);
            await RespondAsync(msg, ephemeral: true);
            return;
        }
        catch (Exception ex)
        {

            _logger.LogError("AnimeFeed.AnimeAdd: Unhandled exception:\n{ex}", ex.Message);
            _guildLogging.GuildLog(Context.Guild.Id, string.Format($"AnimeFeed.AnimeAdd: Unhandled exception:\n {ex.Message}"));

            await RespondAsync("Przepraszam, coś się popsuło (˃̣̣̥∩˂̣̣̥)", ephemeral: true);
            return;
        }

        try
        {
            _animeListService.AddAnimeSubscriber(Context.Guild.Id, Context.User.Id, foundAnime, note);
        }
        catch (Exception ex)
        {
            _logger.LogError("AnimeListService.AddAnimeSubscriber: Unhandled exception:\n{ex}", ex.Message);
            _guildLogging.GuildLog(Context.Guild.Id, string.Format($"AnimeListService.AddAnimeSubscriber: Unhandled exception:\n {ex.Message}"));

            await RespondAsync("Przepraszam, coś się popsuło (˃̣̣̥∩˂̣̣̥)", ephemeral: true);
            return;
        }

        await RespondAsync($"Dodałam Cię do listy {foundAnime.Name}!", ephemeral: true);
    }

    [SlashCommand("anime-usuń", "Usuń z listy wołania na nowy odcinek anime")]
    public async Task AnimeDelete([Name("Nazwa Anime")][MinLength(4)] string anime = "")
    {
        if (!String.IsNullOrEmpty(anime))
        {
            Anime foundAnime;
            try
            {
                foundAnime = _animeFeedService.MatchAnime(anime);
            }
            catch (InvalidOperationException ex)
            {
                string msg = "Oto anime które obserwujesz: \n" +
                    $"{ex.Message}\n" +
                    "Wybierz właściwą nazwę!";
                await RespondAsync(msg, ephemeral: true);
                return;
            }
            catch (KeyNotFoundException)
            {
                string msg = "Nie znalazłam żadnego pasującego anime, które obserwujesz.\n" +
                    "Użyj /anime-usuń bez podawania nazwy by usunąć wszystkie obserwujące anime!";

                await RespondAsync(msg, ephemeral: true);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError("AnimeFeed.AnimeDelete: Unhandled exception:\n{ex}", ex.Message);
                _guildLogging.GuildLog(Context.Guild.Id, string.Format($"AnimeFeed.AnimeDelete: Unhandled exception:\n {ex.Message}"));

                await RespondAsync("Przepraszam, coś się popsuło (˃̣̣̥∩˂̣̣̥)", ephemeral: true);
                return;
            }

            _animeListService.RemoveAnimeSubscriber(Context.Guild.Id, Context.User.Id, foundAnime);
            await RespondAsync($"Już nie obserwujesz {foundAnime.Name}. \n" +
                $"Co Ci się nie spodobało w tym anime?", ephemeral: true);
        }
        // Remove user from all anime lists
        else
        {
            _animeListService.RemoveAnimeSubscriber(Context.Guild.Id, Context.User.Id);
            await RespondAsync($"Już nie obserwujesz żadnego anime!", ephemeral: true);
        }
        return;
    }

    [Discord.Interactions.RequireOwner]
    [SlashCommand("anime-altname", "Ustaw Alternatywną nazwę dla anime")]
    public async Task SetAlternateName([Name("Nazwa Anime")][MinLength(4)] string animeName, [Name("Alternatywna nazwa")][MinLength(4)] string alternateName)
    {
        Anime foundAnime;
        try
        {
            foundAnime = _animeFeedService.MatchAnime(animeName);
        }
        catch (InvalidOperationException ex)
        {
            string msg = $"Znalazłam kilka pasujących anime do twojego opisu: \n" +
                $"{ex.Message}" +
                " Które Cie interesuje?";

            _logger.LogInformation("AnimeFeed.SetAlternateName: Query {query} gave out multiple results: \n {anime}", animeName, ex.Message);
            await RespondAsync(msg, ephemeral: true);
            return;
        }
        catch (ArgumentNullException)
        {
            string msg = "Nie wiem o które anime Ci chodzi. Czy pojawił się już chociaż jeden odcinek?";

            _logger.LogInformation("AnimeFeed.SetAlternateName: Query {query} gave out no results.", animeName);
            await RespondAsync(msg, ephemeral: true);
            return;
        }
        catch (Exception ex)
        {

            _logger.LogError("AnimeFeed.SetAlternateName: Unhandled exception:\n{ex}", ex.Message);
            _guildLogging.GuildLog(Context.Guild.Id, string.Format($"AnimeFeed.AnimeAdd: Unhandled exception:\n {ex.Message}"));

            await RespondAsync("Przepraszam, coś się popsuło (˃̣̣̥∩˂̣̣̥)", ephemeral: true);
            return;
        }

        _animeListService.SetAlternateNameForAnime(foundAnime, alternateName);
        await RespondAsync($"Ustawiłam nazwę alternatywną dla \"{foundAnime.Name} \" na \"{alternateName}\" ^.^", ephemeral: true);

    }

    private async void Update()
    {
        IEnumerable<Anime> animeList = [];
        try
        {
            await _animeFeedService.UpdateAnimeFeedAsync();
            animeList = _animeFeedService.GetAnimeList();
        }
        catch (Exception ex)
        {
            _logger.LogCritical("AnimeFeed.Update: Unhandled exception:\n{ex}", ex.Message);
        }

        await _animeListService.UpdateAnimeList(animeList);
    }

    [ComponentInteraction("animeFeed")]
    public static void AnimeFeed()
    {

    }
}
