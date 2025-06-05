using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stocki.Bot.Chat;
using Stocki.Bot.Commands;
using Stocki.Bot.Setup;
using Stocki.Domain.Interfaces.StockData;
using Stocki.Infrastructure.Clients;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureAppConfiguration(cfg => cfg.AddEnvironmentVariables());
builder.ConfigureServices(
    (context, services) =>
    {
        services.AddSingleton<DiscordSocketClient>();
        services.AddHostedService<BotService>();
        services.AddSingleton<CommandService>();
        services.AddSingleton<CommandRegistry>();
        services.AddSingleton<InputHandlerService>();
        // Keeps Infrastructure decoupled from Bot and Application layers
        services.AddSingleton<IAlphaVantageClient, AlphaVantageClient>();
    }
);
builder.ConfigureLogging(logging => logging.AddConsole());

var app = builder.Build();

await app.RunAsync();
