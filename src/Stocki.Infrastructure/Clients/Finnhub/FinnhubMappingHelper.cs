using Microsoft.Extensions.Logging; // For ILogger
using Stocki.Domain.Models;
using Stocki.Infrastructure.Clients.Finnhub.DTOs;

namespace Stocki.Infrastructure.Clients.Finnhub;

public static class FinnhubMappingHelper
{
    public static StockQuote? MapStockQuote(FHStockQuoteDTO fhObj, string symbol, ILogger logger)
    {
        if (string.IsNullOrEmpty(fhObj.DifferencePercentage))
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

    public static List<StockNewsArticle>? MapStockNews(
        List<FHStockNewsArticleDTO> dtoList,
        string symbol,
        ILogger logger
    )
    {
        List<StockNewsArticle>? articles = new();
        int i = 0;
        while (articles.Count() <= 3)
        {
            var a = dtoList[i];
            if (string.IsNullOrEmpty(a.TimeStamp))
            {
                logger.LogWarning("Date is invalid or empty");
                return null;
            }
            if (string.IsNullOrEmpty(a.Headline))
            {
                logger.LogWarning("Headline is invalid or empty");
                return null;
            }
            if (string.IsNullOrEmpty(a.Image))
            {
                logger.LogWarning("Image is invalid or empty");
            }
            if (string.IsNullOrEmpty(a.Source))
            {
                logger.LogWarning("Source is invalid or empty");
                return null;
            }
            if (string.IsNullOrEmpty(a.Summary))
            {
                logger.LogWarning("Summary is invalid or empty");
                return null;
            }
            if (string.IsNullOrEmpty(a.Url))
            {
                logger.LogWarning("Url is invalid or empty");
                return null;
            }
            var newArticle = new StockNewsArticle(
                a.TimeStamp,
                a.Headline,
                a.Image,
                a.Source,
                a.Summary,
                a.Url
            );

            articles.Add(newArticle);
            i++;
        }

        return articles;
    }
}
