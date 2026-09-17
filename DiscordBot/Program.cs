global using Discord;
global using Discord.Interactions;
global using Discord.WebSocket;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging;
using DiscordBot.Modules.AnimeBirthdays;
using DiscordBot.Modules.AnimeFeed;
using DiscordBot.Modules.GuildConfig;
using DiscordBot.Modules.GuildLogging;
using DiscordBot.Modules.Reminder;
using DiscordBot.Modules.Tarot;
using DiscordBot.Modules.Twitter;
using DiscordBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = new HostApplicationBuilder(args);

builder.Configuration.AddEnvironmentVariables("DNetTemplate_");

var loggerConfig = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File($"logs/log-{DateTime.Now:yy.MM.dd_HH.mm}.log")
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(loggerConfig, dispose: true);

// Core clients
builder.Services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
    FormatUsersInBidirectionalUnicode = false,
    // Add GatewayIntents.GuildMembers to the GatewayIntents and change this to true if you want to download all users on startup
    AlwaysDownloadUsers = false,
    LogGatewayIntentWarnings = false,
    LogLevel = LogSeverity.Info
}));

// Interaction system
builder.Services.AddSingleton<InteractionService>(sp => new InteractionService(sp.GetRequiredService<DiscordSocketClient>(), new InteractionServiceConfig()
{
    LogLevel = LogSeverity.Info
}));
builder.Services.AddSingleton<InteractionHandler>();

// Core services
builder.Services.AddSingleton<GuildConfigService>();
builder.Services.AddSingleton<TimerService>();
builder.Services.AddSingleton<DiscordChatService>();

// Logging / utilities
builder.Services.AddSingleton<GuildLoggingService>();
builder.Services.AddSingleton<BooruService>();

// Feature services (register as singletons; they can register timer jobs in their constructors)
builder.Services.AddSingleton<TarotService>();
builder.Services.AddSingleton<AnimeFeedService>();
builder.Services.AddSingleton<TwitterService>();
builder.Services.AddSingleton<AnimeListService>();
builder.Services.AddSingleton<BirthdayAnimeService>();

// Hosted services
builder.Services.AddHostedService<DiscordBotService>();
builder.Services.AddHostedService<ReminderService>();
var app = builder.Build();


await app.RunAsync();