using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;


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
                if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
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
        /// Retrieves detailed historical bid data for a specified pay item number.
        /// </summary>
        /// <param name="number">Pay item number to fetch historical unit price data for.</param>
        /// <returns>JSON result
        [HttpGet]
        public ActionResult GetPayItemDetails(string number, int months, string contractWorkType, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var historyData = _webTransportService.GetUnitPriceDetails(number, months, contractWorkType, startDate, endDate);
                if (historyData == null)
                    return new HttpNotFoundResult("No bid history found for the specified range.");

                return new JsonResult
                {
                    Data = historyData,
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