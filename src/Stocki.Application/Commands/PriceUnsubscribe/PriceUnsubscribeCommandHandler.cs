using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.Application.Interfaces;
using Stocki.Application.Queries.Quote;
using Stocki.Domain.Interfaces;
using Stocki.Domain.Models;
using Stocki.Shared.Notifications;

namespace Stocki.Application.Commands.PriceUnsubscribe;

public class PriceUnsubscribeCommandHandler : IRequestHandler<PriceUnsubscribeCommand, bool>
{
    private readonly ILogger<PriceUnsubscribeCommandHandler> _logger;
    private readonly IStockPriceSubscriptionRepository _stockPriceSubscriptionRepository;
    private readonly IFinnhubClient _client;
    private readonly IMediator _mediator;

    public PriceUnsubscribeCommandHandler(
        ILogger<PriceUnsubscribeCommandHandler> l,
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
        PriceUnsubscribeCommand request,
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
            "Handling /unsubscribe command for symbol: {Symbol}",
            request.Symbol.Value
        );

        var exists = await _stockPriceSubscriptionRepository.GetStockPriceSubscription(
            request.DiscordId,
            request.Symbol,
            cancellationToken
        );
        if (exists != null && exists.IsActive)
        {
            var unsubscribed = await _stockPriceSubscriptionRepository.UnsubscribeAsync(
                request.DiscordId,
                request.Symbol.Value,
                cancellationToken
            );
            if (unsubscribed)
            {
                await _mediator.Publish(
                    new PriceUnsubscribedNotification(request.Symbol.Value, request.DiscordId)
                );
            }
            return unsubscribed;
        }
        _logger.LogInformation("User is not subscribed to {ticker}", request.Symbol.Value);
        return false;
    }
}
