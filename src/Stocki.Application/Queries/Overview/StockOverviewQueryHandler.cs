using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.Application.Interfaces;
using Stocki.Application.Utilities;
using Stocki.Domain.Models;
using Stocki.Shared.Models;

namespace Stocki.Application.Queries.Overview;

public class StockOverviewQueryHandler : IRequestHandler<StockOverviewQuery, StockOverview?>
{
    private readonly IAlphaVantageClient _alphaVantageClient;
    private readonly ILogger<StockOverviewQueryHandler> _logger;

    public StockOverviewQueryHandler(
        IAlphaVantageClient client,
        ILogger<StockOverviewQueryHandler> l
    )
    {
        _alphaVantageClient = client;
        _logger = l;
    }

    public async Task<StockOverview?> Handle(
        StockOverviewQuery request,
        CancellationToken cancellationToken
    )
    {
        _logger.LogDebug("Handling StockOverviewQuery for symbol: {Symbol}", request.Symbol.Value);
        ApiResponse<StockOverview> Response = await _alphaVantageClient.GetStockOverviewAsync(
            request,
            cancellationToken
        );

        if (!Response.IsSuccess)
        {
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw ExceptionGenerator.GenerateNon200StatusCodeException(
                    Response.StatusCode,
                    "overview",
                    request.Symbol.Value,
                    Response.Message
                );
            }
            else
            {
                _logger.LogWarning(
                    "No stock overview data returned from AlphaVantage for symbol: {Symbol}",
                    request.Symbol.Value
                );
                throw ExceptionGenerator.GenerateDataNotFoundException(
                    "overview",
                    request.Symbol.Value,
                    "The data could not be retrieved"
                );
            }
        }
        else
        {
            if (Response.Data is null)
            {
                _logger.LogWarning(
                    "Successfully retrieved response from API, but no stock overview data was found for symbol: {Symbol}",
                    request.Symbol.Value
                );
                throw ExceptionGenerator.GenerateDataNotFoundException(
                    "StockOverviewQuery",
                    request.Symbol.Value,
                    "No stock overview data available."
                );
            }
            else
            {
                _logger.LogInformation(
                    "Successfully retrieved stock overview for symbol: {Symbol}",
                    request.Symbol.Value
                );
                return Response.Data;
            }
        }
    }
}
