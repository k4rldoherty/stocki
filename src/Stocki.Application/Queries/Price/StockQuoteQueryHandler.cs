using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.Application.Interfaces;
using Stocki.Domain.Models;

namespace Stocki.Application.Queries.Price;

public class StockQuoteQueryHandler : IRequestHandler<StockQuoteQuery, StockQuote?>
{
    private readonly IFinnhubClient _finnhubClient;
    private readonly ILogger<StockQuoteQueryHandler> _logger;

    public StockQuoteQueryHandler(IFinnhubClient client, ILogger<StockQuoteQueryHandler> l)
    {
        _finnhubClient = client;
        _logger = l;
    }

    public async Task<StockQuote?> Handle(
        StockQuoteQuery request,
        CancellationToken cancellationToken
    )
    {
        StockQuote? domainOverview = await _finnhubClient.GetStockQuoteAsync(
            request,
            cancellationToken
        );
        if (domainOverview == null)
        {
            return null;
        }
        return domainOverview;
    }
}
