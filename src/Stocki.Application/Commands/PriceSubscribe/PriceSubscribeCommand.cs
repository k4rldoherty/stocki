using MediatR;
using Stocki.Domain.ValueObjects;

namespace Stocki.Application.Commands.PriceSubscribe;

public record PriceSubscribeCommand : IRequest<bool>
{
    public TickerSymbol Symbol { get; set; }
    public ulong DiscordId { get; set; }

    public PriceSubscribeCommand(TickerSymbol s, ulong discordId)
    {
        if (s == null)
        {
            throw new ArgumentNullException("TickerSymbol object must be provided");
        }
        Symbol = s;
        DiscordId = discordId;
    }
}
