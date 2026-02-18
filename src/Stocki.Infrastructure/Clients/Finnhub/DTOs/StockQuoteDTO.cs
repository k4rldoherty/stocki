using System.Text.Json.Serialization;

namespace Stocki.Infrastructure.Clients.Finnhub.DTOs;

public record FHStockQuoteDTO(
    [property: JsonPropertyName("c")] decimal CurrentPrice,
    [property: JsonPropertyName("h")] decimal HighPrice,
    [property: JsonPropertyName("l")] decimal LowPrice,
    [property: JsonPropertyName("o")] decimal OpenPrice,
    [property: JsonPropertyName("pc")] decimal PreviousClosePrice,
    [property: JsonPropertyName("dp")] decimal? DifferencePercentage,
    [property: JsonPropertyName("d")] decimal? Difference,
    [property: JsonPropertyName("t")] long Timestamp
);
