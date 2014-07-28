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
        public ActionResult GetPayItemStructures(int panel)
        {
            var items = _payItemStructureRepository.GetGroup(panel)
                .Select(i => new
                {
                    id = i.Id,
                    structureId = i.StructureId,
                    title = i.Title,
                    showSummary = true,
                    showItems = true
                })
                .ToList();
            return new DqeResult(items, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPayItemStructure(int id)
        {
            var item = _payItemStructureRepository.Get(id);
            if (item == null)
            {
                return new DqeResult(null, new ClientMessage{ Severity = ClientMessageSeverity.Error, text = "Pay Item Structure not found"}, JsonRequestBehavior.AllowGet);
            }
            var pis = new
            {
                id = item.Id,
                structureId = item.StructureId,
                title = item.Title,
                effectiveDate = item.EffectiveDate.ToShortDateString(),
                obsoleteDate = item.ObsoleteDate.HasValue ? item.ObsoleteDate.Value.ToShortDateString() : string.Empty,
                accuracy = 0,
                isPlanQuantity = item.IsPlanQuantity.ToString(),
                isDoNotBid = item.IsDoNotBid.ToString(),
                isFixedPrice = item.IsFixedPrice.ToString(),
                fixedAmount = item.FixedAmount.HasValue ? item.FixedAmount.Value : 0,
                notes = item.Notes,
                details = item.Details,
                essHistory = item.EssHistory,
                boeRecentChangeDate =
                    item.BoeRecentChangeDate.HasValue
                        ? item.BoeRecentChangeDate.Value.ToShortDateString()
                        : string.Empty,
                boeRecentChangeDescription = item.BoeRecentChangeDescription,
                structureDescription = item.StructureDescription,
                primaryUnit = item.PrimaryUnit,
                secondaryUnit = item.SecondaryUnit.HasValue ? item.SecondaryUnit : 0,
                otherReferences = item.OtherReferences.Select(x => new
                {
                    id = x.Id,
                    name = x.Name,
                    webLink = x.WebLink
                }),
                ppmChapters = item.PpmChapters.Select(x => new
                {
                    id = x.Id,
                    name = x.Name,
                    webLink = x.WebLink
                }),
                prepAndDocChapters = item.PrepAndDocChapters.Select(x => new
                {
                    id = x.Id,
                    name = x.Name,
                    webLink = x.WebLink
                }),
                specifications = item.Specifications.Select(x => new
                {
                    id = x.Id,
                    name = x.Name,
                    webLink = x.WebLink
                }),
                standards = item.Standards.Select(x => new
                {
                    id = x.Id,
                    name = x.Name,
                    webLink = x.WebLink
                }),
                showSummary = false,
                showItems = true,
                items = item.PayItems
                    .Select(i => new
                    {
                        id = i.Id,
                        payItemId = i.PayItemId,
                        shortDescription = i.ShortDescription,
                        unitOfMeasure = Enum.GetName(typeof (PrimaryUnitType), i.PrimaryUnit),
                        effectiveDate =
                            i.EffectiveDate.HasValue ? i.EffectiveDate.Value.ToShortDateString() : string.Empty,
                        obsoleteDate = i.ObsoleteDate.HasValue ? i.ObsoleteDate.Value.ToShortDateString() : string.Empty,
                        showSummary = true
                    })
                    .ToList()
            };
            return new DqeResult(pis, JsonRequestBehavior.AllowGet);
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