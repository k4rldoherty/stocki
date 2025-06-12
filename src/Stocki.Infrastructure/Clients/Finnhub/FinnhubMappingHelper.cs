using Microsoft.Extensions.Logging; // For ILogger
using Stocki.Domain.Models;
using Stocki.Infrastructure.Clients.Finnhub.DTOs;

namespace Stocki.Infrastructure.Clients.Finnhub;

public static class FinnhubMappingHelper
{
    public static StockQuote? MapStockQuote(FHStockQuoteDTO fhObj, string symbol, ILogger logger)
    {
        if (string.IsNullOrEmpty(fhObj.DifferencePercentage.ToString()))
        {
            logger.LogWarning($"Ticker {symbol} is invalid for item difference %");
            return null;
        }

        if (!double.TryParse(fhObj.OpenPrice, out var openingPrice))
        {
            logger.LogWarning($"Ticker {symbol} is invalid for item open price");
            return null;
        }

        if (!double.TryParse(fhObj.PreviousClosePrice, out var previousClosePrice))
        {
            logger.LogWarning($"Ticker {symbol} is invalid for item close price");
            return null;
        }

        if (!double.TryParse(fhObj.CurrentPrice, out var currentPrice))
        {
            logger.LogWarning($"Ticker {symbol} is invalid for item current price");
            return null;
        }

        if (!double.TryParse(fhObj.HighPrice, out var high))
        {
            logger.LogWarning($"Ticker {symbol} is invalid for item high price");
            return null;
        }

        if (!double.TryParse(fhObj.LowPrice, out var low))
        {
            logger.LogWarning($"Ticker {symbol} is invalid for item low price");
            return null;
        }

        StockQuote quote = new(
            Ticker: symbol,
            CurrentPrice: currentPrice,
            OpeningPrice: openingPrice,
            ClosingPrice: previousClosePrice,
            High: high,
            Low: low
        );

        return quote;
    }
}
