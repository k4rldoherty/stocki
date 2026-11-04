using Stocki.Application.Queries.Overview;
using Stocki.Shared.Models;

namespace Stocki.Application.Interfaces;

public interface IAlphaVantageClient
{
    ValueTask<ApiResponse<StockOverview>> GetStockOverviewAsync(
        StockOverviewQuery q,
        CancellationToken t
    );
}
