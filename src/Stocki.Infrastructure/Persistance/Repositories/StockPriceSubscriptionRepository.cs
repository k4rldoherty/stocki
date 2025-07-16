using Microsoft.Extensions.Logging;
using Stocki.Domain.Interfaces;
using Stocki.Domain.Models;

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
            await _stockiDbContext.StockPriceSubscriptions.AddAsync(sps);
            var success = await _stockiDbContext.SaveChangesAsync();
            return success > 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error: {error}", ex.Message);
            return false;
        }
    }
}
