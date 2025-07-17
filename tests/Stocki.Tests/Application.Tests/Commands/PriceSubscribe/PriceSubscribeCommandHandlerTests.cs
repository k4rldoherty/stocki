using Microsoft.Extensions.Logging;
using Moq;
using Stocki.Application.Commands.PriceSubscribe;
using Stocki.Domain.Interfaces;
using Stocki.Domain.Models;
using Stocki.Domain.ValueObjects;

public class PriceSubscribeCommandHandlerTests
{
    [Fact]
    public async Task Handle_SuccessfulSubscription()
    {
        var mockRepo = new Mock<IStockPriceSubscriptionRepository>();
        var mockLogger = new Mock<ILogger<PriceSubscribeCommandHandler>>();
        var handler = new PriceSubscribeCommandHandler(mockLogger.Object, mockRepo.Object);
        TickerSymbol Symbol = new("test");
        ulong DiscordId = 1019292920202929;
        var query = new PriceSubscribeCommand(Symbol, DiscordId);
        mockRepo
            .Setup(r =>
                r.AddSubscriptionAsync(
                    It.IsAny<StockPriceSubscription>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);
        var result = await handler.Handle(query, CancellationToken.None);
        Assert.True(result);
    }

    [Fact]
    public async Task Handle_UnsuccessfulSubscription()
    {
        var mockRepo = new Mock<IStockPriceSubscriptionRepository>();
        var mockLogger = new Mock<ILogger<PriceSubscribeCommandHandler>>();
        var handler = new PriceSubscribeCommandHandler(mockLogger.Object, mockRepo.Object);
        TickerSymbol Symbol = new("test");
        ulong DiscordId = 1019292920202929;
        var query = new PriceSubscribeCommand(Symbol, DiscordId);
        mockRepo
            .Setup(r =>
                r.AddSubscriptionAsync(
                    It.IsAny<StockPriceSubscription>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);
        var result = await handler.Handle(query, CancellationToken.None);
        Assert.False(result);
    }
}
