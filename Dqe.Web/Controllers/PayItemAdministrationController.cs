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
    public class PayItemAdministrationController : Controller
    {
        private readonly IStaffService _staffService;
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly ICommandRepository _commandRepository;
        private readonly IPayItemRepository _payItemRepository;
        private readonly IPayItemStructureRepository _payItemStructureRepository;
        private readonly IMasterFileRepository _masterFileRepository;
        private readonly IDqeWebLinkRepository _dqeWebLinkRepository;
        private readonly ITransactionManager _transactionManager;

        public PayItemAdministrationController
            (
            IStaffService staffService,
            IDqeUserRepository dqeUserRepository,
            ICommandRepository commandRepository,
            IPayItemRepository payItemRepository,
            IPayItemStructureRepository payItemStructureRepository,
            IDqeWebLinkRepository dqeWebLinkRepository,
            IMasterFileRepository masterFileRepository,
            ITransactionManager transactionManager
            )
        {
            _staffService = staffService;
            _dqeUserRepository = dqeUserRepository;
            _commandRepository = commandRepository;
            _payItemRepository = payItemRepository;
            _payItemStructureRepository = payItemStructureRepository;
            _dqeWebLinkRepository = dqeWebLinkRepository;
            _masterFileRepository = masterFileRepository;
            _transactionManager = transactionManager;
        }

        [HttpGet]
        public ActionResult GetPayItems(int payItemStructureId)
        {
            var items = _payItemRepository.GetByStructure(payItemStructureId)
                .Select(i => new
                {
                    id = i.Id,
                    payItemId = i.PayItemId,
                    shortDescription = i.ShortDescription,
                    unitOfMeasure = Enum.GetName(typeof(PrimaryUnitType), i.PrimaryUnit),
                    effectiveDate = i.EffectiveDate.HasValue ? i.EffectiveDate.Value.ToShortDateString() : string.Empty,
                    obsoleteDate = i.ObsoleteDate.HasValue ? i.ObsoleteDate.Value.ToShortDateString() : string.Empty,
                    showSummary = true
                })
                .ToList();
            return new DqeResult(items, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPayItem(int id)
        {
            var item = _payItemRepository.Get(id);
            if (item == null)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Pay Item not found" }, JsonRequestBehavior.AllowGet);
            }
            var pis = new
            {
                id = item.Id,
                payItemId = item.PayItemId,
                shortDescription = item.ShortDescription,
                description = item.Description,
                effectiveDate =
                    item.EffectiveDate.HasValue ? item.EffectiveDate.Value.ToShortDateString() : string.Empty,
                obsoleteDate = item.ObsoleteDate.HasValue ? item.ObsoleteDate.Value.ToShortDateString() : string.Empty,
                isFederalFunded = item.IsFederalFunded.ToString(),
                isLikeItemsCombined = item.IsLikeItemsCombined.ToString(),
                isSupplementalDescriptionRequired = item.IsSupplementalDescriptionRequired.ToString(),
                lreReferencePrice = item.LreReferencePrice,
                dqeReferencePrice = item.DqeReferencePrice,
                concreteFactor = item.ConcreteFactor,
                asphaltFactor = item.AsphaltFactor,
                fuelFactor = item.FuelFactor,
                factorNotes = item.FactorNotes,
                primaryUnit = item.PrimaryUnit,
                secondaryUnit = item.SecondaryUnit.HasValue ? item.SecondaryUnit : 0,
                showSummary = false,
                masterFileId = item.MyMasterFile.FileNumber
            };
            return new DqeResult(pis, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdatePayItem(dynamic payItem)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var pi = (PayItem)(payItem.id == 0 ? new PayItem() : _payItemRepository.Get(payItem.id));
            var pit = pi.GetTransformer();
            //dates
            if (!string.IsNullOrWhiteSpace(payItem.effectiveDate.ToString()))
            {
                DateTime parsedEffectiveDate;
                if (DateTime.TryParse(payItem.effectiveDate.ToString(), out parsedEffectiveDate))
                {
                    pit.EffectiveDate = parsedEffectiveDate;
                }
                else
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Effective Date is invalid." });
                }
            }
            if (!string.IsNullOrWhiteSpace(payItem.obsoleteDate.ToString()))
            {
                DateTime parsedObsoleteDate;
                if (DateTime.TryParse(payItem.obsoleteDate.ToString(), out parsedObsoleteDate))
                {
                    pit.ObsoleteDate = parsedObsoleteDate;
                }
                else
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Obsolete Date is invalid." });
                }
            }
            //decimals
            decimal parsedLreReferencePrice;
            if (decimal.TryParse(payItem.lreReferencePrice.ToString(), out parsedLreReferencePrice))
            {
                pit.LreReferencePrice = parsedLreReferencePrice;
            }
            decimal parsedDqeReferencePrice;
            if (decimal.TryParse(payItem.dqeReferencePrice.ToString(), out parsedDqeReferencePrice))
            {
                pit.DqeReferencePrice = parsedDqeReferencePrice;
            }
            decimal parsedConcreteFactor;
            if (decimal.TryParse(payItem.concreteFactor.ToString(), out parsedConcreteFactor))
            {
                pit.ConcreteFactor = parsedConcreteFactor;
            }
            decimal parsedAsphaltFactor;
            if (decimal.TryParse(payItem.asphaltFactor.ToString(), out parsedAsphaltFactor))
            {
                pit.AsphaltFactor = parsedAsphaltFactor;
            }
            decimal parsedFuelFactor;
            if (decimal.TryParse(payItem.fuelFactor.ToString(), out parsedFuelFactor))
            {
                pit.FuelFactor = parsedFuelFactor;
            }
            //bools
            bool parsedBool;
            pit.IsFederalFunded = bool.TryParse(payItem.isFederalFunded, out parsedBool) && parsedBool;
            pit.IsLikeItemsCombined = bool.TryParse(payItem.isLikeItemsCombined, out parsedBool) && parsedBool;
            pit.IsSupplementalDescriptionRequired = bool.TryParse(payItem.isSupplementalDescriptionRequired, out parsedBool) && parsedBool;
            //strings
            pit.PayItemId = payItem.payItemId;
            pit.Description = payItem.description;
            pit.ShortDescription = payItem.shortDescription;
            pit.FactorNotes = payItem.factorNotes;
            pit.PrimaryUnit = (PrimaryUnitType)payItem.primaryUnit;
            pit.SecondaryUnit = (SecondaryUnitType?)payItem.secondaryUnit;
            if (payItem.id == 0)
            {
                var pis = _payItemStructureRepository.Get(payItem.structureId);
                var mf = _masterFileRepository.Get(payItem.masterFileId);
                pi.AssociatePayItemToStructureAndMasterFile(pis, mf);
            }
            pi.Transform(pit, currentDqeUser);
            //TODO: implement remaining properties
            var r = EntityValidator.Validate(_transactionManager, pi);
            if (r != null) return r;
            if (payItem.id == 0)
            {
                _commandRepository.Add(pi);
            }
            return new DqeResult(null, new ClientMessage { text = "Pay Item Structure added" });
        }
    }
}