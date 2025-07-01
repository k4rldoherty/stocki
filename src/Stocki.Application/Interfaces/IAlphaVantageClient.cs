using Stocki.Application.Queries.Overview;
using Stocki.Domain.Models;
using Stocki.Shared.Models;

namespace Stocki.Application.Interfaces;

public interface IAlphaVantageClient
{
    public Task<ApiResponse<StockOverview>> GetStockOverviewAsync(
        StockOverviewQuery q,
        CancellationToken t
    );
}
