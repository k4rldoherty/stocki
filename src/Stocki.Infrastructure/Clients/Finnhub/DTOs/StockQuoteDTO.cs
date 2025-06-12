using Newtonsoft.Json;

namespace Stocki.Infrastructure.Clients.Finnhub.DTOs;

public record FHStockQuoteDTO(
    [JsonProperty("c")] string CurrentPrice,
    [JsonProperty("h")] string HighPrice,
    [JsonProperty("l")] string LowPrice,
    [JsonProperty("o")] string OpenPrice,
    [JsonProperty("pc")] string PreviousClosePrice,
    [JsonProperty("dp")] string DifferencePercentage,
    [JsonProperty("d")] string Difference,
    [JsonProperty("t")] string Timestamp
);
