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
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly ICommandRepository _commandRepository;
        private readonly IPayItemStructureRepository _payItemStructureRepository;
        private readonly IDqeWebLinkRepository _dqeWebLinkRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IStaffService _staffService;
        private readonly ICostBasedTemplateRepository _costBasedTemplateRepository;

        public PayItemStructureAdministrationController
            (
            IDqeUserRepository dqeUserRepository,
            ICommandRepository commandRepository,
            IPayItemStructureRepository payItemStructureRepository,
            IDqeWebLinkRepository dqeWebLinkRepository,
            ITransactionManager transactionManager,
            IStaffService staffService,
            ICostBasedTemplateRepository costBasedTemplateRepository
            )
        {
            _dqeUserRepository = dqeUserRepository;
            _commandRepository = commandRepository;
            _payItemStructureRepository = payItemStructureRepository;
            _dqeWebLinkRepository = dqeWebLinkRepository;
            _transactionManager = transactionManager;
            _staffService = staffService;
            _costBasedTemplateRepository = costBasedTemplateRepository;
        }

        [HttpGet]
        public ActionResult GetPayItemStructures(bool viewAll)
        {
            var items = _payItemStructureRepository.GetAll(viewAll)
                .Select(i => new
                {
                    id = i.Id,
                    structureId = i.StructureId,
                    title = i.Title,
                    effectiveDate = i.EffectiveDate.ToShortDateString(),
                    obsoleteDate = i.ObsoleteDate.HasValue ? i.ObsoleteDate.Value.ToShortDateString() : string.Empty,
                    isMixedUnit = i.PrimaryUnit == PrimaryUnitType.Mixed,
                    primaryUnit = (int)i.PrimaryUnit,
                    secondarUnit = i.SecondaryUnit.HasValue ? (int)i.SecondaryUnit.Value : 0,
                    primaryUnitCode = Enum.GetName(typeof(PrimaryUnitType), i.PrimaryUnit),
                    secondaryUnitCode = i.SecondaryUnit.HasValue ? Enum.GetName(typeof(SecondaryUnitType), i.SecondaryUnit) : string.Empty,
                    showItems = false,
                    srsId = i.SrsId,
                    costBasedTemplateId = i.MyCostBasedTemplate == null ? 0 : i.MyCostBasedTemplate.Id,
                    items = i.PayItems.Select(x => new
                    {
                        id = x.Id,
                        number = x.PayItemId,
                        shortDescription = x.ShortDescription,
                        masterFile = x.MyMasterFile.FileNumber,
                        unitOfMeasure = x.PrimaryUnit == PrimaryUnitType.LS && x.SecondaryUnit.HasValue
                            ? string.Format("{0}/{1}", Enum.GetName(typeof (PrimaryUnitType), x.PrimaryUnit), Enum.GetName(typeof (SecondaryUnitType), x.SecondaryUnit.Value))
                            : Enum.GetName(typeof (PrimaryUnitType), x.PrimaryUnit),
                        effectiveDate = x.EffectiveDate.HasValue ? x.EffectiveDate.Value.ToShortDateString() : string.Empty,
                        obsoleteDate = x.ObsoleteDate.HasValue ? x.ObsoleteDate.Value.ToShortDateString() : string.Empty
                    })
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
            var monitor = item.SrsId > 0 ? _staffService.GetStaffById(item.SrsId) : null;
            var pis = new
            {
                id = item.Id,
                structureId = item.StructureId,
                title = item.Title,
                effectiveDate = item.EffectiveDate.ToShortDateString(),
                obsoleteDate = item.ObsoleteDate.HasValue ? item.ObsoleteDate.Value.ToShortDateString() : string.Empty,
                accuracy = item.Accuracy,
                isPlanQuantity = item.IsPlanQuantity.ToString(),
                isDoNotBid = item.IsDoNotBid.ToString(),
                isFixedPrice = item.IsFixedPrice.ToString(),
                fixedAmount = item.FixedAmount.HasValue ? item.FixedAmount.Value : 0,
                notes = item.Notes,
                details = item.Details,
                pendingInformation = item.PendingInformation,
                essHistory = item.EssHistory,
                boeRecentChangeDate =
                    item.BoeRecentChangeDate.HasValue
                        ? item.BoeRecentChangeDate.Value.ToShortDateString()
                        : string.Empty,
                boeRecentChangeDescription = item.BoeRecentChangeDescription,
                structureDescription = item.StructureDescription,
                primaryUnit = item.PrimaryUnit,
                secondaryUnit = item.SecondaryUnit.HasValue ? item.SecondaryUnit : 0,
                primaryUnitCode = Enum.GetName(typeof(PrimaryUnitType), item.PrimaryUnit),
                secondaryUnitCode = item.SecondaryUnit.HasValue ? Enum.GetName(typeof(SecondaryUnitType), item.SecondaryUnit) : string.Empty,
                srsId = item.SrsId,
                monitor = monitor == null 
                    ? null 
                    : new
                    {
                        id = monitor.Id,
                        fullName = monitor.FullName,
                        district = monitor.District,
                        email = monitor.Email,
                        phoneNumber = monitor.PhoneAndExtension
                    },
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
                costBasedTemplateId = item.MyCostBasedTemplate == null ? 0 : item.MyCostBasedTemplate.Id
            };
            return new DqeResult(pis, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdatePayItemStructure(dynamic payItemStructure)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var message = payItemStructure.id == 0 ? "Pay Item Structure added" : "Pay Item Structure updated";
            var pis = (PayItemStructure)(payItemStructure.id == 0 ? new PayItemStructure(_payItemStructureRepository) : _payItemStructureRepository.Get(payItemStructure.id));
            var pist = pis.GetTransformer();
            //dates
            pist.BoeRecentChangeDate = null;
            pist.ObsoleteDate = null;
            if (!string.IsNullOrWhiteSpace(payItemStructure.boeRecentChangeDate.ToString()))
            {
                DateTime parsedBoeRecentChangeDate;
                if (DateTime.TryParse(payItemStructure.boeRecentChangeDate.ToString(), out parsedBoeRecentChangeDate))
                {
                    pist.BoeRecentChangeDate = parsedBoeRecentChangeDate;
                }
                else
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "BOE Recent Change Date is invalid." });
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
                    return new DqeResult(null, new ClientMessage {Severity = ClientMessageSeverity.Error, text = "Effective Date is invalid."});
                }
            }
            else
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Effective Date is required." });
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
            pist.BoeRecentChangeDescription = payItemStructure.boeRecentChangeDescription.ToString();
            pist.Details = payItemStructure.details.ToString();
            pist.PendingInformation = payItemStructure.pendingInformation.ToString();
            pist.EssHistory = payItemStructure.essHistory.ToString();
            pist.Notes = payItemStructure.notes.ToString();
            pist.StructureDescription = payItemStructure.structureDescription.ToString();
            pist.StructureId = payItemStructure.structureId.ToString();
            pist.Title = payItemStructure.title.ToString();
            //enums
            pist.PrimaryUnit = (PrimaryUnitType)payItemStructure.primaryUnit;
            pist.SecondaryUnit = (SecondaryUnitType?)payItemStructure.secondaryUnit;
            //ints
            pist.SrsId = payItemStructure.srsId;
            int accuracy;
            if (int.TryParse(payItemStructure.accuracy.ToString(), out accuracy))
            {
                pist.Accuracy = accuracy;    
            }
            pis.Transform(pist, currentDqeUser);
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
            if (payItemStructure.costBasedTemplateId == 0)
            {
                pis.RemoveCostBasedTemplate(currentDqeUser);
            }
            else
            {
                var cbt = _costBasedTemplateRepository.Get(payItemStructure.costBasedTemplateId);
                if (cbt != null)
                {
                    pis.AddCostBasedTemplate(cbt, currentDqeUser);
                }
            }
            var o = new
            {
                id = pis.Id,
                structureId = pis.StructureId,
                title = pis.Title,
                effectiveDate = pis.EffectiveDate.ToShortDateString(),
                obsoleteDate = pis.ObsoleteDate.HasValue ? pis.ObsoleteDate.Value.ToShortDateString() : string.Empty,
                isMixedUnit = pis.PrimaryUnit == PrimaryUnitType.Mixed,
                primaryUnit = (int)pis.PrimaryUnit,
                secondarUnit = pis.SecondaryUnit.HasValue ? (int)pis.SecondaryUnit.Value : 0,
                primaryUnitCode = Enum.GetName(typeof(PrimaryUnitType), pis.PrimaryUnit),
                secondaryUnitCode = pis.SecondaryUnit.HasValue ? Enum.GetName(typeof(SecondaryUnitType), pis.SecondaryUnit) : string.Empty,
                showItems = false,
                srsId = pis.SrsId,
                costBasedTemplateId = pis.MyCostBasedTemplate == null ? 0 : pis.MyCostBasedTemplate.Id,
                items = pis.PayItems.Select(i => new
                {
                    id = i.Id,
                    number = i.PayItemId,
                    masterFile = i.MyMasterFile.FileNumber,
                    shortDescription = i.ShortDescription,
                    unitOfMeasure = i.PrimaryUnit == PrimaryUnitType.LS && i.SecondaryUnit.HasValue
                        ? string.Format("{0}/{1}", Enum.GetName(typeof(PrimaryUnitType), i.PrimaryUnit), Enum.GetName(typeof(SecondaryUnitType), i.SecondaryUnit.Value))
                        : Enum.GetName(typeof(PrimaryUnitType), i.PrimaryUnit),
                    effectiveDate = i.EffectiveDate.HasValue ? i.EffectiveDate.Value.ToShortDateString() : string.Empty,
                    obsoleteDate = i.ObsoleteDate.HasValue ? i.ObsoleteDate.Value.ToShortDateString() : string.Empty
                })
            };
            return new DqeResult(o, new ClientMessage { text = message });
        }
    }
}