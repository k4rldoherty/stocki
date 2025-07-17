using System.Net;
using Bogus;
using Microsoft.Extensions.Logging;
using Moq;
using Stocki.Application.Commands.PriceSubscribe;
using Stocki.Application.Interfaces;
using Stocki.Application.Queries.Price;
using Stocki.Domain.Interfaces;
using Stocki.Domain.Models;
using Stocki.Domain.ValueObjects;
using Stocki.Shared.Models;

public class PriceSubscribeCommandHandlerTests
{
    [Fact]
    public async Task Handle_SuccessfulSubscription()
    {
        var mockRepo = new Mock<IStockPriceSubscriptionRepository>();
        var mockLogger = new Mock<ILogger<PriceSubscribeCommandHandler>>();
        var mockClient = new Mock<IFinnhubClient>();
        var handler = new PriceSubscribeCommandHandler(
            mockLogger.Object,
            mockRepo.Object,
            mockClient.Object
        );
        var fakeQuote = new Faker<StockQuote>();
        StockQuote quote = fakeQuote.Generate();
        TickerSymbol Symbol = new("test");
        ulong DiscordId = 1019292920202929;
        var query = new PriceSubscribeCommand(Symbol, DiscordId);
        mockClient
            .Setup(c =>
                c.GetStockQuoteAsync(It.IsAny<StockQuoteQuery>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(ApiResponse<StockQuote>.Success(quote, HttpStatusCode.OK));
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
        var mockClient = new Mock<IFinnhubClient>();
        var handler = new PriceSubscribeCommandHandler(
            mockLogger.Object,
            mockRepo.Object,
            mockClient.Object
        );
        TickerSymbol Symbol = new("test");
        ulong DiscordId = 1019292920202929;
        var query = new PriceSubscribeCommand(Symbol, DiscordId);
        mockClient
            .Setup(c =>
                c.GetStockQuoteAsync(It.IsAny<StockQuoteQuery>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(ApiResponse<StockQuote>.Failure("fail", HttpStatusCode.OK));
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
