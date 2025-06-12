using Newtonsoft.Json; // Use Newtonsoft.Json's attribute

namespace Stocki.Infrastructure.Clients.DTOs;

public record AVStockOverviewDTO(
    [JsonProperty("Symbol")] string? Symbol, // Changed to string?
    [JsonProperty("AssetType")] string? AssetType,
    [JsonProperty("Name")] string? Name, // Changed to string?
    [JsonProperty("Description")] string? Description,
    [JsonProperty("CIK")] string? CIK,
    [JsonProperty("Exchange")] string? Exchange,
    [JsonProperty("Currency")] string? Currency,
    [JsonProperty("Country")] string? Country,
    [JsonProperty("Sector")] string? Sector,
    [JsonProperty("Industry")] string? Industry,
    [JsonProperty("Address")] string? Address,
    [JsonProperty("OfficialSite")] string? OfficialSite,
    [JsonProperty("FiscalYearEnd")] string? FiscalYearEnd,
    [JsonProperty("LatestQuarter")] string? LatestQuarter, // Changed to string?
    [JsonProperty("MarketCapitalization")] string? MarketCapitalization, // Changed to string?
    [JsonProperty("EBITDA")] string? EBITDA, // Changed to string?
    [JsonProperty("PERatio")] string? PERatio, // Changed to string?
    [JsonProperty("PEGRatio")] string? PEGRatio, // Changed to string?
    [JsonProperty("BookValue")] string? BookValue, // Changed to string?
    [JsonProperty("DividendPerShare")] string? DividendPerShare,
    [JsonProperty("DividendYield")] string? DividendYield,
    [JsonProperty("EPS")] string? EPS, // Changed to string?
    [JsonProperty("RevenuePerShareTTM")] string? RevenuePerShareTTM, // Changed to string?
    [JsonProperty("ProfitMargin")] string? ProfitMargin, // Changed to string?
    [JsonProperty("OperatingMarginTTM")] string? OperatingMarginTTM, // Changed to string?
    [JsonProperty("ReturnOnAssetsTTM")] string? ReturnOnAssetsTTM, // Changed to string?
    [JsonProperty("ReturnOnEquityTTM")] string? ReturnOnEquityTTM, // Changed to string?
    [JsonProperty("RevenueTTM")] string? RevenueTTM, // Changed to string?
    [JsonProperty("GrossProfitTTM")] string? GrossProfitTTM, // Changed to string?
    [JsonProperty("DilutedEPSTTM")] string? DilutedEPSTTM, // Changed to string?
    [JsonProperty("QuarterlyEarningsGrowthYOY")] string? QuarterlyEarningsGrowthYOY, // Changed to string?
    [JsonProperty("QuarterlyRevenueGrowthYOY")] string? QuarterlyRevenueGrowthYOY, // Changed to string?
    [JsonProperty("AnalystTargetPrice")] string? AnalystTargetPrice, // Changed to string?
    [JsonProperty("AnalystRatingStrongBuy")] string? AnalystRatingStrongBuy, // Changed to string?
    [JsonProperty("AnalystRatingBuy")] string? AnalystRatingBuy, // Changed to string?
    [JsonProperty("AnalystRatingHold")] string? AnalystRatingHold, // Changed to string?
    [JsonProperty("AnalystRatingSell")] string? AnalystRatingSell, // Changed to string?
    [JsonProperty("AnalystRatingStrongSell")] string? AnalystRatingStrongSell, // Changed to string?
    [JsonProperty("TrailingPE")] string? TrailingPE, // Changed to string?
    [JsonProperty("ForwardPE")] string? ForwardPE, // Changed to string?
    [JsonProperty("PriceToSalesRatioTTM")] string? PriceToSalesRatioTTM, // Changed to string?
    [JsonProperty("PriceToBookRatio")] string? PriceToBookRatio, // Changed to string?
    [JsonProperty("EVToRevenue")] string? EVToRevenue, // Changed to string?
    [JsonProperty("EVToEBITDA")] string? EVToEBITDA, // Changed to string?
    [JsonProperty("Beta")] string? Beta, // Changed to string?
    [JsonProperty("52WeekHigh")] string? FiftyTwoWeekHigh, // Changed to string?
    [JsonProperty("52WeekLow")] string? FiftyTwoWeekLow, // Changed to string?
    [JsonProperty("50DayMovingAverage")] string? FiftyDayMovingAverage, // Changed to string?
    [JsonProperty("200DayMovingAverage")] string? TwoHundredDayMovingAverage, // Changed to string?
    [JsonProperty("SharesOutstanding")] string? SharesOutstanding, // Changed to string?
    [JsonProperty("DividendDate")] string? DividendDate, // Changed to string?
    [JsonProperty("ExDividendDate")] string? ExDividendDate // Changed to string?
);
