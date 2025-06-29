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

    public async Task<StockQuote?> GetStockQuoteAsync(StockQuoteQuery q, CancellationToken t)
    {
        var url = $"{_settings.Value.BaseUrl}quote?symbol={q.Symbol.Value}";
        if (_cache.TryGetValue(url, out StockQuote? CacheRes))
        {
            _logger.LogInformation($"Query retrieved from cache: {url}");
            return CacheRes;
        }
        try
        {
            var res = await _client.GetAsync(url, t);
            if (res.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogWarning($"API returned status code {res.StatusCode}");
                return null;
            }
            string resStr = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(resStr) || resStr.Equals("{}"))
            {
                _logger.LogError("Response was empty");
                return null;
            }

            FHStockQuoteDTO? stockQuoteDTO = JsonConvert.DeserializeObject<FHStockQuoteDTO>(resStr);

            if (stockQuoteDTO == null)
            {
                _logger.LogWarning("Serialized object was null");
                return null;
            }

            var returnObj = FinnhubMappingHelper.MapStockQuote(
                stockQuoteDTO,
                q.Symbol.Value,
                _logger
            );
            if (returnObj == null)
            {
                _logger.LogError("Problem mapping object");
                return null;
            }
            _logger.LogInformation("Object mapped successfully");
            _cache.Set(url, returnObj, absoluteExpiration: DateTime.UtcNow.AddMinutes(5));
            return returnObj;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return null;
        }
    }

    public async Task<List<StockNewsArticle>?> GetCompanyNewsAsync(
        StockNewsQuery q,
        CancellationToken c
    )
    {
        try
        {
            // TODO: Make the to and from dates changeable for older news / specific ranges
            var url =
                $"{_settings.Value.BaseUrl}company-news?symbol={q.Symbol.Value}&from={DateTime.UtcNow.AddHours(-24).ToString("yyyy-MM-dd")}&to={DateTime.UtcNow.ToString("yyyy-MM-dd")}";
            if (_cache.TryGetValue($"news-{q.Symbol.Value}", out List<StockNewsArticle>? CacheRes))
            {
                _logger.LogInformation("Retrieved from cache");
                return CacheRes;
            }
            var res = await _client.GetAsync(url, c);
            if (res.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogWarning($"API returned status code {res.StatusCode}");
                return null;
            }
            string resStr = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(resStr) || resStr.Equals("[]"))
            {
                _logger.LogError("Response was empty");
                return null;
            }

            List<FHStockNewsArticleDTO>? dto = JsonConvert.DeserializeObject<
                List<FHStockNewsArticleDTO>
            >(resStr);

            if (dto == null)
            {
                _logger.LogWarning("Serialized object was null");
                return null;
            }

            var returnObj = FinnhubMappingHelper.MapStockNews(dto, q.Symbol.Value, _logger);

            if (returnObj == null)
            {
                _logger.LogError("Problem mapping object");
                return null;
            }
            _logger.LogInformation("Object mapped successfully");
            _cache.Set(
                $"news-{q.Symbol.Value}",
                returnObj,
                absoluteExpiration: DateTime.UtcNow.AddDays(1)
            );
            return returnObj;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return null;
        }
    }
}
