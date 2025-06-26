using Newtonsoft.Json;

namespace Stocki.Infrastructure.Clients.Finnhub.DTOs;

public record FHStockNewsArticleDTO(
    [JsonProperty("category")] string Category,
    [JsonProperty("datetime")] string TimeStamp,
    [JsonProperty("headline")] string Headline,
    [JsonProperty("id")] string Id,
    [JsonProperty("image")] string Image,
    [JsonProperty("related")] string Related,
    [JsonProperty("source")] string Source,
    [JsonProperty("summary")] string Summary,
    [JsonProperty("url")] string Url
);
