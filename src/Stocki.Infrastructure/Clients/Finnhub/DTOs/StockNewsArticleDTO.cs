using System.Text.Json.Serialization;

namespace Stocki.Infrastructure.Clients.Finnhub.DTOs;

public record FHStockNewsArticleDTO(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("datetime")] long TimeStamp,
    [property: JsonPropertyName("headline")] string Headline,
    [property: JsonPropertyName("image")] string Image,
    [property: JsonPropertyName("related")] string Related,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("summary")] string Summary,
    [property: JsonPropertyName("url")] string? Url
);
