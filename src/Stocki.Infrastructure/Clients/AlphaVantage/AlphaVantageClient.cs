using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Stocki.Infrastructure.Clients.DTOs;

namespace Stocki.Infrastructure.Clients;

public class AlphaVantageClient : IAlphaVantageClient
{
    private readonly string? _alphaApi =
        Environment.GetEnvironmentVariable("ALPHAAPI")
        ?? throw new NullReferenceException("Cannot retrieve alpha api key");

    // TODO: put this in config
    private readonly string _alphaBaseUrl = "https://www.alphavantage.co/";

    private readonly IMemoryCache _cache;
    private readonly HttpClient _client;
    private readonly ILogger<AlphaVantageClient> _logger;

    public AlphaVantageClient(
        IMemoryCache cache,
        HttpClient httpClient,
        ILogger<AlphaVantageClient> logger
    )
    {
        _cache = cache;
        _client = httpClient;
        _logger = logger;
    }

    public async Task<AVStockOverviewDTO?> GetStockOverviewAsync(string symbol)
    {
        var url = $"{_alphaBaseUrl}query?function=OVERVIEW&symbol={symbol}&apikey={_alphaApi}";
        if (_cache.TryGetValue(url, out AVStockOverviewDTO? CacheRes))
        {
            return CacheRes;
        }
        try
        {
            symbol = symbol.ToUpper();
            var res = await _client.GetStreamAsync(url);
            AVStockOverviewDTO? obj = await JsonSerializer.DeserializeAsync<AVStockOverviewDTO>(
                res
            );
            _cache.Set(url, obj, DateTimeOffset.Now.AddDays(1));
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
