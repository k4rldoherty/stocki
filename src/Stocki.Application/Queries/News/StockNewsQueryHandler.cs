using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.Application.Interfaces;
using Stocki.Application.Utilities;
using Stocki.Domain.Models;
using Stocki.Shared.Models;

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
        ApiResponse<List<StockNewsArticle>> Response = await _finnhubClient.GetCompanyNewsAsync(
            request,
            cancellationToken
        );
        if (!Response.IsSuccess)
        {
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw ExceptionGenerator.GenerateNon200StatusCodeException(
                    Response.StatusCode,
                    "news",
                    request.Symbol.Value,
                    Response.Message
                );
            }
            else
            {
                _logger.LogWarning(
                    "No stock news data returned from Finnhub for symbol: {Symbol}",
                    request.Symbol.Value
                );
                throw ExceptionGenerator.GenerateDataNotFoundException(
                    "news",
                    request.Symbol.Value,
                    Response.Message ?? "The data could not be retrieved"
                );
            }
        }
        else
        {
            if (Response.Data is null)
            {
                _logger.LogWarning(
                    "Successfully retrieved response from API, but no stock news data was found for symbol: {Symbol}",
                    request.Symbol.Value
                );
                throw ExceptionGenerator.GenerateDataNotFoundException(
                    "news",
                    request.Symbol.Value,
                    Response.Message ?? "No stock news data available."
                );
            }
            else
            {
                _logger.LogInformation(
                    "Successfully retrieved stock news for symbol: {Symbol}",
                    request.Symbol.Value
                );
                return Response.Data;
            }
        }
    }
}
