using System;
using System.Collections.Generic;
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
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly ICommandRepository _commandRepository;
        private readonly IPayItemRepository _payItemRepository;
        private readonly IPayItemStructureRepository _payItemStructureRepository;
        private readonly IMasterFileRepository _masterFileRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IStaffService _staffService;
        private readonly ICostBasedTemplateRepository _costBasedTemplateRepository;

        public PayItemAdministrationController
            (
            IDqeUserRepository dqeUserRepository,
            ICommandRepository commandRepository,
            IPayItemRepository payItemRepository,
            IPayItemStructureRepository payItemStructureRepository,
            IMasterFileRepository masterFileRepository,
            ITransactionManager transactionManager,
            IStaffService staffService,
            ICostBasedTemplateRepository costBasedTemplateRepository
            )
        {
            _dqeUserRepository = dqeUserRepository;
            _commandRepository = commandRepository;
            _payItemRepository = payItemRepository;
            _payItemStructureRepository = payItemStructureRepository;
            _masterFileRepository = masterFileRepository;
            _transactionManager = transactionManager;
            _staffService = staffService;
            _costBasedTemplateRepository = costBasedTemplateRepository;
        }

        [HttpGet]
        public ActionResult GetPayItem(int id)
        {
            var item = _payItemRepository.Get(id);
            if (item == null)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Pay Item not found" }, JsonRequestBehavior.AllowGet);
            }
            var monitor = item.SrsId > 0 ? _staffService.GetStaffById(item.SrsId) : null;
            var pls = new
            {
                id = item.Id,
                payItemId = item.PayItemId,
                shortDescription = item.ShortDescription,
                primaryUnit = item.PrimaryUnit,
                secondaryUnit = item.SecondaryUnit.HasValue ? item.SecondaryUnit : 0,
                primaryUnitCode = Enum.GetName(typeof(PrimaryUnitType), item.PrimaryUnit),
                secondaryUnitCode = item.SecondaryUnit.HasValue ? Enum.GetName(typeof(SecondaryUnitType), item.SecondaryUnit) : string.Empty,
                masterFileId = item.MyMasterFile.FileNumber,
                structureId = item.MyPayItemStructure.Id,
                structureNumber = item.MyPayItemStructure.StructureId,
                isMixedUnit = item.MyPayItemStructure.PrimaryUnit == PrimaryUnitType.Mixed,
                description = item.Description,
                isSupplementalDescriptionRequired = item.IsSupplementalDescriptionRequired.ToString(),
                isLikeItemsCombined = item.IsLikeItemsCombined.ToString(),
                isFederalFunded = item.IsFederalFunded.ToString(),
                lreReferencePrice = item.LreReferencePrice,
                concreteFactor = item.ConcreteFactor,
                dqeReferencePrice = item.DqeReferencePrice,
                asphaltFactor = item.AsphaltFactor,
                effectiveDate = item.EffectiveDate.HasValue ? item.EffectiveDate.Value.ToShortDateString() : string.Empty,
                obsoleteDate = item.ObsoleteDate.HasValue ? item.ObsoleteDate.Value.ToShortDateString() : string.Empty,
                factorNotes = item.FactorNotes,
                srsId = item.SrsId,
                costBasedTemplateId = item.MyCostBasedTemplate == null ? 0 : item.MyCostBasedTemplate.Id,
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
            };

            return new DqeResult(pls, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMonitor(int srsId)
        {
            var staff = srsId > 0 ? _staffService.GetStaffById(srsId) : null;
            var monitor = staff == null
                ? null
                : new
                {
                    id = staff.Id,
                    fullName = staff.FullName,
                    district = staff.District,
                    email = staff.Email,
                    phoneNumber = staff.PhoneAndExtension
                };
            return new DqeResult(monitor, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateFactors(dynamic factors)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);

            var items = ((IEnumerable<dynamic>)factors).ToList();

            var payItems = _payItemRepository.GetAll();
            //TODO: Jeremy - factors are decimals
            foreach (var item in payItems)
            {
                var factor = items.SingleOrDefault(i => i.id == item.Id);

                if (factor == null) continue;
                var transformer = item.GetTransformer();

                Decimal asphaltFactor;
                Decimal concreteFactor =0;
                if (Decimal.TryParse(factor.asphaltFactor, out asphaltFactor) && Decimal.TryParse(factor.concreteFactor, out concreteFactor))
                {
                    transformer.AsphaltFactor = asphaltFactor;    
                    transformer.ConcreteFactor = concreteFactor;
                }
                else
                {
                    _transactionManager.Abort();
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Pay items factors are incorrectly formatted.", ttl = 3000 });
                }

                item.Transform(transformer, currentDqeUser);
            }

            return new DqeResult(null,new ClientMessage{Severity = ClientMessageSeverity.Success,text = "Pay items have been successful updated.",ttl = 3000});
        }

        [HttpGet]
        public ActionResult GetAllPayItems()
        {
            var items = _payItemRepository.GetAll();
            if (items == null)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Pay Items not found" }, JsonRequestBehavior.AllowGet);
            }
            var payItems = items.Select(i => new
            {
                asphaltFactor = i.AsphaltFactor,
                concreteFactor = i.ConcreteFactor,
                payItemId = i.PayItemId,
                masterFileId = i.MyMasterFile.FileNumber,
                id = i.Id
            });
            return new DqeResult(payItems, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdatePayItem(dynamic payItem)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var message = payItem.id == 0 ? "Pay Item added" : "Pay Item updated";
            var pi = (PayItem)(payItem.id == 0 ? new PayItem(_payItemRepository) : _payItemRepository.Get(payItem.id));
            var pit = pi.GetTransformer();
            //dates
            pit.EffectiveDate = null;
            pit.ObsoleteDate = null;
            if (DynamicHelper.HasNotNullProperty(payItem, "effectiveDate") && !string.IsNullOrWhiteSpace(payItem.effectiveDate.ToString()))
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
            if (DynamicHelper.HasNotNullProperty(payItem, "obsoleteDate") && !string.IsNullOrWhiteSpace(payItem.obsoleteDate.ToString()))
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
            if (payItem.id == 0 && payItem.masterFileId == 0)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "A Master File must be selected." });
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
            //bools
            bool parsedBool;
            pit.IsFederalFunded = bool.TryParse(payItem.isFederalFunded, out parsedBool) && parsedBool;
            pit.IsLikeItemsCombined = bool.TryParse(payItem.isLikeItemsCombined, out parsedBool) && parsedBool;
            pit.IsSupplementalDescriptionRequired = bool.TryParse(payItem.isSupplementalDescriptionRequired, out parsedBool) && parsedBool;
            //strings
            pit.PayItemId = payItem.payItemId.ToString();
            pit.Description = payItem.description.ToString();
            pit.ShortDescription = payItem.shortDescription.ToString();
            pit.FactorNotes = payItem.factorNotes.ToString();
            //enums
            pit.PrimaryUnit = (PrimaryUnitType)payItem.primaryUnit;
            pit.SecondaryUnit = DynamicHelper.HasNotNullProperty(payItem, "secondaryUnit") ? (SecondaryUnitType?)payItem.secondaryUnit : SecondaryUnitType.None;
            pit.SrsId = payItem.srsId;
            if (payItem.costBasedTemplateId == 0)
            {
                pi.RemoveCostBasedTemplate(currentDqeUser);
            }
            else
            {
                var cbt = _costBasedTemplateRepository.Get(payItem.costBasedTemplateId);
                if (cbt != null)
                {
                    pi.AddCostBasedTemplate(cbt, currentDqeUser);
                }
            }
            if (payItem.id == 0)
            {
                var pis = _payItemStructureRepository.Get(payItem.structureId);
                var mf = _masterFileRepository.Get(payItem.masterFileId);
                pi.AssociatePayItemToStructureAndMasterFile(pis, mf);
            }
            pi.Transform(pit, currentDqeUser);
            var r = EntityValidator.Validate(_transactionManager, pi);
            if (r != null) return r;
            if (payItem.id == 0)
            {
                _commandRepository.Add(pi);
            }
            return new DqeResult(new
            {
                id = pi.Id,
                number = pi.PayItemId,
                masterFile = pi.MyMasterFile.FileNumber,
                shortDescription = pi.ShortDescription,
                unitOfMeasure = pi.PrimaryUnit == PrimaryUnitType.LS && pi.SecondaryUnit.HasValue
                    ? string.Format("{0}/{1}", Enum.GetName(typeof(PrimaryUnitType), pi.PrimaryUnit), Enum.GetName(typeof(SecondaryUnitType), pi.SecondaryUnit.Value))
                    : Enum.GetName(typeof(PrimaryUnitType), pi.PrimaryUnit),
                effectiveDate = pi.EffectiveDate.HasValue ? pi.EffectiveDate.Value.ToShortDateString() : string.Empty,
                obsoleteDate = pi.ObsoleteDate.HasValue ? pi.ObsoleteDate.Value.ToShortDateString() : string.Empty
            }, new ClientMessage { text = message });
        }
    }
}