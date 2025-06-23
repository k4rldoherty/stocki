using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Stocki.Application.Interfaces;
using Stocki.Application.Queries.Overview;
using Stocki.Domain.Models;
using Stocki.Infrastructure.Clients.AlphaVantage;
using Stocki.Infrastructure.Clients.DTOs;
using Stocki.Shared.Config;

namespace Stocki.Infrastructure.Clients;

public class AlphaVantageClient : IAlphaVantageClient
{
    private readonly IMemoryCache _cache;
    private readonly HttpClient _client;
    private readonly ILogger<AlphaVantageClient> _logger;
    private readonly IOptions<AlphaVantageSettings> _settings;

    public AlphaVantageClient(
        IMemoryCache cache,
        HttpClient httpClient,
        ILogger<AlphaVantageClient> logger,
        IOptions<AlphaVantageSettings> settings
    )
    {
        _cache = cache;
        _client = httpClient;
        _logger = logger;
        _settings = settings;
    }

    public async Task<StockOverview?> GetStockOverviewAsync(
        StockOverviewQuery q,
        CancellationToken token
    )
    {
        var url =
            $"{_settings.Value.BaseUrl}query?function=OVERVIEW&symbol={q.Symbol.Value}&apikey={_settings.Value.ApiKey}";
        if (_cache.TryGetValue(url, out StockOverview? CacheRes))
        {
            _logger.LogInformation($"Query retrieved from cache: {url}");
            return CacheRes;
        }
        try
        {
            var res = await _client.GetAsync(url, token);
            if (res.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogWarning($"API returned status code {res.StatusCode}");
                return null;
            }
            string resStr = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(resStr) || resStr.Equals("{}"))
                return null;

            AVStockOverviewDTO? avObj = JsonConvert.DeserializeObject<AVStockOverviewDTO>(resStr);

            if (avObj == null)
            {
                _logger.LogWarning("Serialized object was null");
                return null;
            }

            // Uses static mapped class
            var returnObj = AlphaVantageMappingHelper.MapOverview(avObj, q.Symbol.Value, _logger);
            _cache.Set(url, returnObj);
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
