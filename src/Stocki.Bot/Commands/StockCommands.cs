using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Stocki.Application.Queries.Overview;
using Stocki.Domain.Models;
using Stocki.Domain.ValueObjects;

namespace Stocki.Bot.Commands;

public class StockCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<StockCommands> _logger;
    private readonly IStockOverviewQueryHandler _stockOverviewQueryHandler;

    public StockCommands(ILogger<StockCommands> logger, IStockOverviewQueryHandler soqh)
    {
        _logger = logger;
        _stockOverviewQueryHandler = soqh;
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
                embed: e.WithTitle($"{StockOverview.Name} ({StockOverview.Symbol})")
                    .WithDescription(StockOverview.Description)
                    .AddField("Sector", StockOverview.Sector)
                    .AddField("Industry", StockOverview.Industry)
                    .AddField("Market Cap", StockOverview.MarketCapitalization)
                    .AddField("P/E Ratio", StockOverview.PERatio)
                    .AddField("Dividend Yield", StockOverview.DividendYield)
                    .AddField("52 Week High", StockOverview.FiftyTwoWeekHigh)
                    .AddField("52 Week Low", StockOverview.FiftyTwoWeekLow)
                    .WithColor(Color.Blue)
                    .WithFooter("Data provided by AlphaVantage")
                    .Build()
            );
        }
    }
}
