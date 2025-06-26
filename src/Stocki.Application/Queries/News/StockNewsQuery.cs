using MediatR;
using Stocki.Domain.Models;
using Stocki.Domain.ValueObjects;

namespace Stocki.Application.Queries.News;

public record StockNewsQuery : IRequest<List<StockNewsArticle>?>
{
    public TickerSymbol Symbol { get; set; }

    // public DateTime? From { get; set; }
    // public DateTime? To { get; set; }

    public StockNewsQuery(TickerSymbol s)
    {
        if (s == null)
            throw new ArgumentNullException("Symbol must have a value");
        Symbol = s;
    }
}
