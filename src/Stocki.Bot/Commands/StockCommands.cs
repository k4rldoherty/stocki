using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Stocki.Application.Queries;
using Stocki.Domain.ValueObjects;

namespace Stocki.Bot.Commands;

public class StockCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<StockCommands> _logger;

    public StockCommands(ILogger<StockCommands> logger)
    {
        _logger = logger;
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
    }
}
