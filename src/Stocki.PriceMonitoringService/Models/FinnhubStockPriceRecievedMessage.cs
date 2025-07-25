using Newtonsoft.Json;

namespace Stocki.PriceMonitor.Models;

public struct FinnhubStockPriceRecievedMessage
{
    [JsonProperty("data")]
    public WSDataObj[] Data { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }
}

public struct WSDataObj
{
    [JsonProperty("c")]
    public string[] Category { get; set; }

    [JsonProperty("p")]
    public double Price { get; set; }

    [JsonProperty("s")]
    public string Symbol { get; set; }

    [JsonProperty("t")]
    public ulong Timestamp { get; set; }

    [JsonProperty("v")]
    public long Volume { get; set; }
}
