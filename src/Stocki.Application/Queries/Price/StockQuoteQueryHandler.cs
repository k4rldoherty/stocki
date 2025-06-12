using Microsoft.Extensions.Logging;
using Stocki.Application.Interfaces;
using Stocki.Domain.Models;

namespace Stocki.Application.Queries.Price;

public class StockQuoteQueryHandler
{
    private readonly IFinnhubClient _finnhubClient;
    private readonly ILogger<StockQuoteQueryHandler> _logger;

    public StockQuoteQueryHandler(IFinnhubClient client, ILogger<StockQuoteQueryHandler> l)
    {
        _finnhubClient = client;
        _logger = l;
    }

    public async Task<StockQuote?> HandleAsync(StockQuoteQuery q)
    {
        var domainOverview = await _finnhubClient.GetStockQuoteAsync(q);
        if (domainOverview == null)
        {
            _logger.LogWarning("Item returned from infra layer was null");
            return null;
        }
        return domainOverview;
    }
}
