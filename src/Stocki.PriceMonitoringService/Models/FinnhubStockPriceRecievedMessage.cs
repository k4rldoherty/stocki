using System.Text.Json.Serialization;

namespace Stocki.PriceMonitor.Models;

public readonly struct FinnhubStockPriceReceivedMessage
{
    [JsonPropertyName("data")]
    public WSDataObj[] Data { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; init; }
}

public readonly struct WSDataObj
{
    [JsonPropertyName("c")]
    public string[]? Category { get; init; }

    [JsonPropertyName("p")]
    public decimal Price { get; init; }

    [JsonPropertyName("s")]
    public string Symbol { get; init; }

    [JsonPropertyName("t")]
    public ulong Timestamp { get; init; }

    [JsonPropertyName("v")]
    public decimal Volume { get; init; }
}
