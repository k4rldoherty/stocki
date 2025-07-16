using Stocki.Domain.Models;

namespace Stocki.Domain.Interfaces;

public interface IStockPriceSubscriptionRepository
{
    public Task<bool> AddSubscriptionAsync(StockPriceSubscription sps, CancellationToken token);
}
