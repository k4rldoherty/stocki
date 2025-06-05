using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stocki.Bot.Chat;
using Stocki.Bot.Commands;

namespace Stocki.Bot.Setup;

public class BotService(
    ILogger<BotService> logger,
    DiscordSocketClient client,
    InputHandlerService inputHandlerService,
    CommandService commandsService,
    CommandRegistry commandRegistry
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // TODO: make this more production-esque
        var token =
            Environment.GetEnvironmentVariable("DISCORDTOKEN")
            ?? throw new NullReferenceException("Cannot find discord token");

        // Login and start the bot
        logger.LogInformation("Attempting login...");
        await client.LoginAsync(TokenType.Bot, token);
        logger.LogInformation("Login successful...");
        await client.StartAsync();

        // Delete all commands to reset the bot
        if (bool.TryParse(Environment.GetEnvironmentVariable("RESETBOTCOMMANDS"), out var res))
        {
            if (res)
            {
                await Task.Delay(2000);
                await DeleteAllCommands(client);
            }
        }

        // Handle Messages
        client.MessageReceived += inputHandlerService.HandleMessageAsync;

        // Initialize commands
        client.Ready += async () => await commandRegistry.InitializeCommandsAsync(client);
        logger.LogInformation("Commands Registered");

        // Handle slash commands
        client.SlashCommandExecuted += commandsService.HandleCommandAsync;

        // Keeps bot running infinitely
        try
        {
            logger.LogInformation("Bot running");
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogWarning("Bot manually stopped. Shutting down");
            logger.LogWarning(ex.Message);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await client.StopAsync();
        await base.StopAsync(stoppingToken);
    }

    public async Task DeleteAllCommands(DiscordSocketClient client)
    {
        var commands = await client.GetGlobalApplicationCommandsAsync();
        foreach (var cmd in commands)
        {
            await cmd.DeleteAsync();
        }
        logger.LogInformation("All Commands Deleted");
    }
}
