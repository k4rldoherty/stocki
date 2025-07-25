using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.Application.Interfaces;
using Stocki.Application.Queries.Quote;
using Stocki.Domain.Interfaces;
using Stocki.Domain.Models;
using Stocki.Shared.Notifications;

namespace Stocki.Application.Commands.PriceSubscribe;

public class PriceSubscribeCommandHandler : IRequestHandler<PriceSubscribeCommand, bool>
{
    private readonly ILogger<PriceSubscribeCommandHandler> _logger;
    private readonly IStockPriceSubscriptionRepository _stockPriceSubscriptionRepository;
    private readonly IFinnhubClient _client;
    private readonly IMediator _mediator;

    public PriceSubscribeCommandHandler(
        ILogger<PriceSubscribeCommandHandler> l,
        IStockPriceSubscriptionRepository sps,
        IFinnhubClient client,
        IMediator mediator
    )
    {
        _logger = l;
        _stockPriceSubscriptionRepository = sps;
        _client = client;
        _mediator = mediator;
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

        _logger.LogInformation(
            "Handling /subscribe command for symbol: {Symbol}",
            request.Symbol.Value
        );
        var exists = await _stockPriceSubscriptionRepository.GetStockPriceSubscription(
            request.DiscordId,
            request.Symbol,
            cancellationToken
        );
        if (exists != null && exists.IsActive)
        {
            _logger.LogInformation("User already subscribed to {ticker}", request.Symbol.Value);
            return false;
        }
        else if (exists != null && !exists.IsActive)
        {
            var resubscribed = await _stockPriceSubscriptionRepository.ReSubscribeAsync(
                request.DiscordId,
                request.Symbol.Value,
                cancellationToken
            );
            if (resubscribed)
            {
                await _mediator.Publish(
                    new PriceSubscribedNotification(request.Symbol.Value, request.DiscordId)
                );
            }
            return resubscribed;
        }
        StockPriceSubscription sps = new(request.DiscordId, request.Symbol.Value);
        var subscribed = await _stockPriceSubscriptionRepository.AddSubscriptionAsync(
            sps,
            cancellationToken
        );
        if (subscribed)
        {
            await _mediator.Publish(
                new PriceSubscribedNotification(request.Symbol.Value, request.DiscordId)
            );
        }
        return subscribed;
    }
}
