using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Dqe.ApplicationServices;

namespace Dqe.Web.Controllers
{
    public class ReportController : Controller
    {
        private readonly IDocumentConverterService _documentConverterService;

        public ReportController(IDocumentConverterService documentConverterService)
        {
            _documentConverterService = documentConverterService;
        }

        // GET: Report
        public ActionResult Index()
        {
            var url = "http://" +Request.Url.Authority + Url.Content("~/Views/reports/TestReport.html");
            var pdf=_documentConverterService.ConvertUrlToPdf(url);
            return File(pdf, "application/pdf", "Report.pdf");
        }
    }
}