using Stocki.Application.Queries.Overview;
using Stocki.Domain.Models;

namespace Stocki.Application.Interfaces;

public interface IAlphaVantageClient
{
    public Task<StockOverview?> GetStockOverviewAsync(StockOverviewQuery q, CancellationToken t);
}
