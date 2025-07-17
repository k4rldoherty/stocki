using System.Net;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Stocki.Application.Exceptions;
using Stocki.Application.Interfaces;
using Stocki.Application.Queries.Quote;
using Stocki.Domain.Models;
using Stocki.Domain.ValueObjects;
using Stocki.Shared.Models;

public class StockQuoteQueryHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_ReturnsNewsArticles()
    {
        var clientMock = new Mock<IFinnhubClient>();
        var loggerMock = new Mock<ILogger<StockQuoteQueryHandler>>();
        var handler = new StockQuoteQueryHandler(clientMock.Object, loggerMock.Object);

        var tickerSymbol = new TickerSymbol("EXMPL");
        var query = new StockQuoteQuery(tickerSymbol);
        var fakeNews = new Faker<StockQuote>();
        List<StockQuote> quotes = fakeNews.Generate(3);
        foreach (var quote in quotes)
        {
            clientMock
                .Setup(c =>
                    c.GetStockQuoteAsync(It.IsAny<StockQuoteQuery>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(ApiResponse<StockQuote>.Success(quote, HttpStatusCode.OK));

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Equal(quote, result);
        }
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsStockDataNotFoundException()
    {
        var clientMock = new Mock<IFinnhubClient>();
        var loggerMock = new Mock<ILogger<StockQuoteQueryHandler>>();
        var handler = new StockQuoteQueryHandler(clientMock.Object, loggerMock.Object);

        var tickerSymbol = new TickerSymbol("fake");
        var query = new StockQuoteQuery(tickerSymbol);
        clientMock
            .Setup(c =>
                c.GetStockQuoteAsync(It.IsAny<StockQuoteQuery>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(ApiResponse<StockQuote>.Failure("Technical error", HttpStatusCode.OK));

        var exception = await Assert.ThrowsAsync<StockDataNotFoundException>(
            () => handler.Handle(query, CancellationToken.None)
        );

        exception
            .UserFriendlyMessage.Should()
            .Contain($"Sorry, I couldn't find any data for '{query.Symbol.Value}'");
    }
}
