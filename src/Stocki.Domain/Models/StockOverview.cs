namespace Stocki.Domain.Models;

public record StockOverview(
    string? Name,
    string Symbol, // Assuming Symbol is always present and a string
    string? Description,
    string? AssetType,
    string? Currency,
    string? Sector,
    string? Industry,
    string? FiscalYearEnd, // AlphaVantage returns this as a string like "December"
    DateTime? LatestQuarter, // Should be parsed to DateTime
    long? MarketCapitalization, // Should be parsed to long
    long? EBITDA, // Should be parsed to long
    decimal? PERatio, // Should be parsed to decimal
    decimal? PEGRatio, // Should be parsed to decimal
    decimal? BookValue, // Should be parsed to decimal
    decimal? DividendPerShare, // Should be parsed to decimal, handling "None"
    decimal? DividendYield, // Should be parsed to decimal, handling "None"
    decimal? EPS, // Should be parsed to decimal
    decimal? RevenuePerShareTTM, // Should be parsed to decimal
    decimal? ProfitMargin, // Should be parsed to decimal
    decimal? OperatingMarginTTM, // Should be parsed to decimal
    decimal? ReturnOnAssetsTTM, // Should be parsed to decimal
    decimal? ReturnOnEquityTTM, // Should be parsed to decimal
    long? RevenueTTM, // Should be parsed to long
    long? GrossProfitTTM, // Should be parsed to long
    decimal? DilutedEPSTTM, // Should be parsed to decimal
    decimal? QuarterlyEarningsGrowthYOY, // Should be parsed to decimal
    decimal? QuarterlyRevenueGrowthYOY, // Should be parsed to decimal
    decimal? AnalystTargetPrice, // Should be parsed to decimal
    int? AnalystRatingStrongBuy, // Should be parsed to int
    int? AnalystRatingBuy, // Should be parsed to int
    int? AnalystRatingHold, // Should be parsed to int
    int? AnalystRatingSell, // Should be parsed to int
    int? AnalystRatingStrongSell, // Should be parsed to int
    decimal? TrailingPE, // Should be parsed to decimal
    decimal? ForwardPE, // Should be parsed to decimal
    decimal? PriceToSalesRatioTTM, // Should be parsed to decimal
    decimal? PriceToBookRatio, // Should be parsed to decimal
    decimal? EVToRevenue, // Should be parsed to decimal
    decimal? EVToEBITDA, // Should be parsed to decimal
    decimal? Beta, // Should be parsed to decimal
    decimal? FiftyTwoWeekHigh, // Should be parsed to decimal
    decimal? FiftyTwoWeekLow, // Should be parsed to decimal
    decimal? FiftyDayMovingAverage, // Should be parsed to decimal
    decimal? TwoHundredDayMovingAverage, // Should be parsed to decimal
    long? SharesOutstanding, // Should be parsed to long
    DateTime? DividendDate, // Should be parsed to DateTime, handling "None"
    DateTime? ExDividendDate // Should be parsed to DateTime, handling "None"
);
