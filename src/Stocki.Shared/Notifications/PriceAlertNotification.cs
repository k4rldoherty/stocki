using MediatR;

namespace Stocki.Shared.Notifications;

public record PriceAlertNotification : INotification
{
    public string Symbol { get; set; }
    public decimal Price { get; set; }
    public decimal PercentChange { get; set; }

    public PriceAlertNotification(string symbol, decimal price, decimal percentChange)
    {
        Symbol = symbol;
        Price = price;
        PercentChange = percentChange;
    }
}
