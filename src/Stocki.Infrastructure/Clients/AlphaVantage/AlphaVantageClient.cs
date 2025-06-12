using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Stocki.Application.Interfaces;
using Stocki.Application.Queries.Overview;
using Stocki.Domain.Models;
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

    public async Task<StockOverview?> GetStockOverviewAsync(StockOverviewQuery q)
    {
        var url =
            $"{_settings.Value.BaseUrl}query?function=OVERVIEW&symbol={q.Symbol.Value}&apikey={_settings.Value.ApiKey}";
        if (_cache.TryGetValue(url, out StockOverview? CacheRes))
        {
            return CacheRes;
        }
        try
        {
            var res = await _client.GetAsync(url);
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

            // TODO: Add a mapper class to save having this messy boilerplate
            var returnObj = new StockOverview(
                Name: avObj.Name,
                Symbol: avObj.Symbol,
                Description: avObj.Description,
                AssetType: avObj.AssetType,
                Currency: avObj.Currency,
                Sector: avObj.Sector,
                Industry: avObj.Industry,
                FiscalYearEnd: avObj.FiscalYearEnd,
                LatestQuarter: avObj.LatestQuarter,
                MarketCapitalization: avObj.MarketCapitalization,
                EBITDA: avObj.EBITDA,
                PERatio: avObj.PERatio,
                PEGRatio: avObj.PEGRatio,
                BookValue: avObj.BookValue,
                DividendPerShare: avObj.DividendPerShare,
                DividendYield: avObj.DividendYield,
                EPS: avObj.EPS,
                RevenuePerShareTTM: avObj.RevenuePerShareTTM,
                ProfitMargin: avObj.ProfitMargin,
                OperatingMarginTTM: avObj.OperatingMarginTTM,
                ReturnOnAssetsTTM: avObj.ReturnOnAssetsTTM,
                ReturnOnEquityTTM: avObj.ReturnOnEquityTTM,
                RevenueTTM: avObj.RevenueTTM,
                GrossProfitTTM: avObj.GrossProfitTTM,
                DilutedEPSTTM: avObj.DilutedEPSTTM,
                QuarterlyEarningsGrowthYOY: avObj.QuarterlyEarningsGrowthYOY,
                QuarterlyRevenueGrowthYOY: avObj.QuarterlyRevenueGrowthYOY,
                AnalystTargetPrice: avObj.AnalystTargetPrice,
                AnalystRatingStrongBuy: avObj.AnalystRatingStrongBuy,
                AnalystRatingBuy: avObj.AnalystRatingBuy,
                AnalystRatingHold: avObj.AnalystRatingHold,
                AnalystRatingSell: avObj.AnalystRatingSell,
                AnalystRatingStrongSell: avObj.AnalystRatingStrongSell,
                TrailingPE: avObj.TrailingPE,
                ForwardPE: avObj.ForwardPE,
                PriceToSalesRatioTTM: avObj.PriceToSalesRatioTTM,
                PriceToBookRatio: avObj.PriceToBookRatio,
                EVToRevenue: avObj.EVToRevenue,
                EVToEBITDA: avObj.EVToEBITDA,
                Beta: avObj.Beta,
                FiftyTwoWeekHigh: avObj.FiftyTwoWeekHigh,
                FiftyTwoWeekLow: avObj.FiftyTwoWeekLow,
                FiftyDayMovingAverage: avObj.FiftyDayMovingAverage,
                TwoHundredDayMovingAverage: avObj.TwoHundredDayMovingAverage,
                SharesOutstanding: avObj.SharesOutstanding,
                DividendDate: avObj.DividendDate,
                ExDividendDate: avObj.ExDividendDate
            );
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
