using MediatR;
using Stocki.Domain.ValueObjects;

namespace Stocki.Application.Queries.Overview;

public record StockOverviewQuery : IRequest<StockOverview?>
{
    public TickerSymbol Symbol { get; set; }

    public StockOverviewQuery(TickerSymbol s)
    {
        Symbol = s;
    }
}
