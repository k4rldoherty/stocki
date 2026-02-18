using Stocki.Bot.Chat;
using Stocki.Bot.Setup;
using Stocki.Domain.Interfaces;
using Stocki.PriceMonitoringService.Interfaces;
using Stocki.Infrastructure.Persistance.Repositories;
using Stocki.NotificationService;
using Stocki.PriceMonitor.Services;
using Stocki.Shared.Config;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureWebHostDefaults(webBuilder =>
{
    HealthChecker.SetupHealthEndpoint(webBuilder);
});

builder.ConfigureServices(
    (context, services) =>
    {
        services.AddMemoryCache();
        services.Configure<AlphaVantageSettings>(context.Configuration.GetSection("AlphaVantage"));
        services.Configure<FinnhubClientSettings>(context.Configuration.GetSection("Finnhub"));
        services.Configure<DiscordSettings>(context.Configuration.GetSection("Discord"));
        services.Configure<FinnhubWebsocketsSettings>(
            context.Configuration.GetSection("FinnhubWebsockets")
        );
        Database.SetupDatabase(context, services);
        MediatRSetup.SetupMediatR(context, services);
        APIClients.SetupAPIClients(context, services);
        DiscordClient.SetupDiscordClient(context, services);
        services.AddHostedService<BotStartupService>();
        services.AddHostedService<PriceMonitoringService>();
        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            return config.GetSection("FinnhubWebsockets")
                         .Get<FinnhubWebsocketsSettings>()
                ?? new FinnhubWebsocketsSettings();
        });
        services.AddSingleton<InputHandlerService>();
        services.AddSingleton<PriceMovedBeyondThresholdHandler>();
        services.AddSingleton<FinnhubWSManager>();
        services.AddSingleton<PriceChecker>();
        services.AddScoped<IStockPriceSubscriptionRepository, StockPriceSubscriptionRepository>();
    }
);

builder.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

var app = builder.Build();

await app.RunAsync();
