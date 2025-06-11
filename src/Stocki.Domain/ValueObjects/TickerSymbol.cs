namespace Stocki.Domain.ValueObjects;

public record TickerSymbol
{
    public string Value { get; set; }

    public TickerSymbol(string ticker)
    {
        if (ticker.Length < 1 || ticker.Length > 5 || string.IsNullOrEmpty(ticker))
        {
            throw new ArgumentNullException("Ticker must be present");
        }
        Value = ticker.ToUpper();
    }
}
