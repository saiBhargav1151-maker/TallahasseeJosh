using System.Linq;
using System.Web.Mvc;
using Dqe.ApplicationServices;
using Dqe.Domain.Repositories.Custom;
using Dqe.Web.Areas.Reports.Models.Report;

namespace Dqe.Web.Areas.Reports.Controllers
{
    public class ReportController : Controller
    {
        private readonly IDocumentConverterService _documentConverterService;
        private readonly IPayItemStructureRepository _payItemStructureRepository;

        public ReportController(IDocumentConverterService documentConverterService, IPayItemStructureRepository payItemStructureRepository)
        {
            _documentConverterService = documentConverterService;
            _payItemStructureRepository = payItemStructureRepository;
        }

        // GET: Report
        public ActionResult Index()
        {
            var url = "http://" +Request.Url.Authority + Url.Content("~/Views/reports/TestReport.html");
            var pdf=_documentConverterService.ConvertUrlToPdf(url);
            return File(pdf, "application/pdf", "Report.pdf");
        }

        public ActionResult GetBoeReport()
        {
            var structures = _payItemStructureRepository.GetAll(false, 11);
            var vm = new BoeViewModel
            {
                PayItemStructures = structures.Select(p => new PayItemStructureViewModel
                {
                    Accuracy = p.Accuracy.ToString(),
                    Description = p.StructureDescription,
                    PayItemStructureId = p.StructureId,
                    Notes = p.Notes,
                    PayItems = p.PayItems.Select(pi=>new PayItemViewModel
                    {
                        Description = pi.Description,
                        ObsoleteDate = pi.ObsoleteDate.GetValueOrDefault(),
                        PayItemId = pi.PayItemId,
                    })
                })
            };

            return View("BasisOfEstimatesTable", vm);
        }

        public ActionResult BoePdf()
        {
            var url = "http://" + Request.Url.Authority + this.Url.Action("GetBoeReport");
            var pdf = _documentConverterService.ConvertUrlToPdf(url);
            return File(pdf, "application/pdf", "Report.pdf");
        }
    }
}