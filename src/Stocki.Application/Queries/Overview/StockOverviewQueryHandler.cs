using Microsoft.Extensions.Logging;
using Stocki.Application.Interfaces;
using Stocki.Domain.Models;

namespace Stocki.Application.Queries.Overview;

public class StockOverviewQueryHandler
{
    private readonly IAlphaVantageClient _alphaVantageClient;
    private readonly ILogger<StockOverviewQueryHandler> _logger;

    public StockOverviewQueryHandler(
        IAlphaVantageClient client,
        ILogger<StockOverviewQueryHandler> l
    )
    {
        _alphaVantageClient = client;
        _logger = l;
    }

    public async Task<StockOverview?> HandleAsync(StockOverviewQuery q)
    {
        var domainOverview = await _alphaVantageClient.GetStockOverviewAsync(q);
        if (domainOverview == null)
            return null;
        return domainOverview;
    }
}
