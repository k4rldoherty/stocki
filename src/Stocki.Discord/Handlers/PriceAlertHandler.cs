using Discord;
using Discord.WebSocket;
using Stocki.Domain.Interfaces;
using Stocki.Shared.Notifications;
using MediatR;
namespace Stocki.Discord.Handlers;

public class PriceAlertHandler : INotificationHandler<PriceAlertNotification>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DiscordSocketClient _discordClient;
    private readonly ILogger<PriceAlertHandler> _logger;

    public PriceAlertHandler(IServiceScopeFactory serviceScopeFactory, DiscordSocketClient discordSocketClient, ILogger<PriceAlertHandler> logger)
    {
        _scopeFactory = serviceScopeFactory;
        _discordClient = discordSocketClient;
        _logger = logger;
    }

    public async Task Handle(PriceAlertNotification notification,
        CancellationToken token)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IStockPriceSubscriptionRepository>();
            var users = await repo.GetAllUsersSubscribedToAStock(notification.Symbol, token);
            foreach (var u in users)
            {
                try
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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending message to user {User}", u);
                }
            }
        }
    }
}
