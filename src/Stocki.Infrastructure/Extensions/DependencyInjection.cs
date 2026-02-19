using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Stocki.Application.Interfaces;
using Stocki.Domain.Interfaces;
using Stocki.Infrastructure.Clients;
using Stocki.Infrastructure.Persistance;
using Stocki.Infrastructure.Persistance.Repositories;
using Stocki.Shared.Config;

namespace Stocki.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAppPersistence(configuration);
        services.AddExternalMarketClients();
        return services;
    }

    private static IServiceCollection AddExternalMarketClients(this IServiceCollection services)
    {
        services.AddHttpClient<IAlphaVantageClient, AlphaVantageClient>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<AlphaVantageSettings>>().Value;
            client.BaseAddress = new Uri(settings.BaseUrl);
        });

        services.AddHttpClient<IFinnhubClient, FinnhubClient>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<FinnhubClientSettings>>().Value;
            client.BaseAddress = new Uri(settings.BaseUrl);
            client.DefaultRequestHeaders.Add("X-Finnhub-Token", settings.ApiKey);
        });

        return services;
    }


    private static IServiceCollection AddAppPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connStr = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connStr)) throw new InvalidOperationException("Connection string is null or empty.");
        services.AddDbContext<StockiDbContext>(opt =>
            {
                opt.UseNpgsql(connStr);
            });

        services.AddScoped<IStockPriceSubscriptionRepository, StockPriceSubscriptionRepository>();
        return services;
    }
}
