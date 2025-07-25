using MediatR;
using Stocki.Domain.ValueObjects;

namespace Stocki.Application.Commands.PriceUnsubscribe;

public record PriceUnsubscribeCommand : IRequest<bool>
{
    public TickerSymbol Symbol { get; set; }
    public ulong DiscordId { get; set; }

    public PriceUnsubscribeCommand(TickerSymbol s, ulong discordId)
    {
        if (s == null)
        {
            throw new ArgumentNullException("TickerSymbol object must be provided");
        }
        Symbol = s;
        DiscordId = discordId;
    }
}
