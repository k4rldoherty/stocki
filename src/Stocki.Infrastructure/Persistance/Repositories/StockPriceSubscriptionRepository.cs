using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stocki.Domain.Interfaces;
using Stocki.Domain.Models;
using Stocki.Domain.ValueObjects;

namespace Stocki.Infrastructure.Persistance.Repositories;

public class StockPriceSubscriptionRepository : IStockPriceSubscriptionRepository
{
    private readonly StockiDbContext _stockiDbContext;
    private readonly ILogger<StockPriceSubscriptionRepository> _logger;

    public StockPriceSubscriptionRepository(
        StockiDbContext context,
        ILogger<StockPriceSubscriptionRepository> logger
    )
    {
        _stockiDbContext = context;
        _logger = logger;
    }

    public async Task<bool> AddSubscriptionAsync(
        StockPriceSubscription sps,
        CancellationToken token
    )
    {
        try
        {
            await _stockiDbContext.StockPriceSubscriptions.AddAsync(sps, token);
            var success = await _stockiDbContext.SaveChangesAsync(token);
            return success > 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error: {error}", ex.Message);
            return false;
        }
    }

    public bool IsUserSubscribedToStockPriceNotifications(
        ulong discordId,
        TickerSymbol symbol,
        CancellationToken token
    )
    {
        return _stockiDbContext.StockPriceSubscriptions.FirstOrDefault(x =>
                x.DiscordId == discordId && x.Ticker == symbol.Value
            ) != null;
    }

    public async Task<List<StockPriceSubscription>> GetAllSubscriptionsForUserAsync(
        ulong discordId,
        CancellationToken token
    )
    {
        var res = await _stockiDbContext
            .StockPriceSubscriptions.Where(x => x.DiscordId == discordId)
            .ToListAsync(token);

        return res;
    }

    public async Task<List<StockPriceSubscription>> GetAllSubscriptionsAsync(
        CancellationToken token
    )
    {
        var res = await _stockiDbContext.StockPriceSubscriptions.ToListAsync(token);
        return res;
    }
}
