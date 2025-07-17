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
        _logger.LogDebug("Handling /subscribe command for symbol: {Symbol}", request.Symbol.Value);
        if (
            _stockPriceSubscriptionRepository.IsUserSubscribedToStockPriceNotifications(
                request.DiscordId,
                request.Symbol,
                cancellationToken
            )
        )
        {
            _logger.LogInformation("User already subscribed to {ticker}", request.Symbol.Value);
            return false;
        }
        StockPriceSubscription sps = new(request.DiscordId, request.Symbol.Value);
        var subscribed = await _stockPriceSubscriptionRepository.AddSubscriptionAsync(
            sps,
            cancellationToken
        );
        return subscribed;
    }
}
