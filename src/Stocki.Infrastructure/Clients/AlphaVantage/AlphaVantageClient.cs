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
using Stocki.Shared.Models;

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

    public async Task<ApiResponse<StockOverview>> GetStockOverviewAsync(
        StockOverviewQuery q,
        CancellationToken token
    )
    {
        var url =
            $"{_settings.Value.BaseUrl}query?function=OVERVIEW&symbol={q.Symbol.Value}&apikey={_settings.Value.ApiKey}";
        if (_cache.TryGetValue(url, out StockOverview? CacheRes) && CacheRes is not null)
        {
            _logger.LogInformation($"Query retrieved from cache: {url}");
            return ApiResponse<StockOverview>.Success(CacheRes);
        }
        try
        {
            var res = await _client.GetAsync(url, token);
            if (res.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogWarning($"API returned status code {res.StatusCode}");
                return ApiResponse<StockOverview>.Failure(
                    $"Alpha Vantage returned status code: {res.StatusCode}",
                    res.StatusCode
                );
            }
            string resStr = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(resStr) || resStr.Equals("{}"))
                return ApiResponse<StockOverview>.Failure(
                    $"The ticker {q.Symbol.Value} is invalid. Try again",
                    res.StatusCode
                );

            AVStockOverviewDTO? avObj = JsonConvert.DeserializeObject<AVStockOverviewDTO>(resStr);

            if (avObj == null)
            {
                _logger.LogWarning("Serialized object was null");
                return ApiResponse<StockOverview>.Failure(
                    $"Failed to serialize Stock Overview DTO object for ticker {q.Symbol.Value}."
                );
            }

            // Uses static mapped class
            StockOverview? returnObj = AlphaVantageMappingHelper.MapOverview(
                avObj,
                q.Symbol.Value,
                _logger
            );
            if (returnObj is null)
            {
                return ApiResponse<StockOverview>.Failure(
                    $"Failed to parse Stock Overview object for ticker {q.Symbol.Value}."
                );
            }
            _cache.Set(url, returnObj);
            return ApiResponse<StockOverview>.Success(returnObj, HttpStatusCode.OK);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex.Message);
            return ApiResponse<StockOverview>.Failure(
                ex.Message,
                HttpStatusCode.InternalServerError
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return ApiResponse<StockOverview>.Failure(
                ex.Message,
                HttpStatusCode.InternalServerError
            );
        }
    }
}
