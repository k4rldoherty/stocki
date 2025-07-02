using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Stocki.Application.Interfaces;
using Stocki.Application.Queries.News;
using Stocki.Application.Queries.Price;
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

    public async Task<ApiResponse<StockQuote>> GetStockQuoteAsync(
        StockQuoteQuery q,
        CancellationToken t
    )
    {
        var url = $"{_settings.Value.BaseUrl}quote?symbol={q.Symbol.Value}";
        if (_cache.TryGetValue(url, out StockQuote? CacheRes) && CacheRes is not null)
        {
            _logger.LogInformation($"Query retrieved from cache: {url}");
            return ApiResponse<StockQuote>.Success(CacheRes);
        }
        try
        {
            var res = await _client.GetAsync(url, t);
            if (res.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogWarning($"API returned status code {res.StatusCode}");
                return ApiResponse<StockQuote>.Failure(
                    "Failed to retrieve the data from Finnhub",
                    res.StatusCode
                );
            }
            string resStr = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(resStr))
            {
                _logger.LogError("Response was empty");
                return ApiResponse<StockQuote>.Failure(
                    "Failed to retrieve the data from Finnhub",
                    res.StatusCode
                );
            }

            FHStockQuoteDTO? stockQuoteDTO = JsonConvert.DeserializeObject<FHStockQuoteDTO>(resStr);

            if (stockQuoteDTO is null || stockQuoteDTO.DifferencePercentage == null)
            {
                _logger.LogWarning("Serialized object was null");
                return ApiResponse<StockQuote>.Failure(
                    $"Failed to serialize Stock Quote DTO object for ticker {q.Symbol.Value}.",
                    HttpStatusCode.InternalServerError
                );
            }

            var returnObj = FinnhubMappingHelper.MapStockQuote(
                stockQuoteDTO,
                q.Symbol.Value,
                _logger
            );
            if (returnObj == null)
            {
                _logger.LogError("Problem mapping object");
                return ApiResponse<StockQuote>.Failure(
                    $"Failed to parse Stock Quote object for ticker {q.Symbol.Value}.",
                    HttpStatusCode.InternalServerError
                );
            }
            _logger.LogInformation("Object mapped successfully");
            _cache.Set(url, returnObj, absoluteExpiration: DateTime.UtcNow.AddMinutes(5));
            return ApiResponse<StockQuote>.Success(returnObj, res.StatusCode);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex.Message);
            return ApiResponse<StockQuote>.Failure(ex.Message, HttpStatusCode.InternalServerError);
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

    public async Task<ApiResponse<List<StockNewsArticle>>> GetCompanyNewsAsync(
        StockNewsQuery q,
        CancellationToken c
    )
    {
        try
        {
            // TODO: Make the to and from dates changeable for older news / specific ranges
            var url =
                $"{_settings.Value.BaseUrl}company-news?symbol={q.Symbol.Value}&from={DateTime.UtcNow.AddHours(-24).ToString("yyyy-MM-dd")}&to={DateTime.UtcNow.ToString("yyyy-MM-dd")}";
            if (
                _cache.TryGetValue($"news-{q.Symbol.Value}", out List<StockNewsArticle>? CacheRes)
                && CacheRes != null
            )
            {
                _logger.LogInformation("Retrieved from cache");
                return ApiResponse<List<StockNewsArticle>>.Success(CacheRes);
            }
            var res = await _client.GetAsync(url, c);
            if (res.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogWarning($"API returned status code {res.StatusCode}");
                return ApiResponse<List<StockNewsArticle>>.Failure(
                    $"Finnhub returned status code {res.StatusCode}",
                    res.StatusCode
                );
            }
            string resStr = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(resStr) || resStr.Equals("[]"))
            {
                _logger.LogWarning("Response was empty");
                return ApiResponse<List<StockNewsArticle>>.Failure(
                    $"No data retrieved for this ticker",
                    res.StatusCode
                );
            }

            List<FHStockNewsArticleDTO>? dto = JsonConvert.DeserializeObject<
                List<FHStockNewsArticleDTO>
            >(resStr);

            if (dto == null)
            {
                _logger.LogWarning("Serialized object was null");
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
            _logger.LogInformation("Object mapped and data cached successfully");
            return ApiResponse<List<StockNewsArticle>>.Success(returnObj, res.StatusCode);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex.Message);
            return ApiResponse<List<StockNewsArticle>>.Failure(
                ex.Message,
                HttpStatusCode.InternalServerError
            );
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
