using Stocki.Shared.Config;
using Microsoft.Extensions.Options;
using Discord.Interactions;
using Discord.WebSocket;

namespace Stocki.Bot.Setup;

public class DiscordClient
{
    public static void SetupDiscordClient(HostBuilderContext context, IServiceCollection services)
    {
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
    }
}
