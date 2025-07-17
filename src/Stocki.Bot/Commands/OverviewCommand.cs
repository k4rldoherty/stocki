using Discord;
using Discord.Interactions;
using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.Application.Exceptions;
using Stocki.Application.Queries.Overview;
using Stocki.Domain.ValueObjects;

namespace Stocki.Bot.Commands;

public class OverviewCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<OverviewCommand> _logger;
    private readonly IMediator _mediator;

    public OverviewCommand(ILogger<OverviewCommand> logger, IMediator m)
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
}
