using MediatR;
using Stocki.Domain.Models;

namespace Stocki.Application.Queries.Subscription;

public record ListPriceSubscriptionQuery : IRequest<List<StockPriceSubscription>>
{
    public ulong DiscordId { get; set; }

    public ListPriceSubscriptionQuery(ulong id)
    {
        DiscordId = id;
    }
}
