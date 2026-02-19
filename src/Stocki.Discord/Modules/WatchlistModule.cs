using Discord;
using Discord.Interactions;
using MediatR;
using Microsoft.Extensions.Options;
using Stocki.Application.Commands.PriceSubscribe;
using Stocki.Application.Commands.PriceUnsubscribe;
using Stocki.Application.Queries.Subscription;
using Stocki.Domain.Models;
using Stocki.Domain.ValueObjects;
using Stocki.Shared.Config;

namespace Stocki.Discord.Modules;

public class WatchlistModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<WatchlistModule> _logger;
    private readonly IMediator _mediator;
    private readonly FinnhubWebsocketsSettings _settings;

    public WatchlistModule(ILogger<WatchlistModule> logger, IMediator m, IOptions<FinnhubWebsocketsSettings> settings)
    {
        _logger = logger;
        _mediator = m;
        _settings = settings.Value;
    }

    [SlashCommand("add-to-watchlist", "Watchlist a stock and get a DM if it moves +/-5%")]
    public async Task AddStockToWatchlistAsync(
        [Summary("ticker", "the ticker of the stock you want to watch to e.g. AAPL")]
      string ticker
        )
    {
        await DeferAsync();

        try
        {
            TickerSymbol symbol = new TickerSymbol(ticker);
            PriceSubscribeCommand command = new(symbol, Context.User.Id);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var isWatchlisted = await _mediator.Send(command, cts.Token);
            if (isWatchlisted)
            {
                await FollowupAsync(
                    embed: new EmbedBuilder()
                    .WithTitle($"{ticker} added to watchlist!") // Clear, user-centric title
                    .AddField("Success!", $"You have sucessfully watchlisted {ticker}")
                    .AddField(
                      "Info",
                      $"You will now recieve real time updates when {ticker} moves up or down {_settings.PriceChangePercentage}%"
                      )
                    .AddField(
                      "Want to remove from watchlist?",
                      "Just use the /remove-from-watchlist command!"
                      )
                    .WithColor(Color.Green) // A warning/informational color
                    .WithFooter("Stocki 2025")
                    .Build()
                    );
            }
            else
            {
                await FollowupAsync(
                    embed: new EmbedBuilder()
                    .WithTitle($"Cannot Watchlist {ticker}") // Clear, user-centric title
                    .AddField("Message", $"You cannot sucessfully watchlist {ticker}") // Directly use the user-friendly message from the exception
                    .AddField(
                      "Next Steps",
                      "Use the /list-stock-watchlist command to ensure you arent already watching this stock and try again"
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
        catch (Exception ex) when (ex is not OperationCanceledException) // Catch any other unexpected errors
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
        "list-stock-watchlist",
        "Lists all the stocks you are currently watching"
        )]
    public async Task ListStockWatchlistAsync()
    {
        await DeferAsync();

        try
        {
            ListPriceSubscriptionQuery Query = new(Context.User.Id);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            List<StockPriceSubscription> watchedStocks = await _mediator.Send(
                Query,
                cts.Token
                );

            if (watchedStocks.Count > 0)
            {
                var e = new EmbedBuilder()
                  .WithTitle("Watched Stocks") // Clear, user-centric title
                  .WithFooter("To unwatch a stock just use the remove-from-watchlist command!")
                  .WithColor(Color.Green);
                foreach (var s in watchedStocks)
                {
                    string fieldName = $"**{s.Ticker} - **";
                    string fieldValue = $"\n\n{s.CreatedDate}\n\n";
                    e.AddField(fieldName, fieldValue, true);
                }
                await FollowupAsync(embed: e.Build());
            }
            else
            {
                await FollowupAsync(
                    embed: new EmbedBuilder()
                    .WithTitle("No Watched Stocks Yet") // Clear, user-centric title
                    .AddField(
                      "Build Your Watchlist",
                      $"Use the /add-to-watchlist command to watch some stocks price changes"
                      ) // Directly use the user-friendly message from the exception
                    .WithColor(Color.Orange) // A warning/informational color
                    .WithFooter("Stocki 2025")
                    .Build()
                    );
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException) // Catch any other unexpected errors
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

    [SlashCommand("remove-from-watchlist", "Removes a stock from your watchlist")]
    public async Task RemoveFromWatchlistAsync(
        [Summary("ticker", "the ticker of the stock you want to remove from your watchlist e.g. AAPL")]
      string ticker
        )
    {
        await DeferAsync();

        try
        {
            TickerSymbol symbol = new TickerSymbol(ticker);
            PriceUnsubscribeCommand command = new(symbol, Context.User.Id);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var isNoLongerWatching = await _mediator.Send(command, cts.Token);
            if (isNoLongerWatching)
            {
                await FollowupAsync(
                    embed: new EmbedBuilder()
                    .WithTitle($"{ticker} removed from watchlist!")
                    .AddField("Success!", $"You have sucessfully removed {ticker} from your watchlist")
                    .AddField(
                      "Info",
                      $"You will now no longer recieve real time updates when {ticker} moves up or down {_settings.PriceChangePercentage}%"
                      )
                    .AddField(
                      "Want to re-watch?",
                      "To subscribe just use the watchlist command!"
                      )
                    .WithColor(Color.Green) // A warning/informational color
                    .WithFooter("Stocki 2025")
                    .Build()
                    );
            }
            else
            {
                await FollowupAsync(
                    embed: new EmbedBuilder()
                    .WithTitle("Cannot unwatch") // Clear, user-centric title
                    .AddField("Message", $"You cannot sucessfully remove {ticker} from your watchlist") // Directly use the user-friendly message from the exception
                    .AddField(
                      "Next Steps",
                      "Use the /list-stock-watchlist command to ensure you are actually watching this stock and try again"
                      )
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
        catch (Exception ex) when (ex is not OperationCanceledException) // Catch any other unexpected errors
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
