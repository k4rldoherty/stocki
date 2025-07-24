using System.Collections.Concurrent;
using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.Application.Commands.PriceSubscribe;
using Stocki.PriceMonitor.Models;

namespace Stocki.PriceMonitor.Services;

public class PriceChecker
{
    public ConcurrentDictionary<string, double> _stockPrices { get; set; } = new();
    private readonly ILogger<PriceChecker> _logger;
    private IMediator _mediator;

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
                // TODO: Add graceful error handling for situation where stock is not found in dictionary
            }
            else
            {
                // TODO: This is only for newly subscribed stocks, will make better later
                if (currPrice == 0.00)
                    currPrice = t.Price;
                if (HasStockMovedXPercent(t.Price, currPrice))
                {
                    _logger.LogInformation("Price for {} has changed", t.Symbol);
                    _stockPrices.TryUpdate(t.Symbol, t.Price, currPrice);
                    // TODO: Send out a notification through mediatR
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

    private bool HasStockMovedXPercent(double newPrice, double oldPrice)
    {
        if (oldPrice == 0)
            return newPrice != 0;
        var priceChange = ((newPrice - oldPrice) / oldPrice) * 100;
        if (Math.Abs(priceChange) >= 5)
        {
            return true;
        }
        return false;
    }
}
