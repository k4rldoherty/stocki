using Stocki.Application.Queries.Overview;
using Stocki.PriceMonitor.Services;
using Stocki.NotificationService;

namespace Stocki.Bot.Setup;

public class MediatRSetup
{
    public static void SetupMediatR(HostBuilderContext context, IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(StockOverviewQuery).Assembly);
            configuration.RegisterServicesFromAssembly(typeof(FinnhubWSManager).Assembly);
            configuration.RegisterServicesFromAssembly(
                typeof(PriceMovedBeyondThresholdHandler).Assembly
            );
        });
    }
}
