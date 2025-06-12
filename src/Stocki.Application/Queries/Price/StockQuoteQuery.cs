using Stocki.Domain.ValueObjects;

namespace Stocki.Application.Queries.Price;

public record StockQuoteQuery
{
    public TickerSymbol Symbol { get; set; }

    public StockQuoteQuery(TickerSymbol s)
    {
        if (s == null)
        {
            throw new ArgumentNullException("TickerSymbol object must be provided");
        }
        Symbol = s;
    }
}
