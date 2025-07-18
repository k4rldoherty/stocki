using System.Text.Json.Serialization;

namespace Stocki.PriceMonitor.Models;

public struct FinnhubSubscriptionMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }
}
