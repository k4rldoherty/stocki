using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Stocki.Bot.Commands;

public class CommandRegistry(ILogger<CommandRegistry> logger)
{
    public async Task InitializeCommandsAsync(DiscordSocketClient client)
    {
        var globalCommands = await client.GetGlobalApplicationCommandsAsync();
        try
        {
            if (!globalCommands.Any(x => x.Name.Equals("get-price-info")))
            {
                var priceCommand = new SlashCommandBuilder()
                    .WithName("stock-price")
                    .WithDescription(
                        "Gets the price of a stock by passing in a ticker eg 'PLTR' after the command"
                    )
                    .AddOption(
                        "ticker",
                        ApplicationCommandOptionType.String,
                        "The symbol to get the price of a stock",
                        true
                    )
                    .Build();

                await client.CreateGlobalApplicationCommandAsync(priceCommand);
                logger.LogInformation($"{priceCommand.Name} registered");
            }
            if (!globalCommands.Any(x => x.Name == "get-stock-info"))
            {
                var infoCommand = new SlashCommandBuilder()
                    .WithName("info")
                    .WithDescription(
                        "Retrives information about a stock by passing in a ticker eg 'PLTR'"
                    )
                    .AddOption(
                        "ticker",
                        ApplicationCommandOptionType.String,
                        "The ticker symbol of a stock",
                        true
                    )
                    .Build();

                await client.CreateGlobalApplicationCommandAsync(infoCommand);
                logger.LogInformation($"{infoCommand.Name} registered");
            }
            if (!globalCommands.Any(x => x.Name == "get-news"))
            {
                var newsCommand = new SlashCommandBuilder()
                    .WithName("news")
                    .WithDescription(
                        "Get the lates news articles about a stock from a number of sources"
                    )
                    .AddOption(
                        "ticker",
                        ApplicationCommandOptionType.String,
                        "The ticker symbol of the stock you want to subscribe to news stories about",
                        true
                    )
                    .Build();
                await client.CreateGlobalApplicationCommandAsync(newsCommand);
                logger.LogInformation($"{newsCommand.Name} registered");
            }
            if (!globalCommands.Any(x => x.Name == "subscribe"))
            {
                var opt = new SlashCommandOptionBuilder[]
                {
                    new SlashCommandOptionBuilder()
                        .WithName("ticker")
                        .WithDescription("The ticker symbol you want to subscribe to")
                        .WithType(ApplicationCommandOptionType.String)
                        .WithRequired(true),
                    new SlashCommandOptionBuilder()
                        .WithName("price-change")
                        .WithDescription(
                            "If the price moves up/down this % you will recieve an update"
                        )
                        .WithType(ApplicationCommandOptionType.Number)
                        .WithRequired(true),
                    new SlashCommandOptionBuilder()
                        .WithName("news-updates")
                        .WithDescription("Select true if you want to subscribe to news updates")
                        .WithType(ApplicationCommandOptionType.Boolean)
                        .WithRequired(true),
                };
                var subscribeCommand = new SlashCommandBuilder()
                    .WithName("subscribe")
                    .WithDescription("Subscribe to price changes and/or latest news in a stock")
                    .AddOptions(opt)
                    .Build();
                await client.CreateGlobalApplicationCommandAsync(subscribeCommand);
                logger.LogInformation($"{subscribeCommand.Name} registered");
            }

            if (!globalCommands.Any(x => x.Name == "unsubscribe"))
            {
                var unsubscribeCommand = new SlashCommandBuilder()
                    .WithName("unsubscribe")
                    .WithDescription(
                        "Unsubscribes the user to price changes and latest news in a stock, either by email, message or both."
                    )
                    .AddOption(
                        "ticker",
                        ApplicationCommandOptionType.String,
                        "The ticker symbol of the stock you want to unsubscribe to notifications about",
                        true
                    )
                    .Build();
                await client.CreateGlobalApplicationCommandAsync(unsubscribeCommand);
                logger.LogInformation($"{unsubscribeCommand.Name} registered");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex.Message);
            throw;
        }
        // More commands here ...
    }
}
