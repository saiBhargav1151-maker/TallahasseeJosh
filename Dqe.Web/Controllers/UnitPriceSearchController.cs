using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Dqe.ApplicationServices;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;
using Dqe.Domain.Model.Reports;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;
using Dqe.Domain.Services;
using Dqe.Web.ActionResults;
using Dqe.Web.Attributes;

namespace Dqe.Web.Controllers
{
    public class UnitPriceSearchController : Controller
    {
        private readonly IWebTransportService _webTransportService;
        private readonly IStaffService _staffService;
        private readonly IReportRepository _reportRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProposalRepository _proposalRepository;
        private readonly IMasterFileRepository _masterFileRepository;
        private readonly IPayItemMasterRepository _payItemMasterRepository;
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly ICommandRepository _commandRepository;
        private static string _userName;
        private static string _passWord;
        string _contentType;
        string _extension;
        string _serviceUrl;
        readonly string _environment;

        public static readonly Dictionary<string, string> RebuildReportDataCache =
        new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            //{"lettingID", "lettingID"}
        };
        public UnitPriceSearchController(IWebTransportService webTransportService, IStaffService staffService, IReportRepository reportRepository,
                                IProjectRepository projectRepository, ISsrsConnectionProvider ssrsConnectionProvider, IEnvironmentProvider environmentProvider,
                                IProposalRepository proposalRepository, IMasterFileRepository masterFileRepository, IPayItemMasterRepository payItemMasterRepository,
                                IDqeUserRepository dqeUserRepository, ICommandRepository commandRepository)
        {
            _webTransportService = webTransportService;
            _staffService = staffService;
            _reportRepository = reportRepository;
            _projectRepository = projectRepository;
            _proposalRepository = proposalRepository;
            _masterFileRepository = masterFileRepository;
            _payItemMasterRepository = payItemMasterRepository;
            _dqeUserRepository = dqeUserRepository;
            _commandRepository = commandRepository;
            var reportConnection = ssrsConnectionProvider.GetConnection();
            _userName = reportConnection[0];
            _passWord = reportConnection[1];
            _environment = environmentProvider.GetEnvironment();
            _serviceUrl = DetermineUrl();
        }
        private string DetermineUrl()
        {
            if (_environment.ToUpper().StartsWith(ApplicationConstants.EnvironmentLevel.UnitTest))
                return ConfigurationManager.AppSettings.Get("reportServerUrlDotUnit");
            if (_environment.ToUpper().StartsWith(ApplicationConstants.EnvironmentLevel.SystemTest))
                return ConfigurationManager.AppSettings.Get("reportServerUrlDotSystem");
            if (_environment.ToUpper().StartsWith(ApplicationConstants.EnvironmentLevel.Production))
                return ConfigurationManager.AppSettings.Get("reportServerUrlDotProduction");
            if (_environment.ToUpper().StartsWith(ApplicationConstants.EnvironmentLevel.WorkStationLocal))
                return ConfigurationManager.AppSettings.Get("reportServerUrl");
            return null;
        }

        /*[HttpGet]*/
        /*public ActionResult GetUnitPriceDetails(string number)
        {
           
            var proposal = _webTransportService.GetUnitPriceSearch(number);
            if (proposal == null)
                return new HttpNotFoundResult("Proposal not found");
           
           *//* var obj = new { proposal.ProposalNumber, proposal.ProposalType, proposal.ProposalStatus };*//*
            return Json(proposal ,JsonRequestBehavior.AllowGet);

          
        }*/

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
                // Optional: Log the error for diagnostics
                System.Diagnostics.Debug.WriteLine($"Error in GetPayItemSuggestions: {ex.Message}");
                return new HttpStatusCodeResult(500, "An error occurred while fetching pay item suggestions.");
            }
        }


        /* [HttpGet]*/
        /*public ActionResult GetUnitPriceDetails(string number)
        {
            try
            {
                var historyData = _webTransportService.GetUnitPriceDetails(number);
                *//*var letting = _webTransportService.GetLettingByProposal(proposalNumber);*//*
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
                // Optionally log the exception here
                return new HttpStatusCodeResult(500, "An error occurred: " + ex.Message);
            }
        }*/

        [HttpGet]
        public ActionResult GetPayItemDetails(string number)
        {
            try
            {
                var historyData = _webTransportService.GetUnitPriceDetails(number);
                /*var letting = _webTransportService.GetLettingByProposal(proposalNumber);*/
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
                // Optionally log the exception here
                return new HttpStatusCodeResult(500, "An error occurred: " + ex.Message);
            }
        }
        


        /*[HttpGet]
        public ActionResult GetUnitPriceDetails(string proposalNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(proposalNumber))
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Proposal number is required");

                var proposal = _webTransportService.GetUnitPrice(proposalNumber);

                if (proposal == null)
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Proposal not found");

                var data = proposal.Sections
     .SelectMany(s => s.ProposalItems)
     .Select(item => new
     {
         ProposalNumber = proposal.ProposalNumber,
         ProjectNumber = proposal.Projects.FirstOrDefault()?.ProjectNumber,
         SpecYear = proposal.Projects.FirstOrDefault()?.SpecBook,
         LettingDate = proposal.MyLetting?.LettingDate,
         ItemName = item.MyRefItem?.Name,
         Description = item.MyRefItem?.Description,
         SuplementatryDescription = item.SupplementalDescription,
         Units = item.MyRefItem?.CalculatedUnit,
         Quantity = item.Quantity,
         ProposalType = proposal.ProposalType,
         ContractType = proposal.ContractType,
         ContractWorkType = proposal.ContractWorkType,
         Days = proposal.Milestones.FirstOrDefault()?.NumberOfUnits ?? 0,
         County = proposal.County?.Description,
         District = proposal.District?.Description,
         Bids = item.Bids?.Select(bid => new
         {
             BidderName = bid.MyProposalVendor?.MyRefVendor?.VendorName,
             Awarded = bid.MyProposalVendor?.Awarded,
             BidPrice = bid.BidPrice,
             BidTotal = bid.MyProposalVendor?.BidTotal,
             //BidderRank = bid.MyProposalVendor?.
         }).ToList()
     });


                return new DqeResult(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }*/



    }
}