using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using ClosedXML.Excel;

namespace Dqe.Web.Controllers
{
    /// <summary>
    /// Provides Florida Fisher Index values for each quarter, sourced dynamically from FDOT NHCCI data.
    /// </summary>
    public static class NHCCIData
    {
        private const string DataSourceUrl = "https://fdotwww.blob.core.windows.net/sitefinity/docs/default-source/fpo/fpc/apps/dqe/cci-inflation-indexs.xlsx";
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(15);
        private static readonly object SyncRoot = new object();
        private static Dictionary<string, decimal> _cachedIndexByQuarter = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        private static DateTime _cacheExpirationEst = DateTime.MinValue;
        private static DateTime _lastRefreshEst = DateTime.MinValue;
        private static readonly TimeZoneInfo EasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        /// <summary>
        /// Gets the cached inflation index table, refreshing from the remote source when needed.
        /// </summary>
        public static IReadOnlyDictionary<string, decimal> IndexByQuarter => EnsureData();

        /// <summary>
        /// Forces a cache refresh immediately ( for testing).
        /// </summary>
        public static void ForceRefresh()
        {
            lock (SyncRoot)
            {
                _cacheExpirationEst = DateTime.MinValue;
                EnsureData();
            }
        }

        /// <summary>
        /// Gets cache status information for debugging.
        /// </summary>
        public static string GetCacheStatus()
        {
            var nowEst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EasternTimeZone);
            var timeUntilExpiration = _cacheExpirationEst > nowEst 
                ? _cacheExpirationEst - nowEst 
                : TimeSpan.Zero;
            
            return $"Cache Status - Records: {_cachedIndexByQuarter.Count}, " +
                   $"Last Refresh: {_lastRefreshEst:yyyy-MM-dd HH:mm:ss} EST, " +
                   $"Expires: {_cacheExpirationEst:yyyy-MM-dd HH:mm:ss} EST, " +
                   $"Time Until Expiration: {timeUntilExpiration.TotalMinutes:F1} minutes, " +
                   $"Latest Quarter: {GetLatestQuarterKey()}";
        }

        /// <summary>
        /// Given a date, returns the quarter key in "YYYY Q#" format.
        /// </summary>
        public static string GetQuarterKey(DateTime date)
        {
            int quarter = (date.Month - 1) / 3 + 1;
            return $"{date.Year} Q{quarter}";
        }

        /// <summary>
        /// Gets the latest available Florida Fisher Index value.
        /// </summary>
        public static decimal GetLatestIndex()
        {
            var data = IndexByQuarter;
            if (data.Count == 0)
            {
                return 0m;
            }

            var latest = data.Keys
                .Select(ParseQuarterKey)
                .Where(info => info.HasValue)
                .Select(info => info.Value)
                .OrderBy(info => info.Year)
                .ThenBy(info => info.Quarter)
                .LastOrDefault();

            if (string.IsNullOrEmpty(latest.QuarterKey))
            {
                return 0m;
            }

            return data.TryGetValue(latest.QuarterKey, out var value) ? value : 0m;
        }

        /// <summary>
        /// Gets the latest quarter key (e.g., "2025 Q3").
        /// </summary>
        public static string GetLatestQuarterKey()
        {
            var data = IndexByQuarter;
            if (data.Count == 0)
            {
                return "Unknown";
            }

            var latest = data.Keys
                .Select(ParseQuarterKey)
                .Where(info => info.HasValue)
                .Select(info => info.Value)
                .OrderBy(info => info.Year)
                .ThenBy(info => info.Quarter)
                .LastOrDefault();

            return latest.QuarterKey ?? "Unknown";
        }

        /// <summary>
        /// Gets the latest quarter formatted for display (e.g., "Q3, 2025").
        /// </summary>
        public static string GetLatestQuarterDisplay()
        {
            var quarterKey = GetLatestQuarterKey();
            if (quarterKey == "Unknown")
            {
                return "Unknown";
            }

            var parts = quarterKey.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                return $"{parts[1]}, {parts[0]}";
            }

            return quarterKey;
        }

        /// <summary>
        /// Calculates the inflation-adjusted price using Florida Fisher Index.
        /// </summary>
        /// <returns>The inflation-adjusted price in today's terms.</returns>
        public static decimal CalculateInflationAdjustedPrice(decimal originalPrice, DateTime lettingDate)
        {
            try
            {
                var data = IndexByQuarter;
                var quarterKey = GetQuarterKey(lettingDate);

                if (!data.TryGetValue(quarterKey, out var lettingDateIndex))
                {
                    return originalPrice;
                }

                var latestIndex = GetLatestIndex();
                if (latestIndex == 0m || lettingDateIndex == 0m)
                {
                    return originalPrice;
                }

                var inflationFactor = latestIndex / lettingDateIndex;
                return originalPrice * inflationFactor;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error calculating inflation-adjusted price: {ex}");
                return originalPrice;
            }
        }
        private static Dictionary<string, decimal> EnsureData()
        {
            var nowUtc = DateTime.UtcNow;
            var nowEst = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, EasternTimeZone);
            if (_cachedIndexByQuarter.Count == 0 || nowEst >= _cacheExpirationEst)
            {
                lock (SyncRoot)
                {
                    nowUtc = DateTime.UtcNow;
                    nowEst = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, EasternTimeZone);

                    if (_cachedIndexByQuarter.Count == 0 || nowEst >= _cacheExpirationEst)
                    {
                        var wasEmpty = _cachedIndexByQuarter.Count == 0;
                        var wasExpired = nowEst >= _cacheExpirationEst;

                        Debug.WriteLine($"[NHCCI] Starting data refresh. WasEmpty: {wasEmpty}, WasExpired: {wasExpired}, CurrentTime: {nowEst:yyyy-MM-dd HH:mm:ss} EST");

                        try
                        {
                            _cachedIndexByQuarter = LoadIndexesFromSource();
                            _lastRefreshEst = nowEst;
                            _cacheExpirationEst = nowEst.Add(RefreshInterval);
                            Debug.WriteLine($"[NHCCI] Data refresh successful. Loaded {_cachedIndexByQuarter.Count} records. Next refresh at {_cacheExpirationEst:yyyy-MM-dd HH:mm:ss} EST");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[NHCCI] Data refresh failed: {ex}");
                            if (_cachedIndexByQuarter.Count == 0)
                            {
                                Debug.WriteLine($"[NHCCI] No cached data available. Application will continue with empty data until next refresh attempt.");
                            }
                            else
                            {
                                Debug.WriteLine($"[NHCCI] Keeping existing cached data ({_cachedIndexByQuarter.Count} records). Will retry at next refresh interval.");
                            }
                            _cacheExpirationEst = nowEst.Add(RefreshInterval);
                            Debug.WriteLine($"[NHCCI] Next refresh scheduled for {_cacheExpirationEst:yyyy-MM-dd HH:mm:ss} EST");
                        }
                    }
                }
            }

            return _cachedIndexByQuarter;
        }

        private static Dictionary<string, decimal> LoadIndexesFromSource()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            using (var webClient = new WebClient())
            {
                webClient.Headers.Add(HttpRequestHeader.UserAgent, "DQE-NHCCI-Loader/1.0");
                var dataBytes = webClient.DownloadData(DataSourceUrl);

                using (var stream = new MemoryStream(dataBytes))
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault()
                                     ?? throw new InvalidOperationException("NHCCI workbook is empty.");

                    var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                    bool headerSkipped = false;

                    foreach (var row in worksheet.RowsUsed())
                    {
                        if (!headerSkipped)
                        {
                            headerSkipped = true;
                            continue;
                        }

                        if (!TryGetYear(row, out var year))
                        {
                            continue;
                        }

                        if (!TryGetQuarter(row, out var quarter))
                        {
                            continue;
                        }

                        if (!TryGetIndex(row, out var index))
                        {
                            continue;
                        }

                        var quarterKey = $"{year} {quarter}";
                        if (!result.ContainsKey(quarterKey))
                        {
                            result[quarterKey] = decimal.Round(index, 6);
                        }
                    }

                    if (result.Count == 0)
                    {
                        throw new InvalidOperationException("NHCCI workbook did not contain any data rows.");
                    }

                    return result;
                }
            }
        }

        private static bool TryGetYear(IXLRow row, out int year)
        {
            var cell = row.Cell(1);
            if (cell.TryGetValue(out year))
            {
                return true;
            }

            var text = cell.GetString();
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out year);
        }

        private static bool TryGetQuarter(IXLRow row, out string quarter)
        {
            quarter = null;
            var cell = row.Cell(2);

            if (cell.TryGetValue(out int quarterNumber))
            {
                if (quarterNumber >= 1 && quarterNumber <= 4)
                {
                    quarter = $"Q{quarterNumber}";
                    return true;
                }
            }

            var text = cell.GetString();
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.Trim().ToUpperInvariant();

            if (text.StartsWith("Q", StringComparison.Ordinal))
            {
                text = text.Substring(1);
            }

            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out quarterNumber)
                   && quarterNumber >= 1
                   && quarterNumber <= 4
                   && (quarter = $"Q{quarterNumber}") != null;
        }

        private static bool TryGetIndex(IXLRow row, out decimal index)
        {
            var cell = row.Cell(3);

            if (cell.TryGetValue(out index))
            {
                return true;
            }

            if (cell.TryGetValue(out double doubleValue))
            {
                index = Convert.ToDecimal(doubleValue);
                return true;
            }

            var text = cell.GetString();
            return decimal.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out index);
        }

        private static QuarterKeyInfo? ParseQuarterKey(string quarterKey)
        {
            if (string.IsNullOrWhiteSpace(quarterKey))
            {
                return null;
            }

            var parts = quarterKey.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                return null;
            }

            if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var year))
            {
                return null;
            }

            var quarterPart = parts[1].Trim().ToUpperInvariant();
            if (quarterPart.StartsWith("Q", StringComparison.Ordinal))
            {
                quarterPart = quarterPart.Substring(1);
            }

            if (!int.TryParse(quarterPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out var quarter))
            {
                return null;
            }

            if (quarter < 1 || quarter > 4)
            {
                return null;
            }

            return new QuarterKeyInfo(quarterKey, year, quarter);
        }

        private struct QuarterKeyInfo
        {
            public QuarterKeyInfo(string quarterKey, int year, int quarter)
            {
                QuarterKey = quarterKey;
                Year = year;
                Quarter = quarter;
            }

            public string QuarterKey { get; }
            public int Year { get; }
            public int Quarter { get; }
        }
    }
}
