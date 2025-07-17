using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.Application.Interfaces;
using Stocki.Application.Utilities;
using Stocki.Domain.Models;
using Stocki.Shared.Models;

namespace Stocki.Application.Queries.Quote;

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
        ApiResponse<StockQuote> Response = await _finnhubClient.GetStockQuoteAsync(
            request,
            cancellationToken
        );
        if (!Response.IsSuccess)
        {
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw ExceptionGenerator.GenerateNon200StatusCodeException(
                    Response.StatusCode,
                    "quote",
                    request.Symbol.Value,
                    Response.Message
                );
            }
            else
            {
                _logger.LogWarning(
                    "No stock quote data returned from Finnhub for symbol: {Symbol}",
                    request.Symbol.Value
                );
                throw ExceptionGenerator.GenerateDataNotFoundException(
                    "quote",
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
                    "Successfully retrieved response from API, but no stock quote data was found for symbol: {Symbol}",
                    request.Symbol.Value
                );
                throw ExceptionGenerator.GenerateDataNotFoundException(
                    "quote",
                    request.Symbol.Value,
                    Response.Message ?? "No stock quote data available."
                );
            }
            else
            {
                _logger.LogInformation(
                    "Successfully retrieved stock quote for symbol: {Symbol}",
                    request.Symbol.Value
                );
                return Response.Data;
            }
        }
    }
}
