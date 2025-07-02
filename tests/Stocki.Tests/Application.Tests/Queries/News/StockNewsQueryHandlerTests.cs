using System.Net;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Stocki.Application.Exceptions;
using Stocki.Application.Interfaces;
using Stocki.Application.Queries.News;
using Stocki.Domain.Models;
using Stocki.Domain.ValueObjects;
using Stocki.Shared.Models;

public class StockNewsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_ReturnsNewsArticles()
    {
        var clientMock = new Mock<IFinnhubClient>();
        var loggerMock = new Mock<ILogger<StockNewsQueryHandler>>();
        var handler = new StockNewsQueryHandler(loggerMock.Object, clientMock.Object);

        var tickerSymbol = new TickerSymbol("EXMPL");
        var query = new StockNewsQuery(tickerSymbol);
        var fakeNews = new Faker<StockNewsArticle>();
        List<StockNewsArticle> articles = fakeNews.Generate(3);
        clientMock
            .Setup(c =>
                c.GetCompanyNewsAsync(It.IsAny<StockNewsQuery>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(ApiResponse<List<StockNewsArticle>>.Success(articles, HttpStatusCode.OK));

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(articles, result);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsStockDataNotFoundException()
    {
        var clientMock = new Mock<IFinnhubClient>();
        var loggerMock = new Mock<ILogger<StockNewsQueryHandler>>();
        var handler = new StockNewsQueryHandler(loggerMock.Object, clientMock.Object);

        var tickerSymbol = new TickerSymbol("fake");
        var query = new StockNewsQuery(tickerSymbol);
        clientMock
            .Setup(c =>
                c.GetCompanyNewsAsync(It.IsAny<StockNewsQuery>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                ApiResponse<List<StockNewsArticle>>.Failure("Technical error", HttpStatusCode.OK)
            );

        var exception = await Assert.ThrowsAsync<StockDataNotFoundException>(
            () => handler.Handle(query, CancellationToken.None)
        );

        exception
            .UserFriendlyMessage.Should()
            .Contain($"Sorry, I couldn't find any data for '{query.Symbol.Value}'");
    }
}
