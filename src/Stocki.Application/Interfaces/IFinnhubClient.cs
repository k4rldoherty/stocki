using Stocki.Application.Queries.News;
using Stocki.Application.Queries.Price;
using Stocki.Domain.Models;

namespace Stocki.Application.Interfaces;

public interface IFinnhubClient
{
    Task<StockQuote?> GetStockQuoteAsync(StockQuoteQuery q, CancellationToken t);
    Task<List<StockNewsArticle>?> GetCompanyNewsAsync(StockNewsQuery q, CancellationToken t);
}
