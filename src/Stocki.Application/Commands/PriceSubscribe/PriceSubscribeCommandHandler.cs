using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.Application.Interfaces;
using Stocki.Application.Queries.Price;
using Stocki.Domain.Interfaces;
using Stocki.Domain.Models;

namespace Stocki.Application.Commands.PriceSubscribe;

public class PriceSubscribeCommandHandler : IRequestHandler<PriceSubscribeCommand, bool>
{
    private readonly ILogger<PriceSubscribeCommandHandler> _logger;
    private readonly IStockPriceSubscriptionRepository _stockPriceSubscriptionRepository;
    private readonly IFinnhubClient _client;

    public PriceSubscribeCommandHandler(
        ILogger<PriceSubscribeCommandHandler> l,
        IStockPriceSubscriptionRepository sps,
        IFinnhubClient client
    )
    {
        _logger = l;
        _stockPriceSubscriptionRepository = sps;
        _client = client;
    }

    public async Task<bool> Handle(
        PriceSubscribeCommand request,
        CancellationToken cancellationToken
    )
    {
        var validTickerCheck = await _client.GetStockQuoteAsync(
            new StockQuoteQuery(request.Symbol),
            cancellationToken
        );

        if (validTickerCheck.Data == null)
        {
            _logger.LogInformation("The passed in ticker did not return a valid quote.");
            return false;
        }

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
