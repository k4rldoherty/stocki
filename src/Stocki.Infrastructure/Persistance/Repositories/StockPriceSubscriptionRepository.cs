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

    public async Task<bool> ReSubscribeAsync(
        ulong discordId,
        string ticker,
        CancellationToken token
    )
    {
        var existing = await _stockiDbContext.StockPriceSubscriptions.FirstOrDefaultAsync(x =>
            x.DiscordId == discordId && x.Ticker == ticker && !x.IsActive
        );
        if (existing != null)
        {
            existing.IsActive = true;
            await _stockiDbContext.SaveChangesAsync(token);
            return true;
        }
        return false;
    }

    public async Task<StockPriceSubscription?> GetStockPriceSubscription(
        ulong discordId,
        TickerSymbol symbol,
        CancellationToken token
    )
    {
        var res = await _stockiDbContext.StockPriceSubscriptions.FirstOrDefaultAsync(x =>
            x.DiscordId == discordId && x.Ticker == symbol.Value
        );
        return res;
    }

    public async Task<List<StockPriceSubscription>> GetAllSubscriptionsForUserAsync(
        ulong discordId,
        CancellationToken token
    )
    {
        var res = await _stockiDbContext
            .StockPriceSubscriptions.Where(x => x.DiscordId == discordId && x.IsActive)
            .ToListAsync(token);

        return res;
    }

    public async Task<List<StockPriceSubscription>> GetAllSubscriptionsAsync(
        CancellationToken token
    )
    {
        var res = await _stockiDbContext
            .StockPriceSubscriptions.Where(x => x.IsActive)
            .ToListAsync(token);
        return res;
    }

    public async Task<bool> UnsubscribeAsync(
        ulong discordId,
        string ticker,
        CancellationToken token
    )
    {
        var toUnsubscribe = await _stockiDbContext.StockPriceSubscriptions.FirstOrDefaultAsync(x =>
            x.DiscordId == discordId && x.Ticker == ticker
        );
        if (toUnsubscribe != null)
        {
            toUnsubscribe.IsActive = false;
            var res = await _stockiDbContext.SaveChangesAsync(token) > 0;
            return res;
        }
        return false;
    }

    public async Task<List<ulong>> GetAllUsersSubscribedToAStock(
        string symbol,
        CancellationToken token
    )
    {
        var subs = await _stockiDbContext
            .StockPriceSubscriptions.Where(x => x.Ticker == symbol && x.IsActive)
            .Select(x => x.DiscordId)
            .Distinct()
            .ToListAsync(token);
        return subs;
    }
}
