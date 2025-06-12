using Stocki.Application.Queries.Overview;
using Stocki.Domain.Models;

public interface IStockOverviewQueryHandler
{
    Task<StockOverview?> HandleAsync(StockOverviewQuery q);
}
