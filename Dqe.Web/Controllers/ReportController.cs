using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Dqe.ApplicationServices;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;
using Dqe.Domain.Model.Reports;
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
        private static string _userName;
        private static string _passWord;
        string _contentType;
        string _extension;
        string _serviceUrl;
        readonly string _environment;

        public ReportController(IWebTransportService webTransportService, IStaffService staffService, IReportRepository reportRepository, 
                                IProjectRepository projectRepository, ISsrsConnectionProvider ssrsConnectionProvider, IEnvironmentProvider environmentProvider)
        {
            _webTransportService = webTransportService;
            _staffService = staffService;
            _reportRepository = reportRepository;
            _projectRepository = projectRepository;
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
                fileName = string.Format("SnapshotReport{0}.{1}", versions[0], _extension);
            }
            else
            {
                var convertedIntegers = versions.Select(item => Convert.ToInt32(item)).ToList();
                var staffMemberNames = new List<string>();
                foreach (var item in convertedIntegers)
                {
                    var estimate = _projectRepository.GetEstimate(item);
                    var staffMember = _staffService.GetStaffById(estimate.MyProjectVersion.VersionOwner.SrsId);
                    staffMemberNames.Add(staffMember.FullName);
                }

                targetUrl = string.Format(_serviceUrl + "/SnapshotComparison&rs:Command=Render&rs:Format={0}&NewestProjectEstimateId={1}&OldestProjectEstimateId={2}&NewOwner={3}&OldestOwner={4}", 
                                            reportFormat, convertedIntegers.Max(), convertedIntegers.Min(), staffMemberNames.First(), staffMemberNames.Last());
                fileName = string.Format("SnapshotComparisonReport{0}-{1}.{2}", versions[0], versions[1], _extension);
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

            return File(fileBytes, _contentType, string.Format("UnbalancedItems{0}.{1}", proposalNumber, _extension));
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator })]
        public ActionResult ViewSummaryOfLettingReport(FormCollection form)
        {
            var lettingNumber = form["lettingNumber"];
            var reportFormat = form["reportFormat"];

            var targetUrl = string.Format(_serviceUrl + "/ExecutiveSummaryOfLetting&rs:Command=Render&rs:Format={0}&LettingNumber={1}&ProposalLevel=2", reportFormat, lettingNumber);

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

            return File(fileBytes, _contentType, string.Format("BidTolerance{0}.{1}", lettingNumber, _extension));
        }

        [HttpGet]
        public ActionResult SaveLettingAndVendorDataByProposal(string proposalNumber)
        {
            var letting = _webTransportService.GetLettingByProposal(proposalNumber);

            letting = _webTransportService.GetLetting(letting.LettingName);

            _reportRepository.SaveReport(letting);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SaveLettingAndVendorDataByLetting(string lettingNumber)
        {
            var letting = _webTransportService.GetLetting(lettingNumber);

            var proposals = _reportRepository.GetProposalsInList(letting.Proposals.Select(i => i.ProposalNumber).ToList()).ToList();

            if (!proposals.Any())
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "No proposals exist in DQE for the provided Letting number." }, JsonRequestBehavior.AllowGet);
            }

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
        public ActionResult GetDqeReportProposals(string proposalNumber, int estimateType)
        {
            var proposals = _reportRepository.GetReportProposals(proposalNumber, (ReportProposalLevel)estimateType);
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
            if (Convert.ToBoolean(ConfigurationManager.AppSettings.Get("sendAuthorizationReport")))
            {
                var currentUser = (DqeIdentity) User.Identity;

                var targetUrl = string.Format(_serviceUrl + "/DetailEstimate&rs:Command=Render&rs:Format={0}&ProposalNumber={1}&ProposalLevel={2}&UserName={3}", "PDF", proposal.number, 1, currentUser.Name);

                var fileBytes = CallSsrsWebService(targetUrl);

                //write the file bytes to location
                if (!Directory.Exists(@"C:\TestFile"))
                    Directory.CreateDirectory(@"C:\TestFile");

                System.IO.File.WriteAllBytes(
                    @"C:\TestFile\" + string.Format("AuthorizationEstimate{0}.{1}", proposal.number, ".pdf"), fileBytes);
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
            if (_environment == ApplicationConstants.EnvironmentLevel.UnitTest)
                return ConfigurationManager.AppSettings.Get("reportServerUrlDotUnit");
            if (_environment == ApplicationConstants.EnvironmentLevel.SystemTest)
                return ConfigurationManager.AppSettings.Get("reportServerUrlDotStage");

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

            using (var httpWResp = (HttpWebResponse)req.GetResponse())
            {
                using (var fStream = httpWResp.GetResponseStream())
                {
                    fileBytes = ReadFully(fStream);
                }
            }

            return fileBytes;
        }
    }
}