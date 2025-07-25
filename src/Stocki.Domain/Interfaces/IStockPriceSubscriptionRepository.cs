using Stocki.Domain.Models;
using Stocki.Domain.ValueObjects;

namespace Stocki.Domain.Interfaces;

public interface IStockPriceSubscriptionRepository
{
    public Task<bool> AddSubscriptionAsync(StockPriceSubscription sps, CancellationToken token);
    public Task<StockPriceSubscription?> GetStockPriceSubscription(
        ulong discordId,
        TickerSymbol symbol,
        CancellationToken token
    );
    public Task<List<StockPriceSubscription>> GetAllSubscriptionsForUserAsync(
        ulong discordId,
        CancellationToken token
    );
    public Task<List<StockPriceSubscription>> GetAllSubscriptionsAsync(CancellationToken token);
    public Task<bool> UnsubscribeAsync(ulong discordId, string ticker, CancellationToken token);
    public Task<bool> ReSubscribeAsync(ulong discordId, string ticker, CancellationToken token);
    public Task<List<ulong>> GetAllUsersSubscribedToAStock(string ticker, CancellationToken token);
}
