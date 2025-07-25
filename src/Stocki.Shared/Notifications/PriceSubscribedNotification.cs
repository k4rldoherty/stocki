using MediatR;

namespace Stocki.Shared.Notifications;

public record PriceSubscribedNotification : INotification
{
    public string Symbol { get; set; }
    public ulong DiscordId { get; set; }

    public PriceSubscribedNotification(string symbol, ulong discordId)
    {
        Symbol = symbol;
        DiscordId = discordId;
    }
}
