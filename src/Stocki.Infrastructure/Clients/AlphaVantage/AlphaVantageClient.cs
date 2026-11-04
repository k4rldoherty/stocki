using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stocki.Application.Interfaces;
using Stocki.Application.Queries.Overview;
using Stocki.Infrastructure.Clients.AlphaVantage.DTOs;
using Stocki.Shared.Config;
using Stocki.Shared.Models;

namespace Stocki.Infrastructure.Clients;

public class AlphaVantageClient : IAlphaVantageClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

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

    public async ValueTask<ApiResponse<StockOverview>> GetStockOverviewAsync(
        StockOverviewQuery q,
        CancellationToken token
    )
    {
        var url =
            $"{_settings.Value.BaseUrl}query?function=OVERVIEW&symbol={q.Symbol.Value}&apikey={_settings.Value.ApiKey}";

        if (_cache.TryGetValue(url, out StockOverview? cacheResult) && cacheResult is not null)
        {
            _logger.LogInformation("Query retrieved from cache: {Url}", url);
            return ApiResponse<StockOverview>.Success(cacheResult);
        }

        try
        {
            await using var stream = await _client.GetStreamAsync(url, token);
            if (stream.CanSeek && stream.Length == 0)
            {
                _logger.LogWarning("No data retrieved for ticker {Symbol}", q.Symbol.Value);
                return ApiResponse<StockOverview>.Failure(
                    "No data retrieved for this ticker",
                    HttpStatusCode.InternalServerError
                );
            }

            var avDto = await JsonSerializer.DeserializeAsync<AVStockOverviewDTO>(stream, JsonOptions, token);

            if (avDto is null)
            {
                _logger.LogWarning("Failed to deserialize response for ticker {Symbol}", q.Symbol.Value);
                return ApiResponse<StockOverview>.Failure(
                    $"Failed to deserialize Stock Overview for ticker {q.Symbol.Value}.",
                    HttpStatusCode.InternalServerError
                );
            }

            if (string.IsNullOrWhiteSpace(avDto.Symbol))
            {
                _logger.LogWarning("DTO has null or empty symbol for {Symbol}", q.Symbol.Value);
                return ApiResponse<StockOverview>.Failure(
                    $"No data found for ticker {q.Symbol.Value}.",
                    HttpStatusCode.NotFound
                );
            }

            var stockOverview = new StockOverview(
                Name: avDto.Name,
                Symbol: avDto.Symbol,
                Description: avDto.Description,
                AssetType: avDto.AssetType,
                Currency: avDto.Currency,
                Sector: avDto.Sector,
                Industry: avDto.Industry,
                FiscalYearEnd: avDto.FiscalYearEnd,
                LatestQuarter: ParseDate(avDto.LatestQuarter),
                MarketCapitalization: avDto.MarketCapitalization,
                EBITDA: avDto.EBITDA,
                PERatio: avDto.PERatio,
                PEGRatio: avDto.PEGRatio,
                BookValue: avDto.BookValue,
                DividendPerShare: avDto.DividendPerShare,
                DividendYield: avDto.DividendYield,
                EPS: avDto.EPS,
                RevenuePerShareTTM: avDto.RevenuePerShareTTM,
                ProfitMargin: avDto.ProfitMargin,
                OperatingMarginTTM: avDto.OperatingMarginTTM,
                ReturnOnAssetsTTM: avDto.ReturnOnAssetsTTM,
                ReturnOnEquityTTM: avDto.ReturnOnEquityTTM,
                RevenueTTM: avDto.RevenueTTM,
                GrossProfitTTM: avDto.GrossProfitTTM,
                DilutedEPSTTM: avDto.DilutedEPSTTM,
                QuarterlyEarningsGrowthYOY: avDto.QuarterlyEarningsGrowthYOY,
                QuarterlyRevenueGrowthYOY: avDto.QuarterlyRevenueGrowthYOY,
                AnalystTargetPrice: avDto.AnalystTargetPrice,
                AnalystRatingStrongBuy: avDto.AnalystRatingStrongBuy,
                AnalystRatingBuy: avDto.AnalystRatingBuy,
                AnalystRatingHold: avDto.AnalystRatingHold,
                AnalystRatingSell: avDto.AnalystRatingSell,
                AnalystRatingStrongSell: avDto.AnalystRatingStrongSell,
                TrailingPE: avDto.TrailingPE,
                ForwardPE: avDto.ForwardPE,
                PriceToSalesRatioTTM: avDto.PriceToSalesRatioTTM,
                PriceToBookRatio: avDto.PriceToBookRatio,
                EVToRevenue: avDto.EVToRevenue,
                EVToEBITDA: avDto.EVToEBITDA,
                Beta: avDto.Beta,
                FiftyTwoWeekHigh: avDto.FiftyTwoWeekHigh,
                FiftyTwoWeekLow: avDto.FiftyTwoWeekLow,
                FiftyDayMovingAverage: avDto.FiftyDayMovingAverage,
                TwoHundredDayMovingAverage: avDto.TwoHundredDayMovingAverage,
                SharesOutstanding: avDto.SharesOutstanding,
                DividendDate: ParseDate(avDto.DividendDate),
                ExDividendDate: ParseDate(avDto.ExDividendDate)
            );

            _cache.Set(url, stockOverview, TimeSpan.FromMinutes(10));
            _logger.LogInformation("Request to api succeeded and data cached successfully");

            return ApiResponse<StockOverview>.Success(stockOverview, HttpStatusCode.OK);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error for ticker {Symbol}", q.Symbol.Value);
            return ApiResponse<StockOverview>.Failure(
                ex.Message,
                HttpStatusCode.InternalServerError
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error for ticker {Symbol}", q.Symbol.Value);
            return ApiResponse<StockOverview>.Failure(
                ex.Message,
                HttpStatusCode.InternalServerError
            );
        }
    }

    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("None", StringComparison.OrdinalIgnoreCase))
            return null;
        if (DateTime.TryParseExact(value, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime result))
            return result;
        return null;
    }
}
