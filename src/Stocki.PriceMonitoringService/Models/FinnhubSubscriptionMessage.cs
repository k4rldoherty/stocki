using Newtonsoft.Json;

namespace Stocki.PriceMonitor.Models;

public struct FinnhubWebsocketSubscriptionMessage
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("symbol")]
    public string Symbol { get; set; }
}
