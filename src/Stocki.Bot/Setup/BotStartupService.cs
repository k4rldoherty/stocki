using System.Reflection; // Needed for Assembly.GetEntryAssembly()
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stocki.Bot.Chat;
using Stocki.Shared.Config;

namespace Stocki.Bot.Setup;

public class BotStartupService : BackgroundService
{
    private readonly ILogger<BotStartupService> _logger;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _serviceProvider;
    private readonly InputHandlerService _inputHandlerService; // If still needed
    private readonly IOptions<DiscordSettings> _discordSettings;

    public BotStartupService(
        ILogger<BotStartupService> logger,
        DiscordSocketClient client,
        InteractionService interactionService,
        IServiceProvider serviceProvider,
        InputHandlerService inputHandlerService,
        IOptions<DiscordSettings> discordSettings
    )
    {
        _logger = logger;
        _client = client;
        _interactionService = interactionService;
        _serviceProvider = serviceProvider;
        _inputHandlerService = inputHandlerService;
        _discordSettings = discordSettings;

        // Hook up logging events
        _client.Log += LogAsync;
        _interactionService.Log += LogAsync;

        // Hook up InteractionCreated event to InteractionService
        _client.InteractionCreated += HandleInteractionAsync;

        // Hook up MessageReceived
        _client.MessageReceived += _inputHandlerService.HandleMessageAsync; // If InputHandlerService handles generic messages
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var token = _discordSettings.Value.Token;
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogError("Token variable is not set.");
            throw new InvalidOperationException("Discord bot token is missing.");
        }

        _logger.LogInformation("Attempting login...");
        await _client.LoginAsync(TokenType.Bot, token);
        _logger.LogInformation("Login successful.");
        await _client.StartAsync();

        // This ensures commands are registered only AFTER the client is ready
        _client.Ready += ClientReadyAsync;

        // Keeps bot running infinitely
        try
        {
            _logger.LogInformation("Bot running");
            // Register a callback to stop the bot when the host stops
            stoppingToken.Register(async () =>
            {
                _logger.LogInformation("Stopping token received. Shutting down bot...");
                await _client.StopAsync();
            });
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Bot host stopped. Shutting down.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while the bot was running.");
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        // ExecuteAsync's stoppingToken.Register handles client.StopAsync() now.
        // Base class implementation is sufficient here.
        await base.StopAsync(stoppingToken);
    }

    private async Task ClientReadyAsync()
    {
        _logger.LogInformation("Discord client is ready.");

        // Clear existing commands if development flag is set
        if (
            bool.TryParse(
                Environment.GetEnvironmentVariable("RESETBOTCOMMANDS"),
                out var resetCommands
            ) && resetCommands
        )
        {
            _logger.LogWarning("RESETBOTCOMMANDS is true. Deleting all global commands...");
            await DeleteAllCommands(_client);
            // Small delay to allow Discord API to process deletions before re-registering
            await Task.Delay(500);
        }

        // Discover and add all modules from the current assembly (Stocki.Bot)
        // This scans for classes like StockCommands
        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        _logger.LogInformation(
            $"Discovered {_interactionService.Modules.Count} interaction modules."
        );

        // Register commands globally. This pushes the commands to Discord.
        try
        {
            await _interactionService.RegisterCommandsGloballyAsync(true);
            _logger.LogInformation("Registered global slash commands.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register global slash commands.");
        }

        // Unhook from Ready event after commands are registered to prevent re-registering on reconnects
        _client.Ready -= ClientReadyAsync;
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        // Create an InteractionContext from the client and interaction
        var context = new SocketInteractionContext(_client, interaction);

        try
        {
            // Execute the command via the InteractionService
            var result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);

            if (!result.IsSuccess && result.Error != InteractionCommandError.UnknownCommand)
            {
                _logger.LogWarning(
                    $"Command execution failed for '{interaction.Type}' ({interaction.Id}): {result.ErrorReason}"
                );
                // Provide user feedback for failures
                if (context.Interaction.HasResponded)
                {
                    await context.Interaction.FollowupAsync($"Error: {result.ErrorReason}");
                }
                else
                {
                    await context.Interaction.RespondAsync($"Error: {result.ErrorReason}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                $"Unhandled exception during interaction: {interaction.Type} ({interaction.Id})"
            );
            // Ensure the user gets a response even on unhandled exceptions
            if (context.Interaction.HasResponded)
            {
                await context.Interaction.FollowupAsync(
                    "An unexpected error occurred. Please try again later."
                );
            }
            else
            {
                await context.Interaction.RespondAsync(
                    "An unexpected error occurred. Please try again later."
                );
            }
        }
    }

    private Task LogAsync(LogMessage msg)
    {
        // Map Discord.Net's LogSeverity to Microsoft.Extensions.Logging levels
        var logLevel = msg.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            LogSeverity.Debug => LogLevel.Trace,
            _ => LogLevel.Information,
        };
        _logger.Log(logLevel, msg.Exception, msg.Message);
        return Task.CompletedTask;
    }

    public async Task DeleteAllCommands(DiscordSocketClient client)
    {
        var commands = await client.GetGlobalApplicationCommandsAsync();
        foreach (var cmd in commands)
        {
            await cmd.DeleteAsync();
        }
        _logger.LogInformation($"Deleted {commands.Count} global commands.");
    }
}
