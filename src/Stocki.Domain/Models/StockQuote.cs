namespace Stocki.Domain.Models;

public record StockQuote
{
    public string Ticker = string.Empty;
    public decimal CurrentPrice;
    public decimal OpeningPrice;
    public decimal ClosingPrice;
    public decimal High;
    public decimal Low;

    public StockQuote() { }

    public StockQuote(
        string ticker,
        decimal currPrice,
        decimal openingPrice,
        decimal closingPrice,
        decimal high,
        decimal low
    )
    {
        Ticker = ticker;
        CurrentPrice = currPrice;
        OpeningPrice = openingPrice;
        ClosingPrice = closingPrice;
        High = high;
        Low = low;
    }
};
