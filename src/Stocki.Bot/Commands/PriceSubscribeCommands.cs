using Discord;
using Discord.Interactions;
using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.Application.Commands.PriceSubscribe;
using Stocki.Application.Queries.Subscription;
using Stocki.Domain.Models;
using Stocki.Domain.ValueObjects;

namespace Stocki.Bot.Commands;

public class PriceSubscribeCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<PriceSubscribeCommand> _logger;
    private readonly IMediator _mediator;

    public PriceSubscribeCommands(ILogger<PriceSubscribeCommand> logger, IMediator m)
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
            await _mediator.Publish(command, CancellationToken.None);
            var subscribed = true;
            if (subscribed)
            {
                _logger.LogInformation(
                    "The /price-subscribe request succeeded for ticker {Ticker}",
                    ticker
                );
                await FollowupAsync(
                    embed: new EmbedBuilder()
                        .WithTitle("Subscribed!") // Clear, user-centric title
                        .AddField("Success!", $"You have sucessfully subscribed to {ticker}")
                        .AddField(
                            "Info",
                            $"You will now recieve real time updates when {ticker} moves up or down 5%"
                        )
                        .AddField(
                            "Want to unsubscribe?",
                            "To unsubscribe just use the unsubscribe command!"
                        )
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
        catch (ArgumentException ex) // Catch validation errors from TickerSymbol or other ArgumentExceptions
        {
            await FollowupAsync(
                embed: new EmbedBuilder()
                    .WithTitle("Input Error")
                    .WithDescription($"The ticker '{ticker}' is invalid. Reason: {ex.Message}")
                    .WithColor(Color.Orange) // Use a different color for input errors
                    .Build()
            );
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

    [SlashCommand(
        "list-price-subscriptions",
        "Lists all the stocks you are currently subscribed to"
    )]
    public async Task HandleListPriceSubscriptionsAsync()
    {
        await DeferAsync();

        try
        {
            _logger.LogInformation("Received /list-price-subscriptions command");
            ListPriceSubscriptionQuery Query = new(Context.User.Id);
            List<StockPriceSubscription> subscribedStocks = await _mediator.Send(
                Query,
                CancellationToken.None
            );

            if (subscribedStocks.Count > 0)
            {
                var e = new EmbedBuilder()
                    .WithTitle("Subscribed Stocks") // Clear, user-centric title
                    .WithFooter("To unsubscribe just use the unsubscribe command!")
                    .WithColor(Color.Green);
                foreach (var s in subscribedStocks)
                {
                    string fieldName = $"**{s.Ticker} - **";
                    string fieldValue = $"\n\n{s.CreatedDate}\n\n";
                    e.AddField(fieldName, fieldValue, true);
                }
                await FollowupAsync(embed: e.Build());
                _logger.LogInformation("The /list-price-subscriptions request succeeded");
            }
            else
            {
                _logger.LogInformation(
                    "The /list-price-subscriptions request succeeded, but the user is not subscribed to any stocks"
                );
                await FollowupAsync(
                    embed: new EmbedBuilder()
                        .WithTitle("No Subscriptions Yet") // Clear, user-centric title
                        .AddField(
                            "Build Your Watchlist",
                            $"Use the /price-subscribe command to subscribe to some stocks price changes"
                        ) // Directly use the user-friendly message from the exception
                        .WithColor(Color.Orange) // A warning/informational color
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
