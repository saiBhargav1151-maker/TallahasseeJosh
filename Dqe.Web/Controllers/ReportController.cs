using System;
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
using Dqe.Web.Attributes;

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator })]
    public class ReportController : Controller
    {
        private readonly IWebTransportService _webTransportService;
        private readonly IReportRepository _reportRepository;
        public ReportController(IWebTransportService webTransportService, IReportRepository reportRepository)
        {
            _webTransportService = webTransportService;
            _reportRepository = reportRepository;
        }

        [HttpPost]
        public ActionResult ViewProjectItemsReport(FormCollection form)
        {
            var versions = Server.UrlDecode(form["hiddenProjectSnapshotIds"]).Split(',');

#if DEBUG
            var serviceUrl = ConfigurationManager.AppSettings.Get("reportServerUrl");
#else
            var serviceUrl = ConfigurationManager.AppSettings.Get("reportServerUrlDot");
#endif
            var targetUrl = string.Empty;
            var fileName = string.Empty;

            if (versions.Length == 1)
            {
                targetUrl = string.Format(serviceUrl + "/SnapshotReport&rs:Command=Render&rs:Format=PDF&ProjectEstimateId={0}", versions[0]);
                fileName = string.Format("SnapshotReport{0}.pdf", versions[0]);
            }
            else
            {
                var convertedIntegers = versions.Select(item => Convert.ToInt32(item)).ToList();

                targetUrl = string.Format(serviceUrl + "/SnapshotComparison&rs:Command=Render&rs:Format=PDF&NewestProjectEstimateId={0}&OldestProjectEstimateId={1}", convertedIntegers.Max(), convertedIntegers.Min());
                fileName = string.Format("SnapshotComparisonReport{0}-{1}.pdf", versions[0], versions[1]);
            }

            var fileBytes = CallSsrsWebService(targetUrl);

            return File(fileBytes, "application/pdf", fileName);
        }

        [HttpPost]
        public ActionResult ViewProposalSummaryReport(FormCollection form)
        {
            var currentUser = (DqeIdentity)User.Identity;

            var proposalNumber = form["proposalNumber"];
            var estimateType = form["estimateType"];
#if DEBUG
            var serviceUrl = ConfigurationManager.AppSettings.Get("reportServerUrl");
#else
            var serviceUrl = ConfigurationManager.AppSettings.Get("reportServerUrlDot");
#endif
            var targetUrl = string.Format(serviceUrl + "/DetailEstimate&rs:Command=Render&rs:Format=PDF&ProposalNumber={0}&ProposalLevel={1}&UserName={2}", proposalNumber, estimateType, currentUser.Name);

            var fileBytes = CallSsrsWebService(targetUrl);

            return File(fileBytes, "application/pdf", string.Format("ProposalSummary{0}.pdf", proposalNumber));
        }

        [HttpPost]
        public ActionResult ViewUnbalancedItemsReport(FormCollection form)
        {
            var proposalNumber = form["proposalNumber"];
            var showEstimate = form["showEstimate"];
#if DEBUG
            var serviceUrl = ConfigurationManager.AppSettings.Get("reportServerUrl");
#else
            var serviceUrl = ConfigurationManager.AppSettings.Get("reportServerUrlDot");
#endif
            var targetUrl = string.Format(serviceUrl + "/UnbalancedItems&rs:Command=Render&rs:Format=PDF&ProposalNumber={0}&ShowEstimate={1}", proposalNumber, showEstimate);

            var fileBytes = CallSsrsWebService(targetUrl);

            return File(fileBytes, "application/pdf", string.Format("UnbalancedItems{0}.pdf", proposalNumber));
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

            _reportRepository.SaveReport(letting);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ViewSummaryOfLettingReport(FormCollection form)
        {
            var lettingNumber = form["lettingNumber"];

#if DEBUG
            var serviceUrl = ConfigurationManager.AppSettings.Get("reportServerUrl");
#else
            var serviceUrl = ConfigurationManager.AppSettings.Get("reportServerUrlDot");
#endif
            var targetUrl = string.Format(serviceUrl + "/ExecutiveSummaryOfLetting&rs:Command=Render&rs:Format=PDF&LettingNumber={0}&ProposalLevel=2", lettingNumber);

            var req = (HttpWebRequest)WebRequest.Create(targetUrl);
            req.PreAuthenticate = true;
            //req.Credentials = new NetworkCredential(ConfigurationManager.AppSettings.Get("rptUser"), ConfigurationManager.AppSettings.Get("rptPassword"), ConfigurationManager.AppSettings.Get("rptDomain"));
            req.Credentials = CredentialCache.DefaultCredentials;

            var httpWResp = (HttpWebResponse)req.GetResponse();
            var fStream = httpWResp.GetResponseStream();
            var fileBytes = ReadFully(fStream);

            httpWResp.Close();
            System.Web.HttpContext.Current.Response.Clear();

            return File(fileBytes, "application/pdf", string.Format("ExecutiveSummaryOfLetting{0}.pdf", lettingNumber));
        }

        [HttpGet]
        public ActionResult GetLettings(string lettingNumber)
        {
            var lettings = _webTransportService.GetLettingNames(lettingNumber);
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

        private static byte[] ReadFully(Stream input)
        {
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private static byte[] CallSsrsWebService(string targetUrl)
        {
            var req = (HttpWebRequest)WebRequest.Create(targetUrl);
            req.PreAuthenticate = true;
#if DEBUG
            req.Credentials = CredentialCache.DefaultCredentials;
#else
            req.Credentials = new NetworkCredential(ConfigurationManager.AppSettings.Get("rptUser"), ConfigurationManager.AppSettings.Get("rptPassword"), ConfigurationManager.AppSettings.Get("rptDomain"));
#endif

            var httpWResp = (HttpWebResponse)req.GetResponse();
            var fStream = httpWResp.GetResponseStream();
            var fileBytes = ReadFully(fStream);

            httpWResp.Close();
            System.Web.HttpContext.Current.Response.Clear();
            return fileBytes;
        }
    }
}