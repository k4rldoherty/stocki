using MediatR;

namespace Stocki.Shared.Notifications;

public record PriceUnsubscribedNotification : INotification
{
    public string Symbol { get; set; }
    public ulong DiscordId { get; set; }

    public PriceUnsubscribedNotification(string symbol, ulong discordId)
    {
        Symbol = symbol;
        DiscordId = discordId;
    }
}
