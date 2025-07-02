public class StockOverview
{
    public string? Name { get; set; }
    public string? Symbol { get; set; }
    public string? Description { get; set; }
    public string? AssetType { get; set; }
    public string? Currency { get; set; }
    public string? Sector { get; set; }
    public string? Industry { get; set; }
    public string? FiscalYearEnd { get; set; }
    public DateTime? LatestQuarter { get; set; }
    public long? MarketCapitalization { get; set; }
    public long? EBITDA { get; set; }
    public decimal? PERatio { get; set; }
    public decimal? PEGRatio { get; set; }
    public decimal? BookValue { get; set; }
    public decimal? DividendPerShare { get; set; }
    public decimal? DividendYield { get; set; }
    public decimal? EPS { get; set; }
    public decimal? RevenuePerShareTTM { get; set; }
    public decimal? ProfitMargin { get; set; }
    public decimal? OperatingMarginTTM { get; set; }
    public decimal? ReturnOnAssetsTTM { get; set; }
    public decimal? ReturnOnEquityTTM { get; set; }
    public long? RevenueTTM { get; set; }
    public long? GrossProfitTTM { get; set; }
    public decimal? DilutedEPSTTM { get; set; }
    public decimal? QuarterlyEarningsGrowthYOY { get; set; }
    public decimal? QuarterlyRevenueGrowthYOY { get; set; }
    public decimal? AnalystTargetPrice { get; set; }
    public int? AnalystRatingStrongBuy { get; set; }
    public int? AnalystRatingBuy { get; set; }
    public int? AnalystRatingHold { get; set; }
    public int? AnalystRatingSell { get; set; }
    public int? AnalystRatingStrongSell { get; set; }
    public decimal? TrailingPE { get; set; }
    public decimal? ForwardPE { get; set; }
    public decimal? PriceToSalesRatioTTM { get; set; }
    public decimal? PriceToBookRatio { get; set; }
    public decimal? EVToRevenue { get; set; }
    public decimal? EVToEBITDA { get; set; }
    public decimal? Beta { get; set; }
    public decimal? FiftyTwoWeekHigh { get; set; }
    public decimal? FiftyTwoWeekLow { get; set; }
    public decimal? FiftyDayMovingAverage { get; set; }
    public decimal? TwoHundredDayMovingAverage { get; set; }
    public long? SharesOutstanding { get; set; }
    public DateTime? DividendDate { get; set; }
    public DateTime? ExDividendDate { get; set; }

    public StockOverview() { }

    public StockOverview(
        string? Name,
        string? Symbol,
        string? Description,
        string? AssetType,
        string? Currency,
        string? Sector,
        string? Industry,
        string? FiscalYearEnd,
        DateTime? LatestQuarter,
        long? MarketCapitalization,
        long? EBITDA,
        decimal? PERatio,
        decimal? PEGRatio,
        decimal? BookValue,
        decimal? DividendPerShare,
        decimal? DividendYield,
        decimal? EPS,
        decimal? RevenuePerShareTTM,
        decimal? ProfitMargin,
        decimal? OperatingMarginTTM,
        decimal? ReturnOnAssetsTTM,
        decimal? ReturnOnEquityTTM,
        long? RevenueTTM,
        long? GrossProfitTTM,
        decimal? DilutedEPSTTM,
        decimal? QuarterlyEarningsGrowthYOY,
        decimal? QuarterlyRevenueGrowthYOY,
        decimal? AnalystTargetPrice,
        int? AnalystRatingStrongBuy,
        int? AnalystRatingBuy,
        int? AnalystRatingHold,
        int? AnalystRatingSell,
        int? AnalystRatingStrongSell,
        decimal? TrailingPE,
        decimal? ForwardPE,
        decimal? PriceToSalesRatioTTM,
        decimal? PriceToBookRatio,
        decimal? EVToRevenue,
        decimal? EVToEBITDA,
        decimal? Beta,
        decimal? FiftyTwoWeekHigh,
        decimal? FiftyTwoWeekLow,
        decimal? FiftyDayMovingAverage,
        decimal? TwoHundredDayMovingAverage,
        long? SharesOutstanding,
        DateTime? DividendDate,
        DateTime? ExDividendDate
    )
    {
        this.Name = Name;
        this.Symbol = Symbol;
        this.Description = Description;
        this.AssetType = AssetType;
        this.Currency = Currency;
        this.Sector = Sector;
        this.Industry = Industry;
        this.FiscalYearEnd = FiscalYearEnd;
        this.LatestQuarter = LatestQuarter;
        this.MarketCapitalization = MarketCapitalization;
        this.EBITDA = EBITDA;
        this.PERatio = PERatio;
        this.PEGRatio = PEGRatio;
        this.BookValue = BookValue;
        this.DividendPerShare = DividendPerShare;
        this.DividendYield = DividendYield;
        this.EPS = EPS;
        this.RevenuePerShareTTM = RevenuePerShareTTM;
        this.ProfitMargin = ProfitMargin;
        this.OperatingMarginTTM = OperatingMarginTTM;
        this.ReturnOnAssetsTTM = ReturnOnAssetsTTM;
        this.ReturnOnEquityTTM = ReturnOnEquityTTM;
        this.RevenueTTM = RevenueTTM;
        this.GrossProfitTTM = GrossProfitTTM;
        this.DilutedEPSTTM = DilutedEPSTTM;
        this.QuarterlyEarningsGrowthYOY = QuarterlyEarningsGrowthYOY;
        this.QuarterlyRevenueGrowthYOY = QuarterlyRevenueGrowthYOY;
        this.AnalystTargetPrice = AnalystTargetPrice;
        this.AnalystRatingStrongBuy = AnalystRatingStrongBuy;
        this.AnalystRatingBuy = AnalystRatingBuy;
        this.AnalystRatingHold = AnalystRatingHold;
        this.AnalystRatingSell = AnalystRatingSell;
        this.AnalystRatingStrongSell = AnalystRatingStrongSell;
        this.TrailingPE = TrailingPE;
        this.ForwardPE = ForwardPE;
        this.PriceToSalesRatioTTM = PriceToSalesRatioTTM;
        this.PriceToBookRatio = PriceToBookRatio;
        this.EVToRevenue = EVToRevenue;
        this.EVToEBITDA = EVToEBITDA;
        this.Beta = Beta;
        this.FiftyTwoWeekHigh = FiftyTwoWeekHigh;
        this.FiftyTwoWeekLow = FiftyTwoWeekLow;
        this.FiftyDayMovingAverage = FiftyDayMovingAverage;
        this.TwoHundredDayMovingAverage = TwoHundredDayMovingAverage;
        this.SharesOutstanding = SharesOutstanding;
        this.DividendDate = DividendDate;
        this.ExDividendDate = ExDividendDate;
    }
}
