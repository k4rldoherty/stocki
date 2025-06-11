using Stocki.Application.Queries;
using Stocki.Domain.Models;

namespace Stocki.Application.Interfaces;

public interface IAlphaVantageClient
{
    public Task<StockOverview?> GetStockOverviewAsync(StockOverviewQuery q);
}
