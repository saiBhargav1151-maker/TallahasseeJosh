using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;
using System.Linq;


namespace Dqe.Web.Controllers
{
    /// <summary>
    /// Controller responsible for handling Unit Price Search functionality,
    /// including pay item autocomplete suggestions and historical bid data retrieval.
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
                System.Diagnostics.Debug.WriteLine($"Error in GetPayItemSuggestions: {ex.Message}");
                return new HttpStatusCodeResult(500, "An error occurred while fetching pay item suggestions.");
            }
        }


        /// <summary>
        /// Retrieves detailed historical bid data for a specified Pay Item.
        /// </summary>
        /// <param name="number">Pay item number to fetch historical unit price data for.</param>
        /// <returns>JSON result
        [HttpGet]
        public ActionResult GetPayItemDetails(string number, List<string> contractType, int? months, List<string> contractWorkType, DateTime? startDate, DateTime? endDate, string[] counties, string bidStatus, string[] marketCounties, decimal? minRank, decimal? maxRank, List<string> workTypeNames, string projectNumber, decimal? minBidAmount, decimal? maxBidAmount, string[] district)
        {
            try
            {
                var selectedCounties = counties?.ToList() ?? new List<string>();
                var historyData = _webTransportService.GetUnitPriceDetails(number, contractType, months ?? 36, contractWorkType, startDate, endDate, counties, bidStatus, marketCounties, minRank, maxRank, workTypeNames, projectNumber, minBidAmount, maxBidAmount, district);
                if (historyData == null)
                    return new HttpNotFoundResult("No bid history found for the specified range.");
               
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
                            if (NHCCIData.IndexByQuarter.ContainsKey(quarterKey))
                            {
                                decimal lettingDateIndex = NHCCIData.IndexByQuarter[quarterKey];
                                decimal latestIndex = NHCCIData.GetLatestIndex();
                                decimal inflationFactor = latestIndex / lettingDateIndex;
                                decimal percentIncrease = (inflationFactor - 1) * 100;

                                item.InflationFactor = inflationFactor;
                                item.InflationPercentIncrease = percentIncrease;
                                item.NHCCIQuarter = quarterKey;
                            }
                        }
                        catch (Exception ex)
                        {
                            item.InflationAdjustedPrice = item.b;
                            item.InflationFactor = 1.0m;
                            item.InflationPercentIncrease = 0.0m;
                            item.NHCCIQuarter = "Unknown";
                            System.Diagnostics.Debug.WriteLine($"Error calculating inflation adjustment: {ex.Message}");
                        }
                    }
                    else
                    {
                        item.InflationAdjustedPrice = item.b;
                        item.InflationFactor = 1.0m;
                        item.InflationPercentIncrease = 0.0m;
                        item.NHCCIQuarter = "Unknown";
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

                return new JsonResult
                {
                    Data = filteredData,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "An error occurred: " + ex.Message);
            }
        }
    }
}