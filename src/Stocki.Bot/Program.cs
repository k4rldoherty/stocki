using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stocki.Application.Interfaces;
using Stocki.Application.Queries.Overview;
using Stocki.Application.Queries.Price;
using Stocki.Bot.Chat;
using Stocki.Bot.Commands;
using Stocki.Bot.Setup;
using Stocki.Infrastructure.Clients;
using Stocki.Shared.Config;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureAppConfiguration(cfg => cfg.AddEnvironmentVariables());
builder.ConfigureServices(
    (context, services) =>
    {
        services.AddMemoryCache();
        services.Configure<AlphaVantageSettings>(context.Configuration.GetSection("AlphaVantage"));
        services.Configure<FinnhubClientSettings>(context.Configuration.GetSection("Finnhub"));
        services.Configure<DiscordSettings>(context.Configuration.GetSection("Discord"));
        //
        // --- Alpha Client
        //
        services.AddHttpClient<IAlphaVantageClient, AlphaVantageClient>(client =>
        {
            var alphaVantageSettings = context
                .Configuration.GetSection("AlphaVantage")
                .Get<AlphaVantageSettings>();
            if (alphaVantageSettings != null && !string.IsNullOrEmpty(alphaVantageSettings.BaseUrl))
            {
                client.BaseAddress = new Uri(alphaVantageSettings.BaseUrl);
            }
        });
        //
        // --- Finnhub Client
        //
        services.AddHttpClient<IFinnhubClient, FinnhubClient>(client =>
        {
            var finnhubClientSettings = context
                .Configuration.GetSection("Finnhub")
                .Get<FinnhubClientSettings>();
            if (
                finnhubClientSettings != null
                && !string.IsNullOrEmpty(finnhubClientSettings.BaseUrl)
            )
            {
                client.BaseAddress = new Uri(finnhubClientSettings.BaseUrl);
            }
        });
        services.AddSingleton(x =>
        {
            var discordSettings = x.GetRequiredService<IOptions<DiscordSettings>>().Value; // Get the settings here
            return new DiscordSocketClient();
        });
        // Interaction Service that handles the execution of commands
        services.AddSingleton(x => new InteractionService(
            x.GetRequiredService<DiscordSocketClient>(),
            new InteractionServiceConfig()
            {
                AutoServiceScopes = true,
                LogLevel = Discord.LogSeverity.Info,
            }
        ));
        // Hosted service for bot startup
        services.AddHostedService<BotStartupService>();
        services.AddSingleton<StockCommands>();
        services.AddSingleton<InputHandlerService>();

        // Keeps Infrastructure decoupled from Bot and Application layers
        // services.AddSingleton<IAlphaVantageClient, AlphaVantageClient>();
        services.AddSingleton<StockOverviewQueryHandler>();
        services.AddSingleton<StockQuoteQueryHandler>();
        // services.AddSingleton<IFinnhubClient, FinnhubClient>();
    }
);
builder.ConfigureLogging(logging => logging.AddConsole());

var app = builder.Build();

await app.RunAsync();
