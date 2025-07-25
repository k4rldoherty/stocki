using MediatR;

namespace Stocki.Shared.Notifications;

public record PriceMovedBeyondThresholdNotification : INotification
{
    public string Symbol { get; set; }
    public double Price { get; set; }
    public double PercentChange { get; set; }

    public PriceMovedBeyondThresholdNotification(string symbol, double price, double percentChange)
    {
        Symbol = symbol;
        Price = price;
        PercentChange = percentChange;
    }
}
