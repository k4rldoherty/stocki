using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.Domain.Interfaces;
using Stocki.Domain.Models;

namespace Stocki.Application.Commands.PriceSubscribe;

public class PriceSubscribeCommandHandler : IRequestHandler<PriceSubscribeCommand, bool>
{
    private readonly ILogger<PriceSubscribeCommandHandler> _logger;
    private readonly IStockPriceSubscriptionRepository _stockPriceSubscriptionRepository;

    public PriceSubscribeCommandHandler(
        ILogger<PriceSubscribeCommandHandler> l,
        IStockPriceSubscriptionRepository sps
    )
    {
        _logger = l;
        _stockPriceSubscriptionRepository = sps;
    }

    public async Task<bool> Handle(
        PriceSubscribeCommand request,
        CancellationToken cancellationToken
    )
    {
        _logger.LogDebug("Handling StockOverviewQuery for symbol: {Symbol}", request.Symbol.Value);
        StockPriceSubscription sps = new(request.DiscordId, request.Symbol.Value);
        var subscribed = await _stockPriceSubscriptionRepository.AddSubscriptionAsync(
            sps,
            cancellationToken
        );
        return subscribed;
    }
}
