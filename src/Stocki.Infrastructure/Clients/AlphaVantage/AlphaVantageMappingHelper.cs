// Stocki.Infrastructure/Clients/AlphaVantage/Mappers/AlphaVantageOverviewMapper.cs
using System.Globalization; // For CultureInfo.InvariantCulture
using Microsoft.Extensions.Logging; // For ILogger
using Stocki.Domain.Models;
using Stocki.Infrastructure.Clients.DTOs;

namespace Stocki.Infrastructure.Clients.AlphaVantage;

public static class AlphaVantageMappingHelper
{
    public static StockOverview? MapOverview(
        AVStockOverviewDTO avObj,
        string symbol,
        ILogger logger
    )
    {
        // Basic validation before mapping
        if (avObj == null || string.IsNullOrWhiteSpace(avObj.Symbol))
        {
            logger.LogError(
                "Cannot map null AVStockOverviewDTO or DTO with null symbol for {Symbol}",
                symbol
            );
            return null;
        }

        // Helper for handling "None" string values common in AlphaVantage API
        static bool IsNone(string? value) =>
            value?.Equals("None", StringComparison.OrdinalIgnoreCase) ?? false;
        static bool IsDash(string? value) =>
            value?.Equals("-", StringComparison.OrdinalIgnoreCase) ?? false;

        // Local helper for parsing decimals with logging
        decimal? ParseDecimalSafe(string? value, string fieldName)
        {
            if (
                string.IsNullOrWhiteSpace(value)
                || IsNone(value)
                || !decimal.TryParse(
                    value,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out decimal parsedValue
                )
            )
            {
                logger.LogWarning(
                    "Failed to parse decimal value for {Field} of {Symbol}. Raw value: '{Value}'",
                    fieldName,
                    symbol,
                    value
                );
                return null;
            }
            return parsedValue;
        }

        // Local helper for parsing longs with logging
        long? ParseLongSafe(string? value, string fieldName)
        {
            if (
                string.IsNullOrWhiteSpace(value)
                || IsNone(value)
                || !long.TryParse(
                    value,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out long parsedValue
                )
            )
            {
                logger.LogWarning(
                    "Failed to parse long value for {Field} of {Symbol}. Raw value: '{Value}'",
                    fieldName,
                    symbol,
                    value
                );
                return null;
            }
            return parsedValue;
        }

        // Local helper for parsing ints with logging (for analyst ratings that might be "-")
        int? ParseIntSafe(string? value, string fieldName)
        {
            if (
                string.IsNullOrWhiteSpace(value)
                || IsDash(value)
                || !int.TryParse(
                    value,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out int parsedValue
                )
            )
            {
                logger.LogWarning(
                    "Failed to parse int value for {Field} of {Symbol}. Raw value: '{Value}'",
                    fieldName,
                    symbol,
                    value
                );
                return null;
            }
            return parsedValue;
        }

        // Local helper for parsing DateTimes with logging
        DateTime? ParseDateTimeSafe(string? value, string fieldName)
        {
            // AlphaVantage dates are often in YYYY-MM-DD format
            if (
                string.IsNullOrWhiteSpace(value)
                || IsNone(value)
                || !DateTime.TryParseExact(
                    value,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime parsedValue
                )
            )
            {
                logger.LogWarning(
                    "Failed to parse DateTime value for {Field} of {Symbol}. Raw value: '{Value}'",
                    fieldName,
                    symbol,
                    value
                );
                return null;
            }
            return parsedValue;
        }

        // The entire mapping block now uses the local helper methods
        return new StockOverview(
            Name: avObj.Name,
            Symbol: avObj.Symbol,
            Description: avObj.Description,
            AssetType: avObj.AssetType,
            Currency: avObj.Currency,
            Sector: avObj.Sector,
            Industry: avObj.Industry,
            FiscalYearEnd: avObj.FiscalYearEnd,
            LatestQuarter: ParseDateTimeSafe(avObj.LatestQuarter, "LatestQuarter"),
            MarketCapitalization: ParseLongSafe(avObj.MarketCapitalization, "MarketCapitalization"),
            EBITDA: ParseLongSafe(avObj.EBITDA, "EBITDA"),
            PERatio: ParseDecimalSafe(avObj.PERatio, "PERatio"),
            PEGRatio: ParseDecimalSafe(avObj.PEGRatio, "PEGRatio"),
            BookValue: ParseDecimalSafe(avObj.BookValue, "BookValue"),
            DividendPerShare: ParseDecimalSafe(avObj.DividendPerShare, "DividendPerShare"),
            DividendYield: ParseDecimalSafe(avObj.DividendYield, "DividendYield"),
            EPS: ParseDecimalSafe(avObj.EPS, "EPS"),
            RevenuePerShareTTM: ParseDecimalSafe(avObj.RevenuePerShareTTM, "RevenuePerShareTTM"),
            ProfitMargin: ParseDecimalSafe(avObj.ProfitMargin, "ProfitMargin"),
            OperatingMarginTTM: ParseDecimalSafe(avObj.OperatingMarginTTM, "OperatingMarginTTM"),
            ReturnOnAssetsTTM: ParseDecimalSafe(avObj.ReturnOnAssetsTTM, "ReturnOnAssetsTTM"),
            ReturnOnEquityTTM: ParseDecimalSafe(avObj.ReturnOnEquityTTM, "ReturnOnEquityTTM"),
            RevenueTTM: ParseLongSafe(avObj.RevenueTTM, "RevenueTTM"),
            GrossProfitTTM: ParseLongSafe(avObj.GrossProfitTTM, "GrossProfitTTM"),
            DilutedEPSTTM: ParseDecimalSafe(avObj.DilutedEPSTTM, "DilutedEPSTTM"),
            QuarterlyEarningsGrowthYOY: ParseDecimalSafe(
                avObj.QuarterlyEarningsGrowthYOY,
                "QuarterlyEarningsGrowthYOY"
            ),
            QuarterlyRevenueGrowthYOY: ParseDecimalSafe(
                avObj.QuarterlyRevenueGrowthYOY,
                "QuarterlyRevenueGrowthYOY"
            ),
            AnalystTargetPrice: ParseDecimalSafe(avObj.AnalystTargetPrice, "AnalystTargetPrice"),
            AnalystRatingStrongBuy: ParseIntSafe(
                avObj.AnalystRatingStrongBuy,
                "AnalystRatingStrongBuy"
            ),
            AnalystRatingBuy: ParseIntSafe(avObj.AnalystRatingBuy, "AnalystRatingBuy"),
            AnalystRatingHold: ParseIntSafe(avObj.AnalystRatingHold, "AnalystRatingHold"),
            AnalystRatingSell: ParseIntSafe(avObj.AnalystRatingSell, "AnalystRatingSell"),
            AnalystRatingStrongSell: ParseIntSafe(
                avObj.AnalystRatingStrongSell,
                "AnalystRatingStrongSell"
            ),
            TrailingPE: ParseDecimalSafe(avObj.TrailingPE, "TrailingPE"),
            ForwardPE: ParseDecimalSafe(avObj.ForwardPE, "ForwardPE"),
            PriceToSalesRatioTTM: ParseDecimalSafe(
                avObj.PriceToSalesRatioTTM,
                "PriceToSalesRatioTTM"
            ),
            PriceToBookRatio: ParseDecimalSafe(avObj.PriceToBookRatio, "PriceToBookRatio"),
            EVToRevenue: ParseDecimalSafe(avObj.EVToRevenue, "EVToRevenue"),
            EVToEBITDA: ParseDecimalSafe(avObj.EVToEBITDA, "EVToEBITDA"),
            Beta: ParseDecimalSafe(avObj.Beta, "Beta"),
            FiftyTwoWeekHigh: ParseDecimalSafe(avObj.FiftyTwoWeekHigh, "52WeekHigh"),
            FiftyTwoWeekLow: ParseDecimalSafe(avObj.FiftyTwoWeekLow, "52WeekLow"),
            FiftyDayMovingAverage: ParseDecimalSafe(
                avObj.FiftyDayMovingAverage,
                "50DayMovingAverage"
            ),
            TwoHundredDayMovingAverage: ParseDecimalSafe(
                avObj.TwoHundredDayMovingAverage,
                "200DayMovingAverage"
            ),
            SharesOutstanding: ParseLongSafe(avObj.SharesOutstanding, "SharesOutstanding"),
            DividendDate: ParseDateTimeSafe(avObj.DividendDate, "DividendDate"),
            ExDividendDate: ParseDateTimeSafe(avObj.ExDividendDate, "ExDividendDate")
        );
    }
}
