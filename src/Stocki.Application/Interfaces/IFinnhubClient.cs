using Stocki.Application.Queries.News;
using Stocki.Application.Queries.Quote;
using Stocki.Domain.Models;
using Stocki.Shared.Models;

namespace Stocki.Application.Interfaces;

public interface IFinnhubClient
{
    Task<ApiResponse<StockQuote>> GetStockQuoteAsync(StockQuoteQuery q, CancellationToken t);
    Task<ApiResponse<List<StockNewsArticle>>> GetCompanyNewsAsync(
        StockNewsQuery q,
        CancellationToken t
    );
}
