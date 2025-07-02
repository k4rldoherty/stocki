using System.Net;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Stocki.Application.Exceptions;
using Stocki.Application.Interfaces;
using Stocki.Application.Queries.Overview;
using Stocki.Domain.ValueObjects;
using Stocki.Shared.Models;

public class StockOverviewQueryHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_ReturnsOverview()
    {
        var clientMock = new Mock<IAlphaVantageClient>();
        var loggerMock = new Mock<ILogger<StockOverviewQueryHandler>>();
        var handler = new StockOverviewQueryHandler(clientMock.Object, loggerMock.Object);

        var tickerSymbol = new TickerSymbol("EXMPL");
        var query = new StockOverviewQuery(tickerSymbol);

        var fakeOverview = new Faker<StockOverview>();

        var testOverview = fakeOverview.Generate();

        clientMock
            .Setup(c =>
                c.GetStockOverviewAsync(
                    It.IsAny<StockOverviewQuery>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(ApiResponse<StockOverview>.Success(testOverview, HttpStatusCode.OK));

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(testOverview, result);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsPartialOverview()
    {
        var clientMock = new Mock<IAlphaVantageClient>();
        var loggerMock = new Mock<ILogger<StockOverviewQueryHandler>>();
        var handler = new StockOverviewQueryHandler(clientMock.Object, loggerMock.Object);

        var tickerSymbol = new TickerSymbol("EXMPL");
        var query = new StockOverviewQuery(tickerSymbol);

        var fakeOverview = new Faker<StockOverview>()
            .RuleFor(o => o.AnalystTargetPrice, f => f.Random.Bool(0.3f) ? null : decimal.One)
            .RuleFor(o => o.AnalystRatingStrongSell, f => f.Random.Bool(0.3f) ? null : 10)
            .RuleFor(o => o.AnalystRatingStrongBuy, f => f.Random.Bool(0.3f) ? null : 5);

        var testOverview = fakeOverview.Generate();

        clientMock
            .Setup(c =>
                c.GetStockOverviewAsync(
                    It.IsAny<StockOverviewQuery>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(ApiResponse<StockOverview>.Success(testOverview, HttpStatusCode.OK));

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(testOverview, result);
    }

    [Fact]
    public async Task Handle_ValidTickerNoData_ThrowsStockDataNotFoundException()
    {
        var clientMock = new Mock<IAlphaVantageClient>();
        var loggerMock = new Mock<ILogger<StockOverviewQueryHandler>>();
        var handler = new StockOverviewQueryHandler(clientMock.Object, loggerMock.Object);

        var tickerSymbol = new TickerSymbol("EXMPL");
        var query = new StockOverviewQuery(tickerSymbol);

        clientMock
            .Setup(c =>
                c.GetStockOverviewAsync(
                    It.IsAny<StockOverviewQuery>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(ApiResponse<StockOverview>.Failure("Technical error", HttpStatusCode.OK));

        var exception = await Assert.ThrowsAsync<StockDataNotFoundException>(
            () => handler.Handle(query, CancellationToken.None)
        );

        exception
            .UserFriendlyMessage.Should()
            .Contain($"Sorry, I couldn't find any data for '{query.Symbol.Value}'");
    }

    [Fact]
    public async Task Handle_ValidTickerTooManyRequests_ThrowsExternalServiceException()
    {
        var clientMock = new Mock<IAlphaVantageClient>();
        var loggerMock = new Mock<ILogger<StockOverviewQueryHandler>>();
        var handler = new StockOverviewQueryHandler(clientMock.Object, loggerMock.Object);

        var tickerSymbol = new TickerSymbol("EXMPL");
        var query = new StockOverviewQuery(tickerSymbol);

        clientMock
            .Setup(c =>
                c.GetStockOverviewAsync(
                    It.IsAny<StockOverviewQuery>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                ApiResponse<StockOverview>.Failure(
                    "Too many requests",
                    HttpStatusCode.TooManyRequests
                )
            );

        var exception = await Assert.ThrowsAsync<ExternalServiceException>(
            () => handler.Handle(query, CancellationToken.None)
        );

        exception
            .UserFriendlyMessage.Should()
            .Contain(
                $"Oops! I'm getting too many requests right now. Please try again in a minute."
            );
    }
}
