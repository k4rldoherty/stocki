using MediatR;
using Stocki.Domain.Models;
using Stocki.Domain.ValueObjects;

namespace Stocki.Application.Queries.Overview;

public record StockOverviewQuery : IRequest<StockOverview?>
{
    public TickerSymbol Symbol { get; set; }

    public StockOverviewQuery(TickerSymbol s)
    {
        if (s == null)
        {
            throw new ArgumentNullException("TickerSymbol object must be provided");
        }
        Symbol = s;
    }
}
