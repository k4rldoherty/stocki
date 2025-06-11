using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stocki.Infrastructure.Clients.DTOs;
using Stocki.Shared.Config;

namespace Stocki.Infrastructure.Clients;

public class AlphaVantageClient : IAlphaVantageClient
{
    private readonly IMemoryCache _cache;
    private readonly HttpClient _client;
    private readonly ILogger<AlphaVantageClient> _logger;
    private readonly AlphaVantageSettings _settings;

    public AlphaVantageClient(
        IMemoryCache cache,
        HttpClient httpClient,
        ILogger<AlphaVantageClient> logger,
        AlphaVantageSettings settings
    )
    {
        _cache = cache;
        _client = httpClient;
        _logger = logger;
        _settings = settings;
    }

    public async Task<AVStockOverviewDTO?> GetStockOverviewAsync(string symbol)
    {
        var url =
            $"{_settings.BaseUrl}query?function=OVERVIEW&symbol={symbol}&apikey={_settings.ApiKey}";
        if (_cache.TryGetValue(url, out AVStockOverviewDTO? CacheRes))
        {
            return CacheRes;
        }
        try
        {
            symbol = symbol.ToUpper();
            var res = await _client.GetAsync(url);
            if (res.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }
            string resStr = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(resStr) || resStr.Equals("{}"))
                return null;

            AVStockOverviewDTO obj = JsonConvert.DeserializeObject<AVStockOverviewDTO>(resStr);
            return obj;
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
