using System.Collections.Concurrent;
using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.PriceMonitor.Models;
using Stocki.Shared.Notifications;

namespace Stocki.PriceMonitor.Services;

public class PriceChecker
{
    public ConcurrentDictionary<string, double> _stockPrices { get; set; } = new();
    private readonly ILogger<PriceChecker> _logger;
    private IMediator _mediator;
    const double PRICECHANGE = 5;

    public PriceChecker(ILogger<PriceChecker> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public void CheckPrice(FinnhubStockPriceRecievedMessage msg)
    {
        // handles messages with multiple trades
        foreach (var t in msg.Data)
        {
            if (!_stockPrices.TryGetValue(t.Symbol, out var currPrice))
            {
                _stockPrices.TryAdd(t.Symbol, 0.0);
            }
            else
            {
                // FIX: This is only for newly subscribed stocks, will make better later
                if (currPrice == 0.00)
                    currPrice = t.Price;
                var priceChange = GetPercentageDifference(t.Price, currPrice);
                if (priceChange >= PRICECHANGE)
                {
                    _logger.LogInformation("Price for {} has changed {}%", t.Symbol, PRICECHANGE);
                    _stockPrices.TryUpdate(t.Symbol, t.Price, currPrice);
                    _mediator.Publish(
                        new PriceMovedBeyondThresholdNotification(t.Symbol, t.Price, priceChange)
                    );
                    // FIX: To prevent multiple messages when there are lots of trades in a message, fix in future
                    break;
                }
                // else
                // {
                //      _logger.LogInformation(
                //          "{} - Current Price: {}. Recieved Price: {}",
                //          t.Symbol,
                //          currPrice,
                //          t.Price
                //      );
                // }
            }
        }
    }

    private double GetPercentageDifference(double newPrice, double oldPrice)
    {
        if (oldPrice == 0)
            return 0;
        var priceChange = ((newPrice - oldPrice) / oldPrice) * 100;
        return priceChange;
    }
}
