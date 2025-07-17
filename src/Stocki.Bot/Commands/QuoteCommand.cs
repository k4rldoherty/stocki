using Discord;
using Discord.Interactions;
using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.Application.Exceptions;
using Stocki.Application.Queries.Quote;
using Stocki.Domain.Models;
using Stocki.Domain.ValueObjects;

namespace Stocki.Bot.Commands;

public class QuoteCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<QuoteCommand> _logger;
    private readonly IMediator _mediator;

    public QuoteCommand(ILogger<QuoteCommand> logger, IMediator m)
    {
        _logger = logger;
        _mediator = m;
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
            TickerSymbol symbol = new TickerSymbol(ticker);
            StockQuoteQuery query = new(symbol);

            _logger.LogInformation("Received /quote command for ticker {Ticker}", ticker);

            StockQuote? quote = await _mediator.Send(query, CancellationToken.None);

            var embedBuilder = new EmbedBuilder();

            if (quote is not null)
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
        catch (StockDataNotFoundException ex)
        {
            await FollowupAsync(
                embed: new EmbedBuilder()
                    .WithTitle("Quote Not Found") // Clear, user-centric title
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
