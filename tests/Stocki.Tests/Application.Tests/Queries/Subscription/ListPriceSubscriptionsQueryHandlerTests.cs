using Bogus;
using Microsoft.Extensions.Logging;
using Moq;
using Stocki.Application.Queries.Subscription;
using Stocki.Domain.Interfaces;
using Stocki.Domain.Models;

public class ListPriceSubscriptionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_NoSubscriptions()
    {
        var mockRepo = new Mock<IStockPriceSubscriptionRepository>();
        var mockLogger = new Mock<ILogger<ListPriceSubscriptionsQueryHandler>>();
        var handler = new ListPriceSubscriptionsQueryHandler(mockLogger.Object, mockRepo.Object);
        ulong DiscordId = 1019292920202929;
        var query = new ListPriceSubscriptionQuery(DiscordId);
        mockRepo
            .Setup(r =>
                r.GetAllSubscriptionsForUserAsync(It.IsAny<ulong>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new List<StockPriceSubscription>());
        var result = await handler.Handle(query, CancellationToken.None);
        Assert.Equal(new List<StockPriceSubscription>(), result);
    }

    [Fact]
    public async Task Handle_HasSubscriptions()
    {
        var mockRepo = new Mock<IStockPriceSubscriptionRepository>();
        var mockLogger = new Mock<ILogger<ListPriceSubscriptionsQueryHandler>>();
        var handler = new ListPriceSubscriptionsQueryHandler(mockLogger.Object, mockRepo.Object);
        ulong DiscordId = 1019292920202929;
        var query = new ListPriceSubscriptionQuery(DiscordId);
        var fakeSubscriptions = new Faker<StockPriceSubscription>();
        List<StockPriceSubscription> Subscriptions = fakeSubscriptions.Generate(3);
        mockRepo
            .Setup(r =>
                r.GetAllSubscriptionsForUserAsync(It.IsAny<ulong>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(Subscriptions);
        var result = await handler.Handle(query, CancellationToken.None);
        Assert.Equal(Subscriptions, result);
    }
}
