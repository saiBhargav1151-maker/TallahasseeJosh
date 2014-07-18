using System;
using System.Linq;
using System.Web.Mvc;
using Dqe.ApplicationServices;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;
using Dqe.Domain.Services;
using Dqe.Web.ActionResults;
using Dqe.Web.Attributes;
using Dqe.Web.Services;

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
    public class PayItemStructureAdministrationController : Controller
    {
        private readonly IStaffService _staffService;
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly ICommandRepository _commandRepository;
        private readonly IPayItemStructureRepository _payItemStructureRepository;
        private readonly IDqeWebLinkRepository _dqeWebLinkRepository;
        private readonly ITransactionManager _transactionManager;

        public PayItemStructureAdministrationController
            (
            IStaffService staffService,
            IDqeUserRepository dqeUserRepository,
            ICommandRepository commandRepository,
            IPayItemStructureRepository payItemStructureRepository,
            IDqeWebLinkRepository dqeWebLinkRepository,
            ITransactionManager transactionManager
            )
        {
            _staffService = staffService;
            _dqeUserRepository = dqeUserRepository;
            _commandRepository = commandRepository;
            _payItemStructureRepository = payItemStructureRepository;
            _dqeWebLinkRepository = dqeWebLinkRepository;
            _transactionManager = transactionManager;
        }

        [HttpGet]
        public ActionResult GetPayItemStructures()
        {
            var items = _payItemStructureRepository
                .GetAll()
                .Select(i => new
                {
                    id = i.Id,
                    structureId = i.StructureId,
                    title = i.Title,
                    effectiveDate = i.EffectiveDate.ToShortDateString(),
                    obsoleteDate = i.ObsoleteDate.HasValue ? i.ObsoleteDate.Value.ToShortDateString() : string.Empty,
                    accuracy = 0,
                    isPlanQuantity = i.IsPlanQuantity.ToString(),
                    isDoNotBid = i.IsDoNotBid.ToString(),
                    isFixedPrice = i.IsFixedPrice.ToString(),
                    fixedAmount = i.FixedAmount.HasValue ? i.FixedAmount.Value : 0,
                    notes = i.Notes,
                    details = i.Details,
                    essHistory = i.EssHistory,
                    boeRecentChangeDate = i.BoeRecentChangeDate.HasValue ? i.BoeRecentChangeDate.Value.ToShortDateString() : string.Empty,
                    boeRecentChangeDescription = i.BoeRecentChangeDescription,
                    structureDescription = i.StructureDescription,
                    primaryUnit = i.PrimaryUnit,
                    secondaryUnit = i.SecondaryUnit.HasValue ? i.SecondaryUnit : 0, 
                    otherReferences = i.OtherReferences.Select(x => new
                    {
                        id = x.Id,
                        name = x.Name,
                        webLink = x.WebLink
                    }),
                    ppmChapters = i.PpmChapters.Select(x => new
                    {
                        id = x.Id,
                        name = x.Name,
                        webLink = x.WebLink
                    }),
                    prepAndDocChapters = i.PrepAndDocChapters.Select(x => new
                    {
                        id = x.Id,
                        name = x.Name,
                        webLink = x.WebLink
                    }),
                    specifications = i.Specifications.Select(x => new
                    {
                        id = x.Id,
                        name = x.Name,
                        webLink = x.WebLink
                    }),
                    standards = i.Standards.Select(x => new
                    {
                        id = x.Id,
                        name = x.Name,
                        webLink = x.WebLink
                    }),
                    showSummary = true
                })
                .ToList();
            var items00 = items.Where(i => i.structureId.StartsWith("00")).OrderBy(i => i.structureId).ToList();
            var items01 = items.Where(i => i.structureId.StartsWith("01")).OrderBy(i => i.structureId).ToList();
            var items02 = items.Where(i => i.structureId.StartsWith("02")).OrderBy(i => i.structureId).ToList();
            var items03 = items.Where(i => i.structureId.StartsWith("03")).OrderBy(i => i.structureId).ToList();
            var items04 = items.Where(i => i.structureId.StartsWith("04")).OrderBy(i => i.structureId).ToList();
            var items05 = items.Where(i => i.structureId.StartsWith("05")).OrderBy(i => i.structureId).ToList();
            var items06 = items.Where(i => i.structureId.StartsWith("06")).OrderBy(i => i.structureId).ToList();
            var items07 = items.Where(i => i.structureId.StartsWith("07")).OrderBy(i => i.structureId).ToList();
            var items08 = items.Where(i => i.structureId.StartsWith("08")).OrderBy(i => i.structureId).ToList();
            var items09 = items.Where(i => i.structureId.StartsWith("09")).OrderBy(i => i.structureId).ToList();
            var items10 = items.Where(i => i.structureId.StartsWith("10")).OrderBy(i => i.structureId).ToList();
            return
                new DqeResult(
                    new
                    {
                        items00,
                        items01,
                        items02,
                        items03,
                        items04,
                        items05,
                        items06,
                        items07,
                        items08,
                        items09,
                        items10
                    }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdatePayItemStructure(dynamic payItemStructure)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var pis = (PayItemStructure) (payItemStructure.id == 0 ? new PayItemStructure(_payItemStructureRepository) : _payItemStructureRepository.Get(payItemStructure.id));
            var pist = pis.GetTransformer();
            //dates
            if (!string.IsNullOrWhiteSpace(payItemStructure.boeRecentChangeDate.ToString()))
            {
                DateTime parsedBoeRecentChangeDate;
                if (DateTime.TryParse(payItemStructure.boeRecentChangeDate.ToString(), out parsedBoeRecentChangeDate))
                {
                    pist.BoeRecentChangeDate = parsedBoeRecentChangeDate;
                }
                else
                {
                    return  new DqeResult(null, new ClientMessage{ Severity = ClientMessageSeverity.Error, text = "BOE Recent Change Date is invalid."});
                }
            }
            if (!string.IsNullOrWhiteSpace(payItemStructure.effectiveDate.ToString()))
            {
                DateTime parsedEffectiveDate;
                if (DateTime.TryParse(payItemStructure.effectiveDate.ToString(), out parsedEffectiveDate))
                {
                    pist.EffectiveDate = parsedEffectiveDate;
                }
                else
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Effective Date is invalid." });
                }
            }
            if (!string.IsNullOrWhiteSpace(payItemStructure.obsoleteDate.ToString()))
            {
                DateTime parsedObsoleteDate;
                if (DateTime.TryParse(payItemStructure.obsoleteDate.ToString(), out parsedObsoleteDate))
                {
                    pist.ObsoleteDate = parsedObsoleteDate;
                }
                else
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Obsolete Date is invalid." });
                }
            }
            //decimals
            decimal parsedFixedAmount;
            if (decimal.TryParse(payItemStructure.fixedAmount.ToString(), out parsedFixedAmount))
            {
                pist.FixedAmount = parsedFixedAmount;
            }
            //bools
            bool parsedBool;
            pist.IsDoNotBid = bool.TryParse(payItemStructure.isDoNotBid, out parsedBool) && parsedBool;
            pist.IsFixedPrice = bool.TryParse(payItemStructure.isFixedPrice, out parsedBool) && parsedBool;
            pist.IsPlanQuantity = bool.TryParse(payItemStructure.isPlanQuantity, out parsedBool) && parsedBool;
            //strings
            pist.Accuracy = "---";
            pist.BoeRecentChangeDescription = payItemStructure.boeRecentChangeDescription;
            pist.Details = payItemStructure.details;
            pist.EssHistory = payItemStructure.essHistory;
            pist.Notes = payItemStructure.notes;
            pist.StructureDescription = payItemStructure.structureDescription;
            pist.StructureId = payItemStructure.structureId;
            pist.Title = payItemStructure.title;
            pist.PrimaryUnit = (PrimaryUnitType)payItemStructure.primaryUnit;
            pist.SecondaryUnit = (SecondaryUnitType?)payItemStructure.secondaryUnit;
            pis.Transform(pist, currentDqeUser);
            //TODO: implement these
            //pist.IsMonitored
            //pist.MonitorSrsId
            //pist.PlanSummary
            var r = EntityValidator.Validate(_transactionManager, pis);
            if (r != null) return r;
            if (payItemStructure.id == 0) _commandRepository.Add(pis);
            pis.ClearOtherReferences(currentDqeUser);
            pis.ClearPpmChapters(currentDqeUser);
            pis.ClearPrepAndDocChapters(currentDqeUser);
            pis.ClearSpecifications(currentDqeUser);
            pis.ClearStandards(currentDqeUser);
            foreach (var link in payItemStructure.otherReferences)
            {
                var wl = _dqeWebLinkRepository.Get((int)link.id);
                if (wl != null) pis.AddOtherReference((OtherReferenceWebLink)wl, currentDqeUser);
            }
            foreach (var link in payItemStructure.ppmChapters)
            {
                var wl = _dqeWebLinkRepository.Get((int)link.id);
                if (wl != null) pis.AddPpmChapter((PpmChapterWebLink)wl, currentDqeUser);
            }
            foreach (var link in payItemStructure.prepAndDocChapters)
            {
                var wl = _dqeWebLinkRepository.Get((int)link.id);
                if (wl != null) pis.AddPrepAndDocChapter((PrepAndDocChapterWebLink)wl, currentDqeUser);
            }
            foreach (var link in payItemStructure.specifications)
            {
                var wl = _dqeWebLinkRepository.Get((int)link.id);
                if (wl != null) pis.AddSpecification((SpecificationWebLink)wl, currentDqeUser);
            }
            foreach (var link in payItemStructure.standards)
            {
                var wl = _dqeWebLinkRepository.Get((int)link.id);
                if (wl != null) pis.AddStandard((StandardWebLink)wl, currentDqeUser);
            }
            return new DqeResult(null, new ClientMessage { text = "Pay Item Structure added" });
        }
    }
}