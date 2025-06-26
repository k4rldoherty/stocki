using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.Application.Interfaces;
using Stocki.Domain.Models;

namespace Stocki.Application.Queries.News;

public class StockNewsQueryHandler : IRequestHandler<StockNewsQuery, List<StockNewsArticle>?>
{
    private readonly IFinnhubClient _finnhubClient;
    private ILogger<StockNewsQueryHandler> _logger;

    public StockNewsQueryHandler(ILogger<StockNewsQueryHandler> logger, IFinnhubClient client)
    {
        _finnhubClient = client;
        _logger = logger;
    }

    public async Task<List<StockNewsArticle>?> Handle(
        StockNewsQuery request,
        CancellationToken cancellationToken
    )
    {
        _logger.LogDebug("Handling StockNewsQuery for symbol: {Symbol}", request.Symbol.Value);
        List<StockNewsArticle>? domainNewsArticles = await _finnhubClient.GetCompanyNewsAsync(
            request,
            cancellationToken
        );

        if (domainNewsArticles == null)
        {
            _logger.LogWarning(
                "No stock news data returned from Finnhub for symbol: {Symbol}",
                request.Symbol.Value
            );
            return null;
        }

        _logger.LogInformation(
            "Successfully retrieved stock overview for symbol: {Symbol}",
            request.Symbol.Value
        );
        return domainNewsArticles;
    }
}
