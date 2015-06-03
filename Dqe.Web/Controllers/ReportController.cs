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
        private readonly IPayItemMasterRepository _payItemMasterRepository;
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly ICommandRepository _commandRepository;
        private static string _userName;
        private static string _passWord;
        string _contentType;
        string _extension;
        string _serviceUrl;
        readonly string _environment;

        public ReportController(IWebTransportService webTransportService, IStaffService staffService, IReportRepository reportRepository,
                                IProjectRepository projectRepository, ISsrsConnectionProvider ssrsConnectionProvider, IEnvironmentProvider environmentProvider,
                                IProposalRepository proposalRepository, IPayItemMasterRepository payItemMasterRepository, IDqeUserRepository dqeUserRepository,
                                ICommandRepository commandRepository)
        {
            _webTransportService = webTransportService;
            _staffService = staffService;
            _reportRepository = reportRepository;
            _projectRepository = projectRepository;
            _proposalRepository = proposalRepository;
            _payItemMasterRepository = payItemMasterRepository;
            _dqeUserRepository = dqeUserRepository;
            _commandRepository = commandRepository;
            var reportConnection = ssrsConnectionProvider.GetConnection();
            _userName = reportConnection[0];
            _passWord = reportConnection[1];
            _environment = environmentProvider.GetEnvironment();
#if DEBUG
            _serviceUrl = ConfigurationManager.AppSettings.Get("reportServerUrl");
#else
            _serviceUrl = DetermineUrl();
#endif
        }

        [HttpPost]
        public ActionResult ViewMasterFileReport(FormCollection form)
        {
            var reportFormat = form["reportFormat"];
            var masterFile = Convert.ToInt32(Server.UrlDecode(form["hiddenMasterFile"]));
            var structure = form["structure"] == "on";
            var current = form["current"] == "on";

            var targetUrl = string.Format(_serviceUrl + "/PayItemMasterReport&rs:Command=Render&rs:Format={0}&MasterFileNumber={1}&WithStructure={2}&CurrentOnly={3}", reportFormat, masterFile, structure, current);

            var fileBytes = CallSsrsWebService(targetUrl);

            FillReportFormat(reportFormat);

            return File(fileBytes, _contentType, string.Format("MasterFile{0}.{1}", masterFile, _extension));
        }

        [HttpPost]
        public ActionResult ViewBoeReport(FormCollection form)
        {
            var boeIndex = Convert.ToInt32(Server.UrlDecode(form["hiddenStructureName"]));
            var reportFormat = form["reportFormat"];

            string structureName1;
            string structureName2;
            string structureName3;

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

            var targetUrl = string.Format(_serviceUrl + "/Boe&rs:Command=Render&rs:Format={0}&StructureName1={1}&StructureName2={2}&StructureName3={3}", reportFormat, structureName1, structureName2, structureName3);

            var fileBytes = CallSsrsWebService(targetUrl);

            FillReportFormat(reportFormat);

            return File(fileBytes, _contentType, string.Format("BOE_ChapterIndex{0}.{1}", boeIndex, _extension));
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator })]
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
                fileName = string.Format("SnapshotReport{0}.{1}", estimate.Id, _extension);
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
                                            reportFormat, estimates.First().Id, estimates.Last().Id, staffMemberNames.First(), staffMemberNames.Last());
                fileName = string.Format("SnapshotComparisonReport{0}-{1}.{2}", estimates.First().Id, estimates.Last().Id, _extension);
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
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator })]
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
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator })]
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
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator })]
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
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator })]
        public ActionResult ViewScopeTrackingGraph(FormCollection form)
        {
            var reportFormat = form["reportFormat"];
            var projectNumber = form["hiddenProjectNumber"];

            var targetUrl = string.Format(_serviceUrl + "/ScopeTrackingGraph&rs:Command=Render&rs:Format={0}&ProjectNumber={1}", reportFormat, projectNumber);

            var fileBytes = CallSsrsWebService(targetUrl);

            FillReportFormat(reportFormat);

            return File(fileBytes, _contentType, string.Format("ScopeTrackingGraph{0}.{1}", projectNumber, _extension));
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator })]
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
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator })]
        public ActionResult SetupWorkingProposalSummaryReport(string proposalNumber, int proposalId)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);

            var reportProposal = _reportRepository.GetReportProposal(proposalNumber, ReportProposalLevel.WorkingEstimate);
            if (reportProposal != null)
            {
                if (reportProposal.MyReportLetting != null)
                {
                    var reportLetting = _reportRepository.GetReportLettingByProposalLevel(reportProposal.MyReportLetting.LettingName, ReportProposalLevel.WorkingEstimate);
                    if (reportLetting != null)
                        _commandRepository.Remove(reportLetting);
                }

                _commandRepository.Remove(reportProposal);
                _commandRepository.Flush();
            }

            //build report structure
            _proposalRepository.BuildReportProposal(proposalId, currentDqeUser, _payItemMasterRepository, _reportRepository, _webTransportService, true);

            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = "" });
        }

        [HttpGet]
        public ActionResult SaveLettingAndVendorDataByProposal(string proposalNumber)
        {
            var letting = _webTransportService.GetLettingByProposal(proposalNumber);
            if (letting == null)
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "No Letting number exists for the Proposal provided." }, JsonRequestBehavior.AllowGet);

            letting = _webTransportService.GetLetting(letting.LettingName);

            if (!letting.Proposals.Where(p => p.ProposalNumber == proposalNumber).Any(p => p.ProposalVendors.Any(v => v.Bids.Any())))
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "No bids received for the given proposal." }, JsonRequestBehavior.AllowGet);

            _reportRepository.SaveReport(letting);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SaveLettingAndVendorDataByLetting(string lettingNumber)
        {
            var letting = _webTransportService.GetLetting(lettingNumber);

            var proposals = _reportRepository.GetProposalsInList(letting.Proposals.Select(i => i.ProposalNumber).ToList(), ReportProposalLevel.Official).ToList();

            var authorizedProposals = _reportRepository.GetProposalsInList(letting.Proposals.Select(i => i.ProposalNumber).ToList(), ReportProposalLevel.Authorization).ToList();

            if (!proposals.Any() && !authorizedProposals.Any())
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "No Official or Authorization Estimates exist in DQE for the provided Letting number." }, JsonRequestBehavior.AllowGet);
            }

            if (authorizedProposals.Any())
            {
                if (authorizedProposals.All(p => p.MyReportLetting == null))
                {
                    var reportLetting = new ReportLetting
                    {
                        LettingName = letting.LettingName,
                        LettingDate = letting.LettingDate
                    };

                    foreach (var proposal in authorizedProposals)
                        reportLetting.AddReportProposal(proposal);
                }
                else
                {
                    var reportLetting = _reportRepository.GetReportLettingByProposalLevel(letting.LettingName, ReportProposalLevel.Authorization);

                    foreach (var proposal in proposals.Where(p => p.MyReportLetting == null))
                        reportLetting.AddReportProposal(proposal);
                }
            }

            if (proposals.Any())
                _reportRepository.SaveReport(letting);

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

                //var folder = _environment.ToUpper().StartsWith("P")
                //    ? "Prod"
                //    : _environment.ToUpper().StartsWith("S")
                //        ? "Systest"
                //        : _environment.ToUpper().StartsWith("U")
                //            ? "Unit"
                //            : string.Empty;

                //if (string.IsNullOrWhiteSpace(folder)) return null;

                //var authorizationReportLocation = ConfigurationManager.AppSettings.Get("authorizationReportLocation") + "\\" + folder;

                var authorizationReportLocation = Environment.GetEnvironmentVariable("Interwebshare") + Environment.GetEnvironmentVariable("LEVEL") + "\\AuthEst";

                //write the file bytes to location
                if (!Directory.Exists(authorizationReportLocation))
                    throw new InvalidOperationException(string.Format("Invalid directory {0}", authorizationReportLocation));

                string finalWriteLocation;

                if (p.LettingDate.HasValue)
                {
                    if (
                        !Directory.Exists(string.Format("{0}\\{1}", authorizationReportLocation,
                            p.LettingDate.Value.Year)))
                        Directory.CreateDirectory(string.Format("{0}\\{1}", authorizationReportLocation,
                            p.LettingDate.Value.Year));

                    if (
                        !Directory.Exists(string.Format("{0}\\{1}\\{2}", authorizationReportLocation,
                            p.LettingDate.Value.Year, p.LettingDate.Value.Month)))
                        Directory.CreateDirectory(string.Format("{0}\\{1}\\{2}", authorizationReportLocation,
                            p.LettingDate.Value.Year,
                            string.Format("{0}_{1}",
                                p.LettingDate.Value.Month.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                                CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(p.LettingDate.Value.Month))));

                    finalWriteLocation = string.Format("{0}\\{1}\\{2}\\{3}.{4}", authorizationReportLocation, p.LettingDate.Value.Year,
                        string.Format("{0}_{1}", p.LettingDate.Value.Month.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                            CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(p.LettingDate.Value.Month)), proposal.number, "pdf");

                    System.IO.File.WriteAllBytes(finalWriteLocation, fileBytes);
                }
                else
                {
                    if (!Directory.Exists(string.Format("{0}\\NO_LETTING_DATE", authorizationReportLocation)))
                        Directory.CreateDirectory(string.Format("{0}\\NO_LETTING_DATE", authorizationReportLocation));

                    finalWriteLocation = string.Format("{0}\\NO_LETTING_DATE\\{1}.{2}", authorizationReportLocation, proposal.number, "pdf");

                    System.IO.File.WriteAllBytes(finalWriteLocation, fileBytes);

                }

                if (!System.IO.File.Exists(finalWriteLocation))
                    return new DqeResult(null,
                        new ClientMessage
                        {
                            Severity = ClientMessageSeverity.Error,
                            text = "An error occured and the Authorization Report was not was not sent to the folder location."
                        },
                        JsonRequestBehavior.AllowGet);
            }

            return Json(true, JsonRequestBehavior.AllowGet);
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

            return null;
        }

        private byte[] CallSsrsWebService(string targetUrl)
        {
            var req = (HttpWebRequest)WebRequest.Create(targetUrl);
            req.PreAuthenticate = true;
            req.Proxy = null;
#if DEBUG
            req.Credentials = CredentialCache.DefaultCredentials;
#else
            req.Credentials = new NetworkCredential(_userName, _passWord, ConfigurationManager.AppSettings.Get("rptDomain"));
#endif
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