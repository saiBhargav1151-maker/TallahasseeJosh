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
    [RemoteRequireHttps]
    public class ReportController : Controller
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

        public ReportController(IWebTransportService webTransportService, IStaffService staffService, IReportRepository reportRepository,
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

        [HttpPost]
        public ActionResult ViewMasterFileReport(FormCollection form)
        {
            var reportFormat = form["reportFormat"];
            var masterFile = Convert.ToInt32(Server.UrlDecode(form["hiddenMasterFile"]));
            var structure = form["structure"] == "on";
            var current = form["current"] == "on";

            var targetUrl = string.Format(_serviceUrl + "/PayItemMasterReport&rs:Command=Render&rs:Format={0}&MasterFileNumber={1}&WithStructure={2}&CurrentOnly={3}", reportFormat, masterFile, structure.ToString().ToLower(), current.ToString().ToLower());

            var fileBytes = CallSsrsWebService(targetUrl);

            FillReportFormat(reportFormat);

            return File(fileBytes, _contentType, string.Format("MasterFile{0}.{1}", masterFile, _extension));
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator, DqeRole.AdminReadOnly })]
        //public ActionResult DownloadStructureData()
        public ActionResult DownloadStructureData(FormCollection form)
        {
            const string reportFormat = "EXCELOPENXML";

            var targetUrl = string.Format(_serviceUrl + "/StructureDataFile&rs:Command=Render&rs:Format={0}", reportFormat);

            var fileBytes = CallSsrsWebService(targetUrl);

            FillReportFormat(reportFormat);

            return File(fileBytes, _contentType, string.Format("Structures.{0}", _extension));
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator, DqeRole.AdminReadOnly })]
        //public ActionResult DownloadStructureData()
        public ActionResult DownloadPayItemData(FormCollection form)
        {
            const string reportFormat = "EXCELOPENXML";
            var specId = form["hiddenSpecBookId"];

            var targetUrl = string.Format(_serviceUrl + "/PayItemDataFile&rs:Command=Render&rs:Format={0}&MasterFileSequence={1}", reportFormat, specId);

            var fileBytes = CallSsrsWebService(targetUrl);

            FillReportFormat(reportFormat);

            return File(fileBytes, _contentType, string.Format("PayItems.{0}", _extension));
        }

        private static bool ValidBoeItem(string index)
        {
            var validBoe = new List<string> { "X", "x" };
            for (var i = 0; i < 11; i++)
            {
                if (i != 10)
                    validBoe.Add(" " + i);

                validBoe.Add(i.ToString());
            }

            if (validBoe.Contains(index))
                return true;

            return false;
        }

        [HttpGet]
        public ActionResult ValidateBoeParameters(string structureType)
        {
            if (!ValidBoeItem(structureType))
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Invalid BOE structure." }, JsonRequestBehavior.AllowGet);

            return new DqeResult(null, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ViewBoeReport(FormCollection form)
        {
            var boeStringIndex = Server.UrlDecode(form["hiddenStructureName"]);

            var reportFormat = form["reportFormat"];

            string structureName1;
            string structureName2;
            string structureName3;

            if (boeStringIndex == "X" || boeStringIndex == "x")
            {
                structureName1 = "X";
                structureName2 = structureName1;
                structureName3 = structureName1;
            }
            else
            {
                var boeIndex = Convert.ToInt32(boeStringIndex);

                if (boeIndex >= 1)
                {
                    if (boeIndex == 1)
                        boeIndex = 10;

                    structureName1 = boeIndex.ToString();
                    structureName2 = structureName1;
                    structureName3 = structureName1;
                }
                else
                {
                    structureName1 = " ";
                    structureName2 = "0";
                    structureName3 = "1";
                }
            }

            var targetUrl = string.Format(_serviceUrl + "/Boe&rs:Command=Render&rs:Format={0}&StructureName1={1}&StructureName2={2}&StructureName3={3}", reportFormat, structureName1, structureName2, structureName3);

            var fileBytes = CallSsrsWebService(targetUrl);

            FillReportFormat(reportFormat);

            return File(fileBytes, _contentType, string.Format("BOE_ChapterIndex{0}.{1}", boeStringIndex, _extension));
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator, DqeRole.AdminReadOnly })]
        public ActionResult ViewProjectItemsReport(FormCollection form)
        {
            var versions = Server.UrlDecode(form["hiddenProjectSnapshotIds"]).Split(',');
            var reportFormat = form["reportFormat"];

            string targetUrl;
            string fileName;

            FillReportFormat(reportFormat);

            if (versions.Length == 1)
            {
                var estimate = _projectRepository.GetEstimate(Convert.ToInt64(versions[0]));
                var staffMember = _staffService.GetStaffById(estimate.MyProjectVersion.VersionOwner.SrsId);
                targetUrl = string.Format(_serviceUrl + "/SnapshotReport&rs:Command=Render&rs:Format={0}&ProjectEstimateId={1}&Owner={2}", reportFormat, versions[0], staffMember.FullName);
                fileName = string.Format("SnapshotReport_P{0}_V{1}_E{2}.{3}", estimate.MyProjectVersion.MyProject.ProjectNumber, estimate.MyProjectVersion.Version, estimate.Estimate, _extension);
            }
            else
            {
                var convertedIntegers = versions.Select(item => Convert.ToInt32(item)).ToList();
                var staffMemberNames = new List<string>();
                var estimates = new List<ProjectEstimate>();
                foreach (var item in convertedIntegers)
                {
                    var estimate = _projectRepository.GetEstimate(item);
                    var staffMember = _staffService.GetStaffById(estimate.MyProjectVersion.VersionOwner.SrsId);
                    staffMemberNames.Add(staffMember.FullName);
                    estimates.Add(estimate);
                }
                estimates = estimates.OrderByDescending(i => i.LastUpdated).ToList();

                targetUrl = string.Format(_serviceUrl + "/SnapshotComparison&rs:Command=Render&rs:Format={0}&NewestProjectEstimateId={1}&OldestProjectEstimateId={2}&NewOwner={3}&OldestOwner={4}",
                                            reportFormat, estimates.First().Id, estimates.Last().Id, estimates.First().MyProjectVersion.VersionOwner.Name, estimates.Last().MyProjectVersion.VersionOwner.Name);
                fileName = string.Format("SnapshotComparisonReport_P{0}_V{1}_E{2}-{3}_V{4}_E{5}.{6}", estimates.First().MyProjectVersion.MyProject.ProjectNumber, estimates.First().MyProjectVersion.Version, estimates.First().Estimate,
                                                                                                    estimates.Last().MyProjectVersion.MyProject.ProjectNumber, estimates.Last().MyProjectVersion.Version, estimates.Last().Estimate, _extension);
            }

            var fileBytes = CallSsrsWebService(targetUrl);

            return File(fileBytes, _contentType, fileName);
        }

        [HttpPost]
        public ActionResult ViewProposalSummaryReport(FormCollection form)
        {
            var currentUser = (DqeIdentity)User.Identity;

            var proposalNumber = form["proposalNumber"];
            var estimateType = form["estimateType"];
            var reportFormat = form["reportFormat"];

            var targetUrl = string.Format(_serviceUrl + "/DetailEstimate&rs:Command=Render&rs:Format={0}&ProposalNumber={1}&ProposalLevel={2}&UserName={3}", reportFormat, proposalNumber, estimateType, currentUser.Name);

            var fileBytes = CallSsrsWebService(targetUrl);

            FillReportFormat(reportFormat);

            return File(fileBytes, _contentType, string.Format("ProposalSummary{0}.{1}", proposalNumber, _extension));
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator, DqeRole.AdminReadOnly })]
        public ActionResult ViewUnbalancedItemsReport(FormCollection form)
        {
            var proposalNumber = form["proposalNumber"];
            var showEstimate = form["showEstimate"];
            var reportFormat = form["reportFormat"];
            var sort = form["sort"];

            var targetUrl = string.Format(_serviceUrl + "/UnbalancedItems&rs:Command=Render&rs:Format={0}&ProposalNumber={1}&ShowEstimate={2}&Sort={3}", reportFormat, proposalNumber, showEstimate, sort);

            var fileBytes = CallSsrsWebService(targetUrl);

            FillReportFormat(reportFormat);

            return File(fileBytes, _contentType, string.Format("UnbalancedItemsReport{0}.{1}", proposalNumber, _extension));
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator, DqeRole.AdminReadOnly })]
        public ActionResult ViewSummaryOfLettingReport(FormCollection form)
        {
            var lettingNumber = form["lettingNumber"];
            var reportFormat = form["reportFormat"];

            var targetUrl = string.Format(_serviceUrl + "/ExecutiveSummaryOfLetting&rs:Command=Render&rs:Format={0}&LettingNumber={1}&ProposalLevel={2}", reportFormat, lettingNumber, "O");

            var fileBytes = CallSsrsWebService(targetUrl);

            FillReportFormat(reportFormat);

            return File(fileBytes, _contentType, string.Format("ExecutiveSummaryOfLetting{0}.{1}", lettingNumber, _extension));
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator, DqeRole.AdminReadOnly })]
        public ActionResult ViewBidToleranceReport(FormCollection form)
        {
            var lettingNumber = form["lettingNumber"];
            var reportFormat = form["reportFormat"];

            var targetUrl = string.Format(_serviceUrl + "/BidTolerance&rs:Command=Render&rs:Format={0}&LettingNumber={1}", reportFormat, lettingNumber);

            var fileBytes = CallSsrsWebService(targetUrl);

            FillReportFormat(reportFormat);

            return File(fileBytes, _contentType, string.Format("EstimateTolerancesForLetting{0}.{1}", lettingNumber, _extension));
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator, DqeRole.StateReviewer, DqeRole.DistrictReviewer, DqeRole.AdminReadOnly })]
        public ActionResult ViewScopeTrackingGraph(FormCollection form)
        {
            var reportFormat = form["reportFormat"];
            var projectNumber = form["hiddenProjectNumber"];

            var targetUrl = string.Format(_serviceUrl + "/ScopeTrackingGraph&rs:Command=Render&rs:Format={0}&ProjectNumber={1}", reportFormat, projectNumber);

            var fileBytes = CallSsrsWebService(targetUrl);

            FillReportFormat(reportFormat);

            return File(fileBytes, _contentType, string.Format("ScopeTrackingGraph{0}.{1}", projectNumber, _extension));
        }

        /// <summary>
        /// NOT YET IMPLEMENTED. The graphing got moved to the next phase of enhancements. MB.
        /// This returns a report with Reviews included. 
        /// </summary>
        /// <param name="form"></param>
        /// <returns><returns><see cref="ActionResult"/></returns>
        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator, DqeRole.StateReviewer, DqeRole.DistrictReviewer })]
        public ActionResult ViewReviewTrackingGraph(FormCollection form)
        {
            var reportFormat = form["reportFormat"];
            var projectNumber = form["hiddenProjectNumber"];

            var targetUrl = string.Format(_serviceUrl + "/ScopeReviewTrackingGraph&rs:Command=Render&rs:Format={0}&ProjectNumber={1}", reportFormat, projectNumber);

            var fileBytes = CallSsrsWebService(targetUrl);

            FillReportFormat(reportFormat);

            return File(fileBytes, _contentType, string.Format("ScopeTrackingGraph{0}.{1}", projectNumber, _extension));
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator, DqeRole.AdminReadOnly })]
        public ActionResult SetupProposalSummaryReport(string proposalNumber)
        {
            var letting = _webTransportService.GetLettingByProposal(proposalNumber);

            //We might not have a letting yet for the proposal
            if (letting != null)
            {
                letting = _webTransportService.GetLetting(letting.LettingName);

                var dqeOfficialLetting = _reportRepository.GetReportLettingByProposalLevel(letting.LettingName, ReportProposalLevel.Official);
                var dqeAuthorizationLetting = _reportRepository.GetReportLettingByProposalLevel(letting.LettingName, ReportProposalLevel.Authorization);

                if (LettingRebuildChecks(letting, dqeOfficialLetting) || LettingRebuildChecks(letting, dqeAuthorizationLetting))
                {
                    var masterFiles = _masterFileRepository.GetAll();
                    var latest = masterFiles.Where(i => i.FileNumber < 90).Max(f => f.FileNumber).ToString();
                    var payItems = _payItemMasterRepository.GetAll(latest).ToList();

                    //delete letting data
                    _reportRepository.DeleteLettingData(letting, false);

                    //rebuild
                    _reportRepository.RebuildReportStructure(letting, null, null, false, payItems);
                }
            }
            else
            {
                var authorizedReportProposal = _reportRepository.GetReportLettingByProposal(proposalNumber, ReportProposalLevel.Authorization);
                if (authorizedReportProposal != null)
                {
                    authorizedReportProposal.ClearProposalVendors();
                    if (authorizedReportProposal.MyReportLetting != null)
                        authorizedReportProposal.MyReportLetting = null;
                }

                var officialReportProposal = _reportRepository.GetReportLettingByProposal(proposalNumber, ReportProposalLevel.Official);
                if (officialReportProposal != null)
                {
                    officialReportProposal.ClearProposalVendors();
                    if (officialReportProposal.MyReportLetting != null)
                        officialReportProposal.MyReportLetting = null;
                }
            }

            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = "" });
        }

        private static bool AreBidsComplete(Domain.Model.Wt.Letting letting, IEnumerable<ReportProposal> officialProposals)
        {
            foreach (var proposal in officialProposals)
            {
                var wtLettingProposal = letting.Proposals.First(i => i.ProposalNumber == proposal.ProposalNumber);

                if (wtLettingProposal.ProposalVendors.Any() && wtLettingProposal.ProposalVendors.Any(b => b.Bids.Any()))
                {
                    foreach (var vendor in wtLettingProposal.ProposalVendors.Where(b => b.Bids.Any()))
                    {
                        if (vendor.Bids.Select(bid => proposal.ReportProposalItems.FirstOrDefault(i => i.LineNumber == bid.MyProposalItem.LineNumber)).Any(myProposalItem => myProposalItem == null))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator, DqeRole.AdminReadOnly, DqeRole.DistrictReviewer, DqeRole.StateReviewer })]
        public ActionResult ViewWorkingProposalSummaryReport(FormCollection form)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var proposalNumber = form["hiddenProposalNumber"];
            var reportFormat = form["reportFormat"];

            var targetUrl = string.Format(_serviceUrl + "/DetailEstimate&rs:Command=Render&rs:Format={0}&ProposalNumber={1}&ProposalLevel={2}&UserName={3}", reportFormat, proposalNumber, "W", currentUser.Name);

            var fileBytes = CallSsrsWebService(targetUrl);

            FillReportFormat(reportFormat);

            return File(fileBytes, _contentType, string.Format("ProposalSummary{0}.{1}", proposalNumber, _extension));
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator, DqeRole.AdminReadOnly, DqeRole.DistrictReviewer, DqeRole.StateReviewer })]
        public ActionResult SetupWorkingProposalSummaryReport(string proposalNumber, int proposalId)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);

            var proposal = _proposalRepository.GetById(proposalId);
            foreach (var section in proposal.SectionGroups)
            {
                foreach (var item in section.ProposalItems)
                {
                    if (!item.ProjectItems.Any())
                    {
                        return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Proposal structure is invalid in Project Pre-Construction.  Proposal items without project items were found." });
                    }
                }
            }

            var letting = _webTransportService.GetLettingByProposal(proposalNumber);

            //build report structure
            _proposalRepository.BuildReportProposal(proposalId, currentDqeUser, _payItemMasterRepository, _reportRepository, _webTransportService, true);

            //We might not have a letting yet for the proposal
            if (letting != null)
            {
                letting = _webTransportService.GetLetting(letting.LettingName);

                var estimateProposal = _reportRepository.GetReportProposal(proposalNumber, ReportProposalLevel.WorkingEstimate);

                var reportLetting = new ReportLetting
                {
                    LettingName = letting.LettingName,
                    LettingDate = letting.LettingDate
                };

                reportLetting.AddReportProposal(estimateProposal);

                _commandRepository.Add(reportLetting);
            }

            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = "" });
        }

        [HttpGet]
        public ActionResult SaveLettingAndVendorDataByProposal(string proposalNumber)
        {
            var letting = _webTransportService.GetLettingByProposal(proposalNumber);
            if (letting == null)
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "No Letting number exists for the Proposal provided." }, JsonRequestBehavior.AllowGet);

            letting = _webTransportService.GetResponsiveLettings(letting.LettingName);

            if (letting == null || !letting.Proposals.Where(p => p.ProposalNumber == proposalNumber).Any(p => p.ProposalVendors.Any(v => v.Bids.Any())))
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "No responsive bids for the provided proposal." }, JsonRequestBehavior.AllowGet);

            var dqeLetting = _reportRepository.GetReportLettingByProposalLevel(letting.LettingName, ReportProposalLevel.Official);

            if (Rebuild(letting, dqeLetting))
            {
                //delete letting data
                _reportRepository.DeleteLettingData(letting, true);

                //check for proposals which have not had bids loaded
                var officialProposals =
                    _reportRepository.GetProposalsInList(letting.Proposals.Select(i => i.ProposalNumber).ToList(),
                        ReportProposalLevel.Official).ToList();
                if (!AreBidsComplete(letting, officialProposals))
                    return new DqeResult(null,
                        new ClientMessage
                        {
                            Severity = ClientMessageSeverity.Error,
                            text = "Report cannot be run due to an incomplete set of bids."
                        }, JsonRequestBehavior.AllowGet);

                var masterFiles = _masterFileRepository.GetAll();
                var latest = masterFiles.Where(i => i.FileNumber < 90).Max(f => f.FileNumber).ToString();
                var payItems = _payItemMasterRepository.GetAll(latest).ToList();

                //rebuild
                _reportRepository.RebuildReportStructure(letting, officialProposals, null, true, payItems);
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Information, text = "Completed rebuild of DQE reporting data for letting " + letting.LettingName, ttl=10000 }, JsonRequestBehavior.AllowGet);
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        private static bool LettingRebuildChecks(Domain.Model.Wt.Letting wtLetting, ReportLetting dqeLetting)
        {
            if (dqeLetting == null)
                return true;

            //check letting date matches
            if (wtLetting.LettingDate != dqeLetting.LettingDate)
                return true;

            //check that all dqe proposals are in wt proposals
            foreach (var proposal in dqeLetting.ReportProposals)
            {
                var wtProposal = wtLetting.Proposals.FirstOrDefault(p => p.ProposalNumber == proposal.ProposalNumber);

                if (wtProposal == null)
                    return true;
            }

            return false;
        }

        private static bool Rebuild(Domain.Model.Wt.Letting wtLetting, ReportLetting dqeLetting)
        {
            if (LettingRebuildChecks(wtLetting, dqeLetting))
                return true;

            //check that all vendor and bid data matches
            foreach (var proposal in dqeLetting.ReportProposals)
            {
                var wtProposal = wtLetting.Proposals.First(p => p.ProposalNumber == proposal.ProposalNumber);

                if (wtProposal == null)
                    return true;

                if (proposal.ReportProposalVendors.Count() != wtProposal.ProposalVendors.Count(v => v.Bids.Any()))
                    return true;

                foreach (var vendor in proposal.ReportProposalVendors)
                {
                    var wtVendor = wtProposal.ProposalVendors.FirstOrDefault(v => v.MyRefVendor.VendorName == vendor.Name);
                    if (wtVendor == null)
                        return true;

                    if (vendor.ReportVendorBids.Count() != wtVendor.Bids.Count())
                        return true;

                    foreach (var bid in vendor.ReportVendorBids)
                    {
                        var wtBid = wtVendor.Bids.FirstOrDefault(b => b.MyProposalItem.LineNumber == bid.MyReportProposalItem.LineNumber);

                        if (wtBid == null)
                            return true;

                        var wtBidPrice = bid.MyReportProposalItem.Unit.StartsWith("LS") ? wtBid.BidPrice / bid.MyReportProposalItem.Quantity : wtBid.BidPrice;
                        if (bid.BidPrice != Decimal.Round((Decimal)wtBidPrice, 5))
                            return true;
                    }

                    foreach (var milestoneBid in vendor.ReportMilestoneBids)
                    {
                        var wtMilestone = wtVendor.BidTimes.FirstOrDefault(b => b.MyMilestone.Id == milestoneBid.MyReportProposalMilestone.WtId);

                        if (wtMilestone == null)
                            return true;

                        if (wtMilestone.CalculatedPrice != milestoneBid.BidPrice * milestoneBid.NumberOfDaysBid)
                            return true;
                    }
                }
            }

            //check letting report data rebuild cache indicator to see if already rebuild for letting 
            //if not in rebuild cache indicator, return rebuild true and add to cache
            //if in rebuild cache indicator, letting has already been rebuilt, return rebuild false
            if (!RebuildReportDataCache.ContainsKey(dqeLetting.LettingName))
            {
                RebuildReportDataCache.Add(dqeLetting.LettingName, dqeLetting.LettingName);
                return true;
            }
            return false;
        }

        [HttpGet]
        public ActionResult SaveLettingAndVendorDataByLettingForEstimateTolerance(string lettingNumber)
        {
            var letting = _webTransportService.GetLetting(lettingNumber);

            var dqeLetting = _reportRepository.GetReportLettingByProposalLevel(letting.LettingName, ReportProposalLevel.Official);

            if (Rebuild(letting, dqeLetting))
            {
                var officialProposals = _reportRepository.GetProposalsInList(letting.Proposals.Select(i => i.ProposalNumber).ToList(), ReportProposalLevel.Official).ToList();

                var authorizedProposals = _reportRepository.GetProposalsInList(letting.Proposals.Select(i => i.ProposalNumber).ToList(), ReportProposalLevel.Authorization).ToList();

                if (!officialProposals.Any() && !authorizedProposals.Any())
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "No Official or Authorization Estimates exist in DQE for the provided Letting number." }, JsonRequestBehavior.AllowGet);
                }

                //delete letting data
                _reportRepository.DeleteLettingData(letting, true);

                //check for proposals which have not had bids loaded
                if (!AreBidsComplete(letting, officialProposals))
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Report cannot be run due to an incomplete set of bids." }, JsonRequestBehavior.AllowGet);

                var masterFiles = _masterFileRepository.GetAll();
                var latest = masterFiles.Where(i => i.FileNumber < 90).Max(f => f.FileNumber).ToString();
                var payItems = _payItemMasterRepository.GetAll(latest).ToList();

                //rebuild
                _reportRepository.RebuildReportStructure(letting, officialProposals, authorizedProposals, true, payItems);
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Information, text = "Completed rebuild of DQE reporting data for letting " + letting.LettingName, ttl=10000 }, JsonRequestBehavior.AllowGet);
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SaveLettingAndVendorDataByLettingForExecutiveSummary(string lettingNumber)
        {
            var letting = _webTransportService.GetResponsiveLettings(lettingNumber);
            if (letting == null)
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "No responsive bids for the provided Letting Number." }, JsonRequestBehavior.AllowGet);

            var dqeLetting = _reportRepository.GetReportLettingByProposalLevel(letting.LettingName, ReportProposalLevel.Official);

            if (Rebuild(letting, dqeLetting))
            {
                var officialProposals = _reportRepository.GetProposalsInList(letting.Proposals.Select(i => i.ProposalNumber).ToList(), ReportProposalLevel.Official).ToList();

                var authorizedProposals = _reportRepository.GetProposalsInList(letting.Proposals.Select(i => i.ProposalNumber).ToList(), ReportProposalLevel.Authorization).ToList();

                if (!officialProposals.Any() && !authorizedProposals.Any())
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "No Official or Authorization Estimates exist in DQE for the provided Letting number." }, JsonRequestBehavior.AllowGet);
                }

                if (!letting.Proposals.Any(p => p.ProposalVendors.Any(v => v.Bids.Any())))
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "No bids received for the given Letting." }, JsonRequestBehavior.AllowGet);

                //delete letting data
                _reportRepository.DeleteLettingData(letting, true);

                //check for proposals which have not had bids loaded
                if (!AreBidsComplete(letting, officialProposals))
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Report cannot be run due to an incomplete set of bids." }, JsonRequestBehavior.AllowGet);

                var masterFiles = _masterFileRepository.GetAll();
                var latest = masterFiles.Where(i => i.FileNumber < 90).Max(f => f.FileNumber).ToString();
                var payItems = _payItemMasterRepository.GetAll(latest).ToList();

                //rebuild
                _reportRepository.RebuildReportStructure(letting, officialProposals, authorizedProposals, true, payItems);
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Information, text = "Completed rebuild of DQE reporting data for letting " + letting.LettingName, ttl=10000 }, JsonRequestBehavior.AllowGet);
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetLettings(string lettingNumber)
        {
            var lettings = _webTransportService.GetLettingNames(lettingNumber).ToList();

            return Json(lettings
                .Select(i => new
                {
                    id = i.Id,
                    number = i.LettingName
                }),
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetDqeReportProposals(string proposalNumber, string estimateType)
        {
            var proposals = _reportRepository.GetReportProposals(proposalNumber, ReportProposalLevel.Official);
            return Json(proposals
                .Select(i => new
                {
                    id = i.Id,
                    number = i.ProposalNumber
                }),
                JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SendAuthorizationReport(dynamic proposal)
        {
#if DEBUG
            return Json(true, JsonRequestBehavior.AllowGet);
#endif
            if (Convert.ToBoolean(ConfigurationManager.AppSettings.Get("sendAuthorizationReport")))
            {
                var currentUser = (DqeIdentity)User.Identity;
                var p = (Proposal)_proposalRepository.GetById(proposal.id);

                var targetUrl = string.Format(_serviceUrl + "/DetailEstimate&rs:Command=Render&rs:Format={0}&ProposalNumber={1}&ProposalLevel={2}&UserName={3}", "PDF", proposal.number, "A", currentUser.Name);

                var fileBytes = CallSsrsWebService(targetUrl);

                var folder = _environment.ToUpper().StartsWith("P")
                     ? string.Empty
                     : _environment.ToUpper().StartsWith("S")
                         ? @"\Systest"
                         : _environment.ToUpper().StartsWith("U")
                             ? @"\Unit"
                             : string.Empty;

                // Commented out the line below to allow just the UNC path in web.config for production
                //if (string.IsNullOrWhiteSpace(folder)) return null;

                var authorizationReportLocation = ConfigurationManager.AppSettings.Get("authorizationReportLocation") + folder;
                
                //var authorizationReportLocation = Environment.GetEnvironmentVariable("Interwebshare") + Environment.GetEnvironmentVariable("LEVEL") + "\\AuthEst";

                //write the file bytes to location
                if (!Directory.Exists(authorizationReportLocation))
                    throw new InvalidOperationException(string.Format("Invalid directory {0}", authorizationReportLocation));

                string finalWriteLocation;

                if (p.LettingDate.HasValue)
                {
                    DetermineAndCreateAuthorizationFolder(authorizationReportLocation, (DateTime)p.LettingDate);

                    finalWriteLocation = string.Format("{0}\\{1}\\{2}\\{3}.{4}", authorizationReportLocation, p.LettingDate.Value.Year,
                        string.Format("{0}_{1}", p.LettingDate.Value.Month.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                            CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(p.LettingDate.Value.Month)), proposal.number, "pdf");

                    System.IO.File.WriteAllBytes(finalWriteLocation, fileBytes);
                }
                else
                {
                    var reportProposal = _reportRepository.GetReportProposal(p.ProposalNumber, ReportProposalLevel.Authorization);
                    var leadingProject = reportProposal.ReportProjects.FirstOrDefault(rp => rp.Primary == "Y");
                    if (leadingProject != null && leadingProject.LettingDate.HasValue)
                    {
                        DetermineAndCreateAuthorizationFolder(authorizationReportLocation, (DateTime) leadingProject.LettingDate);

                        finalWriteLocation = string.Format("{0}\\{1}\\{2}\\{3}.{4}", authorizationReportLocation, leadingProject.LettingDate.Value.Year,
                        string.Format("{0}_{1}", leadingProject.LettingDate.Value.Month.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                            CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(leadingProject.LettingDate.Value.Month)), proposal.number, "pdf");
                    }
                    else
                    {
                        if (!Directory.Exists(string.Format("{0}\\NO_LETTING_DATE", authorizationReportLocation)))
                            Directory.CreateDirectory(string.Format("{0}\\NO_LETTING_DATE", authorizationReportLocation));

                        finalWriteLocation = string.Format("{0}\\NO_LETTING_DATE\\{1}.{2}", authorizationReportLocation, proposal.number, "pdf");
                    }

                    System.IO.File.WriteAllBytes(finalWriteLocation, fileBytes);
                }

                if (!System.IO.File.Exists(finalWriteLocation))
                    return new DqeResult(null,
                        new ClientMessage
                        {
                            Severity = ClientMessageSeverity.Error,
                            text = "An error occurred and the Authorization Report was not was not sent to the folder location."
                        },
                        JsonRequestBehavior.AllowGet);
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        private static void DetermineAndCreateAuthorizationFolder(string authorizationReportLocation, DateTime lettingDate)
        {
            if (
                !Directory.Exists(string.Format("{0}\\{1}", authorizationReportLocation,
                    lettingDate.Year)))
                Directory.CreateDirectory(string.Format("{0}\\{1}", authorizationReportLocation,
                    lettingDate.Year));

            if (
                !Directory.Exists(string.Format("{0}\\{1}\\{2}", authorizationReportLocation,
                    lettingDate.Year, lettingDate.Month)))
                Directory.CreateDirectory(string.Format("{0}\\{1}\\{2}", authorizationReportLocation,
                   lettingDate.Year,
                    string.Format("{0}_{1}",
                        lettingDate.Month.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                        CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(lettingDate.Month))));
        }

        private static byte[] ReadFully(Stream input)
        {
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private void FillReportFormat(string reportFormat)
        {
            if (reportFormat != "PDF")
            {
                _contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                _extension = "xlsx";
            }
            else
            {
                _contentType = "pdf";
                _extension = "pdf";
            }
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

        private byte[] CallSsrsWebService(string targetUrl)
        {
            var req = (HttpWebRequest)WebRequest.Create(targetUrl);
            req.PreAuthenticate = true;
            req.Proxy = null;
            req.Credentials = new NetworkCredential(_userName, _passWord, ConfigurationManager.AppSettings.Get("rptDomain"));

            byte[] fileBytes;

            try
            {
                using (var httpWResp = (HttpWebResponse)req.GetResponse())
                {
                    using (var fStream = httpWResp.GetResponseStream())
                    {
                        fileBytes = ReadFully(fStream);
                    }
                }
            }
            catch (Exception ex)
            {
                var serviceEx = new InvalidOperationException(targetUrl, ex);
                throw serviceEx;
            }
            return fileBytes;
        }
    }
}