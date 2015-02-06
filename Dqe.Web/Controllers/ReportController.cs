using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Dqe.Domain.Fdot;
using Dqe.Web.Attributes;

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    public class ReportController : Controller
    {
        private readonly IWebTransportService _webTransportService;
        public ReportController(IWebTransportService webTransportService)
        {
            _webTransportService = webTransportService;
        }

        [HttpPost]
        public ActionResult ViewProposalSummaryReport(FormCollection form)
        {
            var proposalNumber = form["proposalNumber"];
            var estimateType = form["estimateType"];

            var strReportUser = "knatckm";//"dqe_rpt";
            var strReportUserPw = "EcKaKa01";//"rptuser";
            var strReportUserDomain = "co";
            var serviceUrl = ConfigurationManager.AppSettings.Get("reportServerUrlDot");
            var targetUrl = string.Format(serviceUrl + "/DQE_Reports/ProposalSummaryLog&rs:Command=Render&rs:Format=PDF&Proposal={0}&EstimateType={1}", proposalNumber, estimateType);

            var req = (HttpWebRequest)WebRequest.Create(targetUrl);
            req.PreAuthenticate = true;
            req.Credentials = new NetworkCredential(strReportUser, strReportUserPw, strReportUserDomain);

            var httpWResp = (HttpWebResponse)req.GetResponse();
            var fStream = httpWResp.GetResponseStream();
            var fileBytes = ReadFully(fStream);

            httpWResp.Close();
            System.Web.HttpContext.Current.Response.Clear();

            return File(fileBytes, "application/pdf", "SSRSReport.pdf");
        }

        [HttpPost]
        public ActionResult ViewItemAverageReport(FormCollection form)
        {
            var p = form["testParm"];
            var marketAreas = new List<string>();

            if (form["marketAreas"] != null)
                marketAreas = form["marketAreas"].Split(',').ToList();

            var strReportUser = "kevin";
            var strReportUserPW = "MySecretPassword";
            var strReportUserDomain = "Kevin-Laptop";

            //string sTargetURL = string.Format("http://kevin-laptop/ReportServer_SQLEXPRESS?" + "/Dqe.Reporting/ItemAverageUnitCost&rs:Command=Render&rs:Format=PDF&PayItemNumber=1090136 11");
            var targetUrl = string.Format("http://kevin-laptop/ReportServer_SQLEXPRESS?" + "/Dqe.Reporting/ItemAverageUnitCostOne&rs:Command=Render&rs:Format=PDF");

            targetUrl = marketAreas.Aggregate(targetUrl, (current, ma) => current + "&MarketArea=" + ma);

            var req = (HttpWebRequest)WebRequest.Create(targetUrl);
            req.PreAuthenticate = true;
            req.Credentials = CredentialCache.DefaultCredentials;// new System.Net.NetworkCredential(strReportUser, strReportUserPW, strReportUserDomain);

            var httpWResp = (HttpWebResponse)req.GetResponse();
            var fStream = httpWResp.GetResponseStream();
            var fileBytes = ReadFully(fStream);

            httpWResp.Close();
            System.Web.HttpContext.Current.Response.Clear();

            return File(fileBytes, "application/pdf", "SSRSReport.pdf");
        }

        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        [HttpGet]
        public ActionResult GetLettings(string number)
        {
            //need to make a letting call
            var lettings = _webTransportService.GetLettingNames(number);
            return Json(lettings
                .Select(i => new
                {
                    id = i.Id,
                    number = i.LettingName,
                }),
                JsonRequestBehavior.AllowGet);
        }
    }
}