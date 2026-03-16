using Stocki.Shared.Config;
namespace Stocki.Discord.Extensions;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Register services the new IOptions<T> way
        services.Configure<AlphaVantageSettings>(configuration.GetSection("AlphaVantage"));
        services.Configure<AlphaVantageSettings>(configuration.GetSection("AlphaVantage"));
        services.Configure<FinnhubClientSettings>(configuration.GetSection("Finnhub"));
        services.Configure<DiscordSettings>(configuration.GetSection("Discord"));
        services.Configure<FinnhubWebsocketsSettings>(
            configuration.GetSection("FinnhubWebsockets")
        );
        services.Configure<GeminiSettings>(configuration.GetSection("Gemini"));
        return services;
    }
}
