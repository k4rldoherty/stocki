using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.Application.Interfaces;
using Stocki.Domain.Models;
using Stocki.Shared.Models;

namespace Stocki.Application.Queries.Overview;

public class StockOverviewQueryHandler : IRequestHandler<StockOverviewQuery, StockOverview?>
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

    public async Task<StockOverview?> Handle(
        StockOverviewQuery request,
        CancellationToken cancellationToken
    )
    {
        _logger.LogDebug("Handling StockOverviewQuery for symbol: {Symbol}", request.Symbol.Value);
        ApiResponse<StockOverview> domainOverview = await _alphaVantageClient.GetStockOverviewAsync(
            request,
            cancellationToken
        );

        if (domainOverview.Data == null)
        {
            _logger.LogWarning(
                "No stock overview data returned from AlphaVantage for symbol: {Symbol}",
                request.Symbol.Value
            );
            return null;
        }

        _logger.LogInformation(
            "Successfully retrieved stock overview for symbol: {Symbol}",
            request.Symbol.Value
        );
        return domainOverview.Data;
    }
}
