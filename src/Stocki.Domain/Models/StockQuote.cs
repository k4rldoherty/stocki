namespace Stocki.Domain.Models;

public record StockQuote(
    string Ticker,
    double CurrentPrice,
    double OpeningPrice,
    double ClosingPrice,
    double High,
    double Low
);
