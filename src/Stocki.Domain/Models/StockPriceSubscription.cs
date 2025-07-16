namespace Stocki.Domain.Models;

public class StockPriceSubscription
{
    public StockPriceSubscription() { }

    public StockPriceSubscription(ulong discordId, string ticker)
    {
        Id = new Guid();
        DiscordId = discordId;
        Ticker = ticker;
        IsActive = true;
    }

    public Guid Id { get; set; }
    public ulong DiscordId { get; set; }
    public string Ticker { get; set; } = String.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastNotificationDate { get; set; } = null;
}
