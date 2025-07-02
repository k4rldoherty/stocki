using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Stocki.Application.Exceptions;
using Stocki.Application.Interfaces;
using Stocki.Application.Queries.Overview;
using Stocki.Domain.Models;
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

        var testOverview = new StockOverview(
            Name: "Example Corp",
            Symbol: "EXMPL",
            Description: "A fictional company used for testing purposes.",
            AssetType: "Common Stock",
            Currency: "USD",
            Sector: "Technology",
            Industry: "Software",
            FiscalYearEnd: "December",
            LatestQuarter: new DateTime(2025, 3, 31),
            MarketCapitalization: 50000000000L,
            EBITDA: 10000000000L,
            PERatio: 25.5m,
            PEGRatio: 1.2m,
            BookValue: 10.5m,
            DividendPerShare: 0.50m,
            DividendYield: 0.01m,
            EPS: 2.5m,
            RevenuePerShareTTM: 10.0m,
            ProfitMargin: 0.25m,
            OperatingMarginTTM: 0.30m,
            ReturnOnAssetsTTM: 0.10m,
            ReturnOnEquityTTM: 0.20m,
            RevenueTTM: 40000000000L,
            GrossProfitTTM: 25000000000L,
            DilutedEPSTTM: 2.45m,
            QuarterlyEarningsGrowthYOY: 0.15m,
            QuarterlyRevenueGrowthYOY: 0.12m,
            AnalystTargetPrice: 120.0m,
            AnalystRatingStrongBuy: 5,
            AnalystRatingBuy: 3,
            AnalystRatingHold: 2,
            AnalystRatingSell: 0,
            AnalystRatingStrongSell: 0,
            TrailingPE: 24.0m,
            ForwardPE: 22.0m,
            PriceToSalesRatioTTM: 5.0m,
            PriceToBookRatio: 2.5m,
            EVToRevenue: 4.8m,
            EVToEBITDA: 15.0m,
            Beta: 0.9m,
            FiftyTwoWeekHigh: 125.0m,
            FiftyTwoWeekLow: 80.0m,
            FiftyDayMovingAverage: 110.0m,
            TwoHundredDayMovingAverage: 100.0m,
            SharesOutstanding: 1000000000L,
            DividendDate: new DateTime(2025, 7, 1),
            ExDividendDate: new DateTime(2025, 6, 20)
        );

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

        var testOverview = new StockOverview(
            Name: "Example Corp",
            Symbol: "EXMPL",
            Description: "A fictional company used for testing purposes.",
            AssetType: "Common Stock",
            Currency: "USD",
            Sector: "Technology",
            Industry: "Software",
            FiscalYearEnd: "December",
            LatestQuarter: new DateTime(2025, 3, 31),
            MarketCapitalization: 50000000000L,
            EBITDA: 10000000000L,
            PERatio: 25.5m,
            PEGRatio: 1.2m,
            BookValue: 10.5m,
            DividendPerShare: 0.50m,
            DividendYield: 0.01m,
            EPS: 2.5m,
            RevenuePerShareTTM: 10.0m,
            ProfitMargin: 0.25m,
            OperatingMarginTTM: 0.30m,
            ReturnOnAssetsTTM: 0.10m,
            ReturnOnEquityTTM: 0.20m,
            RevenueTTM: 40000000000L,
            GrossProfitTTM: 25000000000L,
            DilutedEPSTTM: 2.45m,
            QuarterlyEarningsGrowthYOY: 0.15m,
            QuarterlyRevenueGrowthYOY: 0.12m,
            AnalystTargetPrice: 120.0m,
            AnalystRatingStrongBuy: 5,
            AnalystRatingBuy: null,
            AnalystRatingHold: null,
            AnalystRatingSell: null,
            AnalystRatingStrongSell: null,
            TrailingPE: 24.0m,
            ForwardPE: 22.0m,
            PriceToSalesRatioTTM: 5.0m,
            PriceToBookRatio: 2.5m,
            EVToRevenue: 4.8m,
            EVToEBITDA: 15.0m,
            Beta: 0.9m,
            FiftyTwoWeekHigh: 125.0m,
            FiftyTwoWeekLow: 80.0m,
            FiftyDayMovingAverage: 110.0m,
            TwoHundredDayMovingAverage: 100.0m,
            SharesOutstanding: 1000000000L,
            DividendDate: null,
            ExDividendDate: null
        );

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
