using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stocki.Application.Interfaces;
using Stocki.Application.Queries.News;
using Stocki.Application.Queries.Quote;
using Stocki.Domain.Models;
using Stocki.Infrastructure.Clients.Finnhub;
using Stocki.Infrastructure.Clients.Finnhub.DTOs;
using Stocki.Shared.Config;
using Stocki.Shared.Models;

namespace Stocki.Infrastructure.Clients;

public class FinnhubClient : IFinnhubClient
{
    private readonly IMemoryCache _cache;
    private readonly HttpClient _client;
    private readonly ILogger<FinnhubClient> _logger;
    private readonly IOptions<FinnhubClientSettings> _settings;

    public FinnhubClient(
        IMemoryCache cache,
        HttpClient httpClient,
        ILogger<FinnhubClient> logger,
        IOptions<FinnhubClientSettings> settings
    )
    {
        _cache = cache;
        _client = httpClient;
        _logger = logger;
        _settings = settings;
    }

    public async ValueTask<ApiResponse<StockQuote>> GetStockQuoteAsync(
        StockQuoteQuery q,
        CancellationToken t
    )
    {
        var url = $"{_settings.Value.BaseUrl}quote?symbol={q.Symbol.Value}";
        if (_cache.TryGetValue(url, out StockQuote? CacheRes) && CacheRes is not null)
        {
            return ApiResponse<StockQuote>.Success(CacheRes);
        }
        try
        {
            using var res = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, t);
            if (!res.IsSuccessStatusCode)
                return ApiResponse<StockQuote>.Failure("Failed to retrieve the data from Finnhub", res.StatusCode);

            using var contentStream = await res.Content.ReadAsStreamAsync(cancellationToken: t);
            var stockQuoteDTO = await JsonSerializer.DeserializeAsync<FHStockQuoteDTO>(contentStream, cancellationToken: t);
            if (stockQuoteDTO is null || stockQuoteDTO.DifferencePercentage is null)
                return ApiResponse<StockQuote>.Failure("Failed to deserialize the data from Finnhub", res.StatusCode);

            var returnObj = FinnhubMappingHelper.MapStockQuote(stockQuoteDTO, q.Symbol.Value, _logger);
            if (returnObj == null)
            {
                _logger.LogError("Problem mapping object");
                return ApiResponse<StockQuote>.Failure(
                    $"Failed to parse Stock Quote object for ticker {q.Symbol.Value}.",
                    HttpStatusCode.InternalServerError
                );
            }
            _cache.Set(url, returnObj, TimeSpan.FromMinutes(5));
            return ApiResponse<StockQuote>.Success(returnObj, res.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return ApiResponse<StockQuote>.Failure(
                "An unexpected error occurred when retrieving the data from Finnhub",
                HttpStatusCode.InternalServerError
            );
        }
    }

    public async ValueTask<ApiResponse<List<StockNewsArticle>>> GetCompanyNewsAsync(
        StockNewsQuery q,
        CancellationToken c
    )
    {
        var url =
            $"{_settings.Value.BaseUrl}company-news?symbol={q.Symbol.Value}&from={DateTime.UtcNow.AddHours(-24).ToString("yyyy-MM-dd")}&to={DateTime.UtcNow.ToString("yyyy-MM-dd")}";
        if (
            _cache.TryGetValue($"news-{q.Symbol.Value}", out List<StockNewsArticle>? CacheRes)
            && CacheRes != null
        )
        {
            return ApiResponse<List<StockNewsArticle>>.Success(CacheRes);
        }
        try
        {
            // TODO: Make the to and from dates changeable for older news / specific ranges
            using var res = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, c);
            if (!res.IsSuccessStatusCode)
            {
                return ApiResponse<List<StockNewsArticle>>.Failure(
                    $"Finnhub returned status code {res.StatusCode}",
                    res.StatusCode
                );
            }

            using var contentStream = await res.Content.ReadAsStreamAsync(cancellationToken: c);
            var dto = await JsonSerializer.DeserializeAsync<List<FHStockNewsArticleDTO>>(contentStream, cancellationToken: c);

            if (dto == null)
            {
                return ApiResponse<List<StockNewsArticle>>.Failure(
                    $"Failed to serialize Stock News DTO object for ticker {q.Symbol.Value}.",
                    HttpStatusCode.InternalServerError
                );
            }

            var returnObj = FinnhubMappingHelper.MapStockNews(dto, q.Symbol.Value, _logger);

            if (returnObj == null)
            {
                _logger.LogError("Problem mapping object");
                return ApiResponse<List<StockNewsArticle>>.Failure(
                    $"Failed to parse Stock News object for ticker {q.Symbol.Value}.",
                    HttpStatusCode.InternalServerError
                );
            }
            _cache.Set(
                $"news-{q.Symbol.Value}",
                returnObj,
                absoluteExpiration: DateTime.UtcNow.AddDays(1)
            );
            return ApiResponse<List<StockNewsArticle>>.Success(returnObj, res.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return ApiResponse<List<StockNewsArticle>>.Failure(
                ex.Message,
                HttpStatusCode.InternalServerError
            );
        }
    }
}
