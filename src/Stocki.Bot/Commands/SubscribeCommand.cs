using Discord;
using Discord.Interactions;
using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.Application.Commands.PriceSubscribe;
using Stocki.Application.Exceptions;
using Stocki.Domain.Models;
using Stocki.Domain.ValueObjects;

namespace Stocki.Bot.Commands;

public class StockPriceSubscriptionCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<PriceSubscribeCommand> _logger;
    private readonly IMediator _mediator;

    public StockPriceSubscriptionCommand(ILogger<PriceSubscribeCommand> logger, IMediator m)
    {
        _logger = logger;
        _mediator = m;
    }

    [SlashCommand("price-subscribe", "Subscribes a user to price action changes in a stock")]
    public async Task HandlePriceSubscribeAsync(
        [Summary("ticker", "the ticker of the stock you want to subscribe to e.g. AAPL")]
            string ticker
    )
    {
        await DeferAsync();

        try
        {
            TickerSymbol symbol = new TickerSymbol(ticker);
            PriceSubscribeCommand command = new(symbol, Context.User.Id);
            _logger.LogInformation("Received /price-subscribe command for ticker {Ticker}", ticker);
            bool subscribed = await _mediator.Send(command, CancellationToken.None);

            if (subscribed)
            {
                _logger.LogInformation(
                    "The /price-subscribe request succeeded for ticker {Ticker}",
                    ticker
                );
                await FollowupAsync(
                    embed: new EmbedBuilder()
                        .WithTitle("Subscribed!") // Clear, user-centric title
                        .AddField("Message", $"You have sucessfully subscribed to {ticker}") // Directly use the user-friendly message from the exception
                        .WithColor(Color.Green) // A warning/informational color
                        .WithFooter("Stocki 2025")
                        .Build()
                );
            }
            else
            {
                _logger.LogWarning(
                    "The /price-subscribe request failed for ticker {Ticker}",
                    ticker
                );
                await FollowupAsync(
                    embed: new EmbedBuilder()
                        .WithTitle("Cannot Subscribe") // Clear, user-centric title
                        .AddField("Message", $"You cannot sucessfully subscribe to {ticker}") // Directly use the user-friendly message from the exception
                        .AddField(
                            "Next Steps",
                            "Use the /list-subscriptions command to ensure you arent already subscribed to this command and try again"
                        ) // Directly use the user-friendly message from the exception
                        .WithColor(Color.Red) // A warning/informational color
                        .WithFooter("Stocki 2025")
                        .Build()
                );
            }
        }
        catch (Exception ex) // Catch any other unexpected errors
        {
            _logger.LogError(ex.Message);
            await FollowupAsync(
                embed: new EmbedBuilder()
                    .WithTitle("System Error")
                    .WithDescription(
                        "An unexpected error occurred while processing your request. Please try again later."
                    )
                    .WithFooter("If this persists, contact support.")
                    .WithColor(Color.Red)
                    .Build()
            );
        }
    }
}
