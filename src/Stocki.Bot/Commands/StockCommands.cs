using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

// using Stocki.Application;

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
        _logger.LogInformation($"Recieved /overview command for ticker {ticker}");
    }

    private async Task HandleGetStockPriceDataAsync(SocketSlashCommand cmd)
    {
        var ticker = (string)cmd.Data.Options.FirstOrDefault();
        if (string.IsNullOrEmpty(ticker))
        {
            await cmd.RespondAsync("Ticker invalid");
            return;
        }
        if (ticker.Count() < 1 || ticker.Count() > 4)
        {
            await cmd.RespondAsync(
                "Please enter a ticker between 1 and 4 characters and try again"
            );
            return;
        }
        // TODO: Pass ticker to application layer to handle
    }

    private async Task HandleGetNewsAsync(SocketSlashCommand cmd)
    {
        var ticker = cmd.Data.Options.FirstOrDefault();
        if (ticker is null)
        {
            return;
        }
        await Task.CompletedTask;
    }
}
