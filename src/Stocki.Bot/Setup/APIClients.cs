using Stocki.Infrastructure.Clients;
using Stocki.Application.Interfaces;
using Stocki.Shared.Config;

namespace Stocki.Bot.Setup;

public class APIClients
{
    public static void SetupAPIClients(HostBuilderContext context, IServiceCollection services)
    {
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
            client.DefaultRequestHeaders.Add(
                "X-Finnhub-Token",
                context.Configuration.GetSection("Finnhub").GetSection("ApiKey").Value
            );
        });
    }
}
