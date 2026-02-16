using Discord;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stocki.Domain.Interfaces;
using Stocki.Shared.Notifications;

namespace Stocki.NotificationService;

public class PriceMovedBeyondThresholdHandler
    : INotificationHandler<PriceMovedBeyondThresholdNotification>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DiscordSocketClient _discordClient;
    private readonly ILogger<PriceMovedBeyondThresholdHandler> _logger;

    public PriceMovedBeyondThresholdHandler(
        IServiceScopeFactory scopeFactory,
        DiscordSocketClient discordClient,
        ILogger<PriceMovedBeyondThresholdHandler> logger
    )
    {
        _scopeFactory = scopeFactory;
        _discordClient = discordClient;
        _logger = logger;
    }

    public async Task Handle(
        PriceMovedBeyondThresholdNotification notification,
        CancellationToken token
    )
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IStockPriceSubscriptionRepository>();
            var users = await repo.GetAllUsersSubscribedToAStock(notification.Symbol, token);
            // Message each user
            _logger.LogInformation(
                "Found {} users subscribed to {}",
                users.Count().ToString(),
                notification.Symbol
            );
            foreach (var u in users)
            {
                var userInfo = await _discordClient.GetUserAsync(u);
                if (userInfo == null)
                {
                    _logger.LogWarning("User info is null for user id {}", u);
                    continue;
                }
                var dmChannel = await userInfo.CreateDMChannelAsync();
                await dmChannel.SendMessageAsync(
                    embed: new EmbedBuilder()
                        .WithTitle($"Price Notification for {notification.Symbol}")
                        .AddField("New Price", $"${notification.Price}")
                        .AddField(
                            "Percent Change",
                            $"{String.Format("{0:0.00}", notification.PercentChange)}%"
                        )
                        .WithFooter("Stocki 2025")
                        .WithColor(Color.Green)
                        .Build()
                );
                _logger.LogInformation("Message sent to user {} successfully", u);
            }
        }
    }
}
