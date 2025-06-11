using Stocki.Infrastructure.Clients.DTOs;

namespace Stocki.Infrastructure.Clients;

public interface IAlphaVantageClient
{
    public Task<AVStockOverviewDTO?> GetStockOverviewAsync(string symbol);
}
