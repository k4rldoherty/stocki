namespace Stocki.Domain.Models;

public record StockQuote
{
    public string Ticker = string.Empty;
    public double CurrentPrice;
    public double OpeningPrice;
    public double ClosingPrice;
    public double High;
    public double Low;

    public StockQuote() { }

    public StockQuote(
        string ticker,
        double currPrice,
        double openingPrice,
        double closingPrice,
        double high,
        double low
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
