using Stocki.Domain.Models.StockData;

namespace Stocki.Domain.Interfaces.StockData;

public interface IAlphaVantageClient
{
    public Task<StockOverview?> GetStockOverviewAsync(string symbol);
}
