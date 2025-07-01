using Discord;
using Discord.Interactions;
using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.Application.Exceptions;
using Stocki.Application.Queries.Overview;
using Stocki.Application.Queries.Price;
using Stocki.Domain.Models;
using Stocki.Domain.ValueObjects;

namespace Stocki.Bot.Commands;

public class StockCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<StockCommands> _logger;
    private readonly IMediator _mediator;

    public StockCommands(ILogger<StockCommands> logger, IMediator m)
    {
        _logger = logger;
        _mediator = m;
    }

    [SlashCommand("overview", "Provides an in depth financial overview of a stock")]
    public async Task HandleGetStockOverviewAsync(
        [Summary("ticker", "the ticker of the stock you want an overview of e.g. AAPL")]
            string ticker
    )
    {
        await DeferAsync(); // Acknowledge command immediately

        try
        {
            var symbol = new TickerSymbol(ticker);
            StockOverviewQuery query = new(symbol);

            _logger.LogInformation("Received /overview command for ticker {Ticker}", ticker);

            StockOverview? stockOverview = await _mediator.Send(query, CancellationToken.None);

            if (stockOverview is not null)
            {
                _logger.LogInformation(
                    "The /overview request succeeded for ticker {Ticker}",
                    ticker
                );
                await FollowupAsync(
                    embed: new EmbedBuilder()
                        .WithTitle($"{stockOverview.Name} ({stockOverview.Symbol})")
                        .WithDescription(stockOverview.Description)
                        .AddField("Sector", stockOverview.Sector ?? "N/A")
                        .AddField("Industry", stockOverview.Industry ?? "N/A")
                        .AddField(
                            "Market Cap",
                            stockOverview.MarketCapitalization?.ToString("N0") ?? "N/A"
                        )
                        .AddField("P/E Ratio", stockOverview.PERatio?.ToString("N2") ?? "N/A")
                        .AddField(
                            "Dividend Yield",
                            stockOverview.DividendYield?.ToString("P2") ?? "N/A"
                        )
                        .AddField(
                            "52 Week High",
                            stockOverview.FiftyTwoWeekHigh?.ToString("C2") ?? "N/A"
                        ) // Format as currency
                        .AddField(
                            "52 Week Low",
                            stockOverview.FiftyTwoWeekLow?.ToString("C2") ?? "N/A"
                        ) // Format as currency
                        .WithColor(Color.Blue)
                        .WithFooter("Data provided by AlphaVantage")
                        .Build()
                );
            }
        }
        catch (StockDataNotFoundException ex)
        {
            await FollowupAsync(
                embed: new EmbedBuilder()
                    .WithTitle("Information Not Found") // Clear, user-centric title
                    .AddField("Message", ex.UserFriendlyMessage) // Directly use the user-friendly message from the exception
                    .WithColor(Color.Orange) // A warning/informational color
                    .WithFooter("Stocki 2025")
                    .Build()
            );
        }
        catch (ExternalServiceException ex)
        {
            await FollowupAsync(
                embed: new EmbedBuilder()
                    .WithTitle("Error")
                    .AddField("Message", ex.UserFriendlyMessage)
                    .WithColor(Color.Red) // Critical error color
                    .WithFooter("If this persists, please contact support.")
                    .Build()
            );
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

    [SlashCommand("quote", "Provides a quote view of a stock")]
    public async Task HandleGetQuoteAsync(
        [Summary("ticker", "the ticker of the stock you want an overview of e.g. AAPL")]
            string ticker
    )
    {
        await DeferAsync();

        try
        {
            // TickerSymbol constructor will now handle validation
            TickerSymbol symbol = new TickerSymbol(ticker);
            StockQuoteQuery query = new(symbol);

            _logger.LogInformation("Received /quote command for ticker {Ticker}", ticker);

            StockQuote? quote = await _mediator.Send(query, CancellationToken.None);

            var embedBuilder = new EmbedBuilder();

            if (quote == null)
            {
                _logger.LogWarning(
                    "The /quote request failed for ticker {Ticker}. No data found.",
                    ticker
                );
                await FollowupAsync(
                    embed: embedBuilder
                        .WithTitle("Error")
                        .WithDescription(
                            $"Could not retrieve quote for **{ticker.ToUpperInvariant()}**."
                        )
                        .AddField(
                            "Reason",
                            "The ticker symbol might be incorrect, or data is temporarily unavailable. Please check the ticker and try again."
                        )
                        .WithFooter("Stocki 2025")
                        .WithColor(Color.Red)
                        .Build()
                );
            }
            else
            {
                _logger.LogInformation("The /quote request succeeded for ticker {Ticker}", ticker);
                await FollowupAsync(
                    embed: embedBuilder
                        .WithTitle($"{quote.Ticker} - Stock Quote ({DateTime.Now})")
                        .AddField("Current Price", $"${quote.CurrentPrice:F2}")
                        .AddField("Opening Price", $"${quote.OpeningPrice:F2}")
                        .AddField("Previous Close", $"${quote.ClosingPrice:F2}")
                        .AddField("High", $"${quote.High:F2}")
                        .AddField("Low", $"${quote.Low:F2}")
                        .WithColor(Color.Blue)
                        .WithFooter("Data provided by Finnhub")
                        .Build()
                );
            }
        }
        catch (ArgumentException ex) // Catch validation errors from TickerSymbol or other ArgumentExceptions
        {
            _logger.LogWarning(ex, "Invalid ticker format for /quote command: {Ticker}", ticker);
            await FollowupAsync(
                embed: new EmbedBuilder()
                    .WithTitle("Input Error")
                    .WithDescription($"The ticker '{ticker}' is invalid. Reason: {ex.Message}")
                    .WithColor(Color.Orange)
                    .Build()
            );
        }
        catch (Exception ex) // Catch any other unexpected errors
        {
            _logger.LogError(
                ex,
                "An unexpected error occurred during /quote command for ticker: {Ticker}",
                ticker
            );
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
