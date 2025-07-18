using Stocki.Domain.Models;
using Stocki.Domain.ValueObjects;

namespace Stocki.Domain.Interfaces;

public interface IStockPriceSubscriptionRepository
{
    public Task<bool> AddSubscriptionAsync(StockPriceSubscription sps, CancellationToken token);
    public bool IsUserSubscribedToStockPriceNotifications(
        ulong discordId,
        TickerSymbol symbol,
        CancellationToken token
    );
    public Task<List<StockPriceSubscription>> GetAllSubscriptionsForUserAsync(
        ulong discordId,
        CancellationToken token
    );
    public Task<List<StockPriceSubscription>> GetAllSubscriptionsAsync(CancellationToken token);
}
