using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.Domain.Interfaces;
using Stocki.Domain.Models;

namespace Stocki.Application.Queries.Subscription;

public class ListPriceSubscriptionsQueryHandler
    : IRequestHandler<ListPriceSubscriptionQuery, List<StockPriceSubscription>>
{
    private readonly ILogger<ListPriceSubscriptionsQueryHandler> _logger;
    private readonly IStockPriceSubscriptionRepository _repository;

    public ListPriceSubscriptionsQueryHandler(
        ILogger<ListPriceSubscriptionsQueryHandler> l,
        IStockPriceSubscriptionRepository repo
    )
    {
        _logger = l;
        _repository = repo;
    }

    public async Task<List<StockPriceSubscription>> Handle(
        ListPriceSubscriptionQuery request,
        CancellationToken cancellationToken
    )
    {
        var res = await _repository.GetAllSubscriptionsForUserAsync(
            request.DiscordId,
            cancellationToken
        );
        return res;
    }
}
