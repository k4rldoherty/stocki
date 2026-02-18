using MediatR;
using Stocki.Domain.ValueObjects;

namespace Stocki.Application.Commands.PriceUnsubscribe;

public record PriceUnsubscribeCommand : IRequest<bool>
{
    public TickerSymbol Symbol { get; set; }
    public ulong DiscordId { get; set; }

    public PriceUnsubscribeCommand(TickerSymbol s, ulong discordId)
    {
        Symbol = s;
        DiscordId = discordId;
    }
}
