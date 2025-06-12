using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Stocki.Application.Queries.Overview;
using Stocki.Application.Queries.Price;
using Stocki.Domain.Models;
using Stocki.Domain.ValueObjects;

namespace Stocki.Bot.Commands;

public class StockCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<StockCommands> _logger;
    private readonly StockOverviewQueryHandler _stockOverviewQueryHandler;
    private readonly StockQuoteQueryHandler _stockQuoteQueryHandler;

    public StockCommands(
        ILogger<StockCommands> logger,
        StockQuoteQueryHandler qh,
        StockOverviewQueryHandler oh
    )
    {
        _logger = logger;
        _stockOverviewQueryHandler = oh;
        _stockQuoteQueryHandler = qh;
    }

    // Handles the overview command
    [SlashCommand("overview", "Provides an in depth financial overview of a stock")]
    public async Task HandleGetStockOverviewAsync(
        [Summary("ticker", "the ticker of the stock you want an overview of e.g. AAPL")]
            string ticker
    )
    {
        await DeferAsync();
        if (string.IsNullOrEmpty(ticker) || ticker.Length < 1 || ticker.Length > 5)
        {
            await FollowupAsync(
                "Please try again and enter a ticker between 1-4 characters in length"
            );
            return;
        }
        // TODO: Make query object and Ticker Param and pass to application layer for processing
        TickerSymbol s = new TickerSymbol(ticker);
        StockOverviewQuery q = new(s);
        _logger.LogInformation($"Recieved /overview command for ticker {ticker}");
        StockOverview? StockOverview = await _stockOverviewQueryHandler.HandleAsync(q);
        var e = new EmbedBuilder();
        try
        {
            if (StockOverview == null)
            {
                _logger.LogWarning($"The request failed");
                await FollowupAsync(
                    embed: e.WithTitle("Error")
                        .AddField(
                            "Why?",
                            "Check the ticker of the stock you are looking for is correct, or try again"
                        )
                        .WithFooter("Stocki 2025")
                        .WithColor(Color.Red)
                        .Build()
                );
            }
            else
            {
                _logger.LogInformation($"The request succeeded");
                await FollowupAsync(
                    embed: e.WithTitle($"{StockOverview.Name} {StockOverview.Symbol})")
                        .WithDescription(StockOverview.Description)
                        .AddField("Sector", StockOverview.Sector)
                        .AddField("Industry", StockOverview.Industry)
                        .AddField("Market Cap", StockOverview.MarketCapitalization)
                        .AddField("P/E Ratio", StockOverview.PERatio)
                        .AddField(
                            "Dividend Yield",
                            StockOverview.DividendYield != null
                                ? StockOverview.DividendYield
                                : "No Dividend for this company"
                        )
                        .AddField("52 Week High", StockOverview.FiftyTwoWeekHigh)
                        .AddField("52 Week Low", StockOverview.FiftyTwoWeekLow)
                        .WithColor(Color.Blue)
                        .WithFooter("Data provided by AlphaVantage")
                        .Build()
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error building message: {ex.Message}");
        }
    }

    [SlashCommand("quote", "Provides a quote view of a stock")]
    public async Task HandleGetQuoteAsync(
        [Summary("ticker", "the ticker of the stock you want an overview of e.g. AAPL")]
            string ticker
    )
    {
        await DeferAsync();
        if (string.IsNullOrEmpty(ticker) || ticker.Length < 1 || ticker.Length > 5)
        {
            await FollowupAsync(
                "Please try again and enter a ticker between 1-4 characters in length"
            );
            return;
        }
        // TODO: Make query object and Ticker Param and pass to application layer for processing
        TickerSymbol s = new TickerSymbol(ticker);
        StockQuoteQuery q = new(s);
        _logger.LogInformation($"Recieved /quote command for ticker {ticker}");
        StockQuote? Quote = await _stockQuoteQueryHandler.HandleAsync(q);
        var e = new EmbedBuilder();
        try
        {
            if (Quote == null)
            {
                _logger.LogWarning($"The request failed");
                await FollowupAsync(
                    embed: e.WithTitle("Error")
                        .AddField(
                            "Why?",
                            "Check the ticker of the stock you are looking for is correct, or try again"
                        )
                        .WithFooter("Stocki 2025")
                        .WithColor(Color.Red)
                        .Build()
                );
            }
            else
            {
                _logger.LogInformation($"The request succeeded");
                await FollowupAsync(
                    embed: e.WithTitle($"{Quote.Ticker} - Stock Quote {DateTime.Now}")
                        .AddField("Current Price", $"${Quote.CurrentPrice}")
                        .AddField("Opening Price", $"${Quote.OpeningPrice}")
                        .AddField("Closing Price", $"${Quote.ClosingPrice}")
                        .AddField("High", $"${Quote.High}")
                        .AddField("Low", $"${Quote.Low}")
                        .WithColor(Color.Blue)
                        .WithFooter("Data provided by Finnhub")
                        .Build()
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error building message: {ex.Message}");
        }
    }
}
