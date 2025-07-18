using System.Text.Json.Serialization;

namespace Stocki.PriceMonitor.Models;

public struct FinnhubStockPriceRecievedMessage
{
    [JsonPropertyName("data")]
    public WSDataObj[] Data { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
}

public struct WSDataObj
{
    [JsonPropertyName("p")]
    public string Price { get; set; }

    [JsonPropertyName("s")]
    public string Symbol { get; set; }

    [JsonPropertyName("t")]
    public string Timestamp { get; set; }

    [JsonPropertyName("v")]
    public string Volume { get; set; }
}
