namespace Stocki.Domain.ValueObjects;

public record TickerSymbol
{
    public string Value { get; set; }

    public TickerSymbol(string ticker)
    {
        if (ticker.Length < 1 || ticker.Length > 5 || string.IsNullOrEmpty(ticker))
        {
            throw new ArgumentException(
                "Ticker must be a valid sequence of charachters between 1-5 letters long"
            );
        }
        Value = ticker.ToUpper();
    }
}
