using MediatR;

namespace Stocki.Shared.Notifications;

public record PriceMovedBeyondThresholdNotification : INotification
{
    public string Symbol { get; set; }
    public decimal Price { get; set; }
    public decimal PercentChange { get; set; }

    public PriceMovedBeyondThresholdNotification(string symbol, decimal price, decimal percentChange)
    {
        Symbol = symbol;
        Price = price;
        PercentChange = percentChange;
    }
}
