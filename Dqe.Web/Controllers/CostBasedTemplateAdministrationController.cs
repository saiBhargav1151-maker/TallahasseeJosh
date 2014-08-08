using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Dqe.ApplicationServices;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;
using Dqe.Domain.Services;
using Dqe.Web.ActionResults;
using Dqe.Web.Attributes;

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    public class CostBasedTemplateAdministrationController : Controller
    {
        private readonly ICommandRepository _commandRepository;
        private readonly ICostBasedTemplateRepository _costBasedTemplateRepository;
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IDocumentService _documentService;

        public CostBasedTemplateAdministrationController(ICommandRepository commandRepository,
            ICostBasedTemplateRepository costBasedTemplateRepository,
            IDqeUserRepository dqeUserRepository, ITransactionManager transactionManager,
            IDocumentService documentService)
        {
            _commandRepository = commandRepository;
            _costBasedTemplateRepository = costBasedTemplateRepository;
            _dqeUserRepository = dqeUserRepository;
            _transactionManager = transactionManager;
            _documentService = documentService;
        }

        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.CostBasedTemplateAdministrator, DqeRole.PayItemAdministrator})]
        [HttpGet]
        public ActionResult GetAll()
        {
            var result = new DqeResult(_costBasedTemplateRepository.GetAll()
            .Select(i =>
                new
                {
                    id = i.Id,
                    name = i.Name,
                    resultCell = i.ResultCell,
                    documentId = i.CurrentDocumentVersion.Id
                }), JsonRequestBehavior.AllowGet);

            return result;
        }

        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.CostBasedTemplateAdministrator })]
        [HttpPost]
        public ActionResult SaveCostBasedTemplate()
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            Document templateDocument = null;

            var name = Request.Form["name"];
            var resultCell = Request.Form["resultCell"];
            var id = Convert.ToInt32(Request.Form["id"]);

            var template = id > 0 ? _costBasedTemplateRepository.Get(id) : new CostBasedTemplate();

            if (Request.Files != null && Request.Files.Count > 0)
            {
                if (!Request.Files[0].FileName.ToUpper().EndsWith(".XLSX"))
                {
                    return new DqeResult(new object(),
                        new ClientMessage
                        {
                            Severity = ClientMessageSeverity.Error,
                            text = "The uploaded file must be a .xlsx excel file"
                        });
                }

                templateDocument = _documentService.AddDocument(Request.Files[0].FileName, Request.Files[0].InputStream);
            }

            var transformer = template.GetTransformer();

            transformer.Name = name;
            transformer.ResultCell = resultCell;

            var validationResults = new List<ValidationResult>();

            if (templateDocument != null)
            {
                var documentVersion = new CostBasedTemplateDocumentVersion();
                var docTransformer=documentVersion.GetTransformer();

                docTransformer.DocumentId = templateDocument.Id;
                docTransformer.Timestamp = DateTime.Now;
                documentVersion.Transform(docTransformer,currentDqeUser);
                template.AddDocumentVersion(documentVersion,currentDqeUser);
            }

            template.Transform(transformer, currentDqeUser);

            if (templateDocument != null)
            {
                validationResults.AddRange(template.ValidateExcelFile(templateDocument));
            }

            validationResults.AddRange(template.Validate(null));

            if (validationResults.Any())
            {
                _transactionManager.Abort();
                return new DqeResult(null, validationResults.Select(v => new ClientMessage { Severity = ClientMessageSeverity.Error, text = v.ErrorMessage, ttl = 0 }));
            }

            string successMsg;
            if (id == 0)
            {
                _commandRepository.Add(template);
                successMsg = "Your Cost-Based Template has been successfuly added.";
            }
            else
            {
                successMsg = "Your Cost-Based Template has been successfuly updated.";
            }

            return new DqeResult(new { name = template.Name, resultCell = template.ResultCell, id = template.Id, documentId = template.CurrentDocumentVersion.Id },
                new ClientMessage { Severity = ClientMessageSeverity.Success, text = successMsg, ttl = 2000 });
        }

        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.CostBasedTemplateAdministrator })]
        [HttpGet]
        public ActionResult DownloadTemplate(int id)
        {
            var document = _documentService.GetDocument(id);
            return File(document.FileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", document.Name);
        }

        //[HttpPost]
        //public ActionResult RemoveCostBasedTemplates(dynamic templates)
        //{
        //    var selectedTemplates = ((IEnumerable<dynamic>)templates).ToList();

        //    foreach (var selectedTemplate in selectedTemplates)
        //    {
        //        var template = _costBasedTemplateRepository.Get(selectedTemplate.id);
        //        if (template != null) _commandRepository.Remove(template);
        //    }

        //    _transactionManager.Commit();

        //    foreach (var selectedTemplate in selectedTemplates)
        //    {
        //        var template = _costBasedTemplateRepository.Get(selectedTemplate.id);
        //        if (template != null) _commandRepository.Remove(template);
        //        _transactionManager.Commit();
        //        _documentService.DeleteDocument(template.CurrentDocumentVersion.Id);
        //    }
        //    return new DqeResult(null, new ClientMessage { text = "Cost-Based Templates Removed" });
        //}
    }
}