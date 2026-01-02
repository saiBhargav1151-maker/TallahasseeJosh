using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;
using System.Linq;
using System.Web;

namespace Dqe.Web.Controllers
{
    /// <summary>
    /// Controller responsible for handling Unit Price Search functionality
    /// </summary>
    public class UnitPriceSearchController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnitPriceSearchController"/> class.
        /// </summary>
        private readonly IWebTransportService _webTransportService;
        public UnitPriceSearchController(IWebTransportService webTransportService)
        {
            _webTransportService = webTransportService;
        }
        // <summary>
        /// Returns a list of pay item suggestions based on the input text.
        /// Input must be at least 2 characters long. 
        /// </summary>
        /// <returns> JSON data
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        [ValidateInput(true)]
        public ActionResult GetPayItemSuggestions(string input)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input) || input.Length < 3)
                {
                    return Json(new List<PayItemDTO>(), JsonRequestBehavior.AllowGet);
                }

                var suggestions = _webTransportService.GetPayItemDetails(input);
                return Json(suggestions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unit Price Search: Failed to get pay item suggestions. Input: '{input}', Input length: {input?.Length ?? 0}, Error: {ex.Message}", ex);
            }
        }


        /// <summary>
        /// Retrieves detailed historical bid data for a specified Pay Item.
        /// </summary>
        /// <param name="number">Pay item number to fetch historical unit price data for.</param>
        /// <returns>JSON result
        [HttpGet]
        [ValidateInput(true)]
        public ActionResult GetPayItemDetails(string number, List<string> contractType, int? months, List<string> contractWorkType, DateTime? startDate, DateTime? endDate, string[] counties, string bidStatus, string[] marketCounties, decimal? minRank, decimal? maxRank, List<string> workTypeNames, string projectNumber, decimal? minBidAmount, decimal? maxBidAmount, string[] district)
        {
            object processedData = null;
            InvalidOperationException refreshExceptionToLog = null;
            
            try
            {
                if (!ValidateBasicInputs(number, months, startDate, endDate, minBidAmount, maxBidAmount, minRank, maxRank))
                {
                    return new HttpStatusCodeResult(400, "Invalid search parameters.");
                }

                if (!CheckBasicRateLimit())
                {
                    return new HttpStatusCodeResult(429, "Too many requests. Please try again later.");
                }

                var selectedCounties = counties?.ToList() ?? new List<string>();
                var historyData = _webTransportService.GetUnitPriceDetails(number, contractType, months ?? 36, contractWorkType, startDate, endDate, counties, bidStatus, marketCounties, minRank, maxRank, workTypeNames, projectNumber, minBidAmount, maxBidAmount, district);
                if (historyData == null)
                    return new HttpNotFoundResult("No bid history found for the specified range.");
               
                System.Collections.Generic.IReadOnlyDictionary<string, decimal> indexByQuarter;
                try
                {
                    indexByQuarter = NHCCIData.IndexByQuarter;
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("Using cached data"))
                {
                    indexByQuarter = NHCCIData.GetCachedIndexByQuarter();
                    refreshExceptionToLog = ex;
                }
               
                foreach (var item in historyData)
                {
                    if (item.l.HasValue)
                    {
                        try
                        {
                            DateTime lettingDate = item.l.Value;

                            decimal adjustedPrice = NHCCIData.CalculateInflationAdjustedPrice(item.b, lettingDate);
                            item.InflationAdjustedPrice = adjustedPrice;

                            string quarterKey = NHCCIData.GetQuarterKey(lettingDate);

                            if (indexByQuarter.TryGetValue(quarterKey, out var lettingDateIndex))
                            {
                                decimal latestIndex = NHCCIData.GetLatestIndex();
                                if (latestIndex > 0m && lettingDateIndex > 0m)
                                {
                                    decimal inflationFactor = latestIndex / lettingDateIndex;
                                    decimal percentIncrease = (inflationFactor - 1) * 100;

                                    item.InflationFactor = inflationFactor;
                                    item.InflationPercentIncrease = percentIncrease;
                                }
                                else
                                {
                                    item.InflationFactor = 1.0m;
                                    item.InflationPercentIncrease = 0.0m;
                                }

                                item.NHCCIQuarter = quarterKey;
                            }
                            else
                            {
                                item.InflationFactor = 1.0m;
                                item.InflationPercentIncrease = 0.0m;
                                item.NHCCIQuarter = "Inflation data not available";
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException($"NHCCI: Failed to calculate inflation adjustment for pay item {item.ri} with letting date {item.l}. Original price: {item.b}, Error: {ex.Message}", ex);
                        }
                    }
                    else
                    {
                        item.InflationAdjustedPrice = item.b;
                        item.InflationFactor = 1.0m;
                        item.InflationPercentIncrease = 0.0m;
                        item.NHCCIQuarter = "Inflation data not available";
                    }
                }
                var filteredData = historyData.Select(item => new
                {
                    ri = item.ri,
                    Quantity = item.Quantity,
                    p = item.p,
                    ProposalType = item.ProposalType,
                    ContractType = item.ContractType,
                    ContractWorkType = item.ContractWorkType,
                    m = item.m,
                    c = item.c,
                    d = item.d,
                    l = item.l,
                    b = item.b,
                    BidStatus = item.BidStatus,
                    PvBidTotal = item.PvBidTotal,
                    ProjectNumber = item.ProjectNumber,
                    Description = item.Description,
                    SupplementalDescription = item.SupplementalDescription,
                    CalculatedUnit = item.CalculatedUnit,
                    ExecutionDate = item.ExecutionDate,
                    VendorName = item.VendorName,
                    FullNameDescription = item.FullNameDescription,
                    Duration = item.Duration,
                    ExecutedDate = item.ExecutedDate,
                    ObsoleteDate = item.ObsoleteDate,
                    BidType = item.BidType,
                    VendorRanking = item.VendorRanking,
                    CategoryDescription = item.CategoryDescription,
                    WorkMixDescription = item.WorkMixDescription,
                    LeadProjectNumber = item.LeadProjectNumber,
                    InflationAdjustedPrice = item.InflationAdjustedPrice,
                    InflationFactor = item.InflationFactor,
                    InflationPercentIncrease = item.InflationPercentIncrease,
                    NHCCIQuarter = item.NHCCIQuarter
                }).ToList();

                processedData = filteredData;
                if (refreshExceptionToLog != null)
                {
                    throw refreshExceptionToLog;
                }
                
                return new JsonResult
                {
                    Data = filteredData,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("NHCCI") && ex.Message.Contains("Using cached data") && processedData != null)
            {
                return new JsonResult
                {
                    Data = processedData,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unit Price Search: Failed to get pay item details. Pay item number: '{number}', Contract types: {string.Join(", ", contractType ?? new List<string>())}, Months: {months}, Start date: {startDate}, End date: {endDate}, Counties: {string.Join(", ", counties ?? new string[0])}, Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Basic input validation to prevent unauthorized data access
        /// </summary>
        private bool ValidateBasicInputs(string number, int? months, DateTime? startDate, DateTime? endDate, decimal? minBidAmount, decimal? maxBidAmount, decimal? minRank, decimal? maxRank)
        {
            try
            {
                if (months.HasValue && (months < 1 || months > 120))
                    return false;
                var now = DateTime.Now;
                var maxPastDate = now.AddYears(-10); 
                var maxFutureDate = now; 

                if (startDate.HasValue && (startDate < maxPastDate || startDate > maxFutureDate))
                    return false;

                if (endDate.HasValue && (endDate < maxPastDate || endDate > maxFutureDate))
                    return false;

                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                    return false;

                if (minBidAmount.HasValue && (minBidAmount < 0 || minBidAmount > 99999999999))
                    return false;

                if (maxBidAmount.HasValue && (maxBidAmount < 0 || maxBidAmount > 99999999999))
                    return false;

                if (minBidAmount.HasValue && maxBidAmount.HasValue && minBidAmount > maxBidAmount)
                    return false;

                if (minRank.HasValue && (minRank < 0 || minRank > 999999999))
                    return false;

                if (maxRank.HasValue && (maxRank < 0 || maxRank > 999999999))
                    return false;

                if (minRank.HasValue && maxRank.HasValue && minRank > maxRank)
                    return false;

                if (!string.IsNullOrEmpty(number) && (number.Length > 50 || !System.Text.RegularExpressions.Regex.IsMatch(number, @"^[A-Za-z0-9\s\-\.]+$")))
                    return false;

                if (!string.IsNullOrEmpty(number) && number.ToLowerInvariant().Contains("script"))
                    return false;

                if (!string.IsNullOrEmpty(number) && number.Contains("<"))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unit Price Search: Failed to validate basic inputs. Pay item number: '{number}', Months: {months}, Start date: {startDate}, End date: {endDate}, Min bid amount: {minBidAmount}, Max bid amount: {maxBidAmount}, Min rank: {minRank}, Max rank: {maxRank}, Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the latest NHCCI quarter information for display purposes.
        /// </summary>
        /// <returns>JSON result with latest quarter key and display format</returns>
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetLatestNHCCIQuarter()
        {
            try
            {
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetNoStore();
                Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
                Response.Cache.AppendCacheExtension("no-cache, no-store, must-revalidate");
                Response.Headers.Add("Pragma", "no-cache");
                Response.Headers.Add("Expires", "0");

                var latestQuarterKey = NHCCIData.GetLatestQuarterKey();
                var latestQuarterDisplay = NHCCIData.GetLatestQuarterDisplay();
                var cacheStatus = NHCCIData.GetCacheStatus();
                var lastRefreshEst = ExtractLastRefreshTime(cacheStatus);
                
                return Json(new
                {
                    quarterKey = latestQuarterKey,
                    display = latestQuarterDisplay,
                    cacheTimestamp = lastRefreshEst?.ToString("yyyy-MM-ddTHH:mm:ss") ?? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                    _cacheBuster = DateTime.UtcNow.Ticks
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unit Price Search: Failed to get latest NHCCI quarter information. Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Extracts the last refresh time from cache status string.
        /// </summary>
        private DateTime? ExtractLastRefreshTime(string cacheStatus)
        {
            try
            {
                if (string.IsNullOrEmpty(cacheStatus))
                    return null;

                var lastRefreshIndex = cacheStatus.IndexOf("Last Refresh: ", StringComparison.OrdinalIgnoreCase);
                if (lastRefreshIndex < 0)
                    return null;

                var startIndex = lastRefreshIndex + "Last Refresh: ".Length;
                var endIndex = cacheStatus.IndexOf(" EST", startIndex);
                if (endIndex < 0)
                    return null;

                var dateTimeString = cacheStatus.Substring(startIndex, endIndex - startIndex);
                if (DateTime.TryParse(dateTimeString, out var dateTime))
                {
                    var estTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    return TimeZoneInfo.ConvertTimeToUtc(dateTime, estTimeZone);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"NHCCI: Failed to extract last refresh time from cache status. Status: '{cacheStatus}', Error: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// rate limiting
        /// </summary>
        private bool CheckBasicRateLimit()
        {
            try
            {
                var clientIp = Request.UserHostAddress;
                var cacheKey = $"RateLimit_{clientIp}";
                var currentTime = DateTime.Now;

                if (HttpContext.Cache[cacheKey] != null)
                {
                    var lastRequest = (DateTime)HttpContext.Cache[cacheKey];
                    var timeDiff = currentTime - lastRequest;

                    if (timeDiff.TotalSeconds < 3)
                    {
                        return false;
                    }
                }
                HttpContext.Cache.Insert(cacheKey, currentTime, null, DateTime.Now.AddMinutes(1), System.Web.Caching.Cache.NoSlidingExpiration);

                return true;
            }
            catch (Exception ex)
            {
                var clientIp = Request?.UserHostAddress ?? "Unknown";
                throw new InvalidOperationException($"Unit Price Search: Failed to check rate limit. Client IP: {clientIp}, Error: {ex.Message}", ex);
            }
        }
    }
}