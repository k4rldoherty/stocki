using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Stocki.Discord.Handlers;
using Stocki.Discord.Interfaces;
using Stocki.Discord.Setup;
using Stocki.PriceMonitor.Services;
using Stocki.Shared.Config;

namespace Stocki.Discord.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddDiscord(this IServiceCollection services)
    {
        services.AddMemoryCache();

        services.AddSingleton(x =>
        {
            var discordSettings = x.GetRequiredService<IOptions<DiscordSettings>>().Value; // Get the settings here
            return new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                // GatewayIntents = GatewayIntents.All
            });
        });

        // Interaction Service that handles the execution of commands
        services.AddSingleton(x => new InteractionService(
            x.GetRequiredService<DiscordSocketClient>(),
            new InteractionServiceConfig()
            {
                AutoServiceScopes = true,
                LogLevel = LogSeverity.Info,
                DefaultRunMode = RunMode.Async
            }
        ));

        services.AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>();

        services.AddHostedService<BotStartupService>();
        services.AddHostedService<PriceMonitoringService>();
        services.AddSingleton<MessageHandler>();
        services.AddSingleton<FinnhubWSManager>();
        services.AddSingleton<PriceChecker>();

        return services;
    }
}
