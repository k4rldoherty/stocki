using System.Collections.Concurrent;
using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.PriceMonitor.Models;
using Stocki.Shared.Notifications;

namespace Stocki.PriceMonitor.Services;

public class PriceChecker
{
    public ConcurrentDictionary<string, decimal> _stockPrices = new ConcurrentDictionary<string, decimal>();
    private readonly ILogger<PriceChecker> _logger;
    private IMediator _mediator;

    public PriceChecker(ILogger<PriceChecker> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public void CheckPrice(FinnhubStockPriceReceivedMessage msg, decimal percentageThreshold)
    {
        foreach (var t in msg.Data)
        {
            if (!_stockPrices.TryGetValue(t.Symbol, out var currPrice))
            {
                _logger.LogInformation("Price for {} is null", t);
                _stockPrices.TryAdd(t.Symbol, 0m);
            }
            else
            {
                var priceChange = GetPercentageDifference(t.Price, currPrice);
                _logger.LogInformation("Price for {} has changed {}%", t.Symbol, priceChange);
                if (priceChange >= percentageThreshold)
                {
                    _stockPrices.TryUpdate(t.Symbol, t.Price, currPrice);
                    _mediator.Publish(
                        new PriceAlertNotification(t.Symbol, t.Price, currPrice)
                    );
                    break;
                }
                else
                {
                    _logger.LogInformation(
                        "{} - Current Price: {}. Recieved Price: {}",
                        t.Symbol,
                        currPrice,
                        t.Price
                    );
                }
            }
        }
    }

    public decimal GetPercentageDifference(decimal newPrice, decimal oldPrice)
    {
        if (oldPrice == 0)
            return 0;
        var priceChange = ((newPrice - oldPrice) / oldPrice) * 100;
        return priceChange;
    }
}
