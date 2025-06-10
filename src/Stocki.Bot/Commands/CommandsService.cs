using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Stocki.Bot.Commands;

public class CommandService
{
    private Dictionary<string, Func<SocketSlashCommand, Task>> commands = new();
    private readonly ILogger<CommandService> _logger;

    public CommandService(ILogger<CommandService> logger)
    {
        _logger = logger;
        commands.Add("get-price-info", HandleGetStockPriceInfoAsync);
        commands.Add("get-stock-info", HandleGetStockInfoAsync);
        commands.Add("get-news", HandleGetNewsAsync);
        commands.Add("subscribe", HandleSubscribeAsync);
        commands.Add("unsubscribe", HandleUnsubscribeAsync);
    }

    public async Task HandleCommandAsync(SocketSlashCommand cmd)
    {
        if (commands.TryGetValue(cmd.CommandName, out var f))
        {
            // TODO: SOMETHING WRONG HERE
            _logger.LogInformation($"RUNNING COMMAND: {cmd.CommandName}");
            await f(cmd);
        }
        _logger.LogInformation($"{cmd.CommandName} executed @ {DateTime.Now}");
    }

    public async Task HandleGetStockPriceInfoAsync(SocketSlashCommand cmd)
    {
        var tickerOption = cmd.Data.Options.FirstOrDefault();
        if (tickerOption == null)
        {
            await cmd.RespondAsync("Ticker invalid");
            return;
        }
    }

    private async Task HandleGetStockInfoAsync(SocketSlashCommand cmd)
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

    private async Task HandleSubscribeAsync(SocketSlashCommand cmd)
    {
        var args = cmd.Data.Options.ToList();
        if (args is null || args.Count() != 3)
        {
            return;
        }
        await Task.CompletedTask;
    }

    private async Task HandleUnsubscribeAsync(SocketSlashCommand cmd)
    {
        var ticker = cmd.Data.Options.FirstOrDefault();
        if (ticker is null)
        {
            return;
        }
        await Task.CompletedTask;
    }
}
