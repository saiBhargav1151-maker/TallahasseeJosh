using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Dqe.ApplicationServices;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;
using Dqe.Domain.Services;
using Dqe.Web.ActionResults;
using Dqe.Web.Attributes;
using FDOT.Enterprise.Configuration.Client;

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    public class PayItemStructureAdministrationController : Controller
    {
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly ICommandRepository _commandRepository;
        private readonly IPayItemStructureRepository _payItemStructureRepository;
        private readonly IPayItemMasterRepository _payItemMasterRepository;
        private readonly IDqeWebLinkRepository _dqeWebLinkRepository;
        private readonly IStaffService _staffService;
        private readonly IWebTransportService _webTransportService;
        private readonly IMasterFileRepository _masterFileRepository;
        private readonly ICostBasedTemplateRepository _costBasedTemplateRepository;
        private readonly ILreService _lreService;
        private readonly ITransactionManager _transactionManager;

        public PayItemStructureAdministrationController
            (
            IDqeUserRepository dqeUserRepository,
            ICommandRepository commandRepository,
            IPayItemStructureRepository payItemStructureRepository,
            IPayItemMasterRepository payItemMasterRepository,
            IDqeWebLinkRepository dqeWebLinkRepository,
            IStaffService staffService,
            IWebTransportService webTransportService,
            IMasterFileRepository masterFileRepository,
            ICostBasedTemplateRepository costBasedTemplateRepository,
            ILreService lreService,
            ITransactionManager transactionManager
            )
        {
            _dqeUserRepository = dqeUserRepository;
            _commandRepository = commandRepository;
            _payItemStructureRepository = payItemStructureRepository;
            _payItemMasterRepository = payItemMasterRepository;
            _dqeWebLinkRepository = dqeWebLinkRepository;
            _staffService = staffService;
            _webTransportService = webTransportService;
            _masterFileRepository = masterFileRepository;
            _costBasedTemplateRepository = costBasedTemplateRepository;
            _lreService = lreService;
            _transactionManager = transactionManager;
        }

        //TODO: add obsolete date and flag for structure.  date is informational, and flag controls the visibility of the structure on the boe. - rule is all structure items must be past obsolete date

        [HttpGet]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult GetCostBasedTemplates()
        {
            var result = _costBasedTemplateRepository.GetAll().Select(i => new
            {
                name = i.Name,
                id = i.Id,
                selected = false
            });
            return new DqeResult(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult GetLrePickLists()
        {
            var result = _lreService.GetLrePickLists().OrderBy(i => i.GroupDescription).Select(i => new
            {
                description = i.GroupDescription,
                id = i.Id,
                selected = false
            });
            return new DqeResult(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult GetUnitCodes()
        {
            return GetCodes("UNITS", true);
        }

        [HttpGet]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult GetUnitTypeCodes()
        {
            return GetCodes("UNITTYP");
        }

        [HttpGet]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult GetItemTypeCodes()
        {
            return GetCodes("ITEMTYP");
        }

        [HttpGet]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult GetFuelAdjustmentTypeCodes()
        {
            return GetCodes("FUELTYP");
        }

        [HttpGet]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult GetItemClassCodes()
        {
            return GetCodes("ITEMCLS");
        }

        [HttpGet]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult GetContractClassCodes()
        {
            return GetCodes("CONTCLS");
        }

        [HttpGet]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult GetPrecisionCodes()
        {
            var result = new List<dynamic>
            {
                new 
                {
                    name = "(0)",
                    description = "1 Whole"
                },
                new 
                {
                    name = "(1)",
                    description = "1/10"
                },
                new 
                {
                    name = "(2)",
                    description = "1/100"
                },
                new 
                {
                    name = "(3)",
                    description = "1/1000"
                },
            };
            return new DqeResult(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        //[CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult GetSpecBooks()
        {
            var mfs = _masterFileRepository.GetAll();
            var result = mfs.Select(i => new
            {
                name = i.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                description = i.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                copyMasterFile = i.DoMasterFileCopy,
                copyDate = i.EffectiveDate
            });
            return new DqeResult(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        //[CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult GetCurrentSpecBook()
        {
            var mfs = _masterFileRepository.GetAll();
            var specBooks = mfs.Select(i => new
            {
                id = i.Id,
                name = i.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                description = i.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                copyMasterFile = i.DoMasterFileCopy,
                effectiveDate = i.EffectiveDate
            }).OrderByDescending(i => i.name);
            var latest = string.Empty;
            foreach (var specBook in specBooks)
            {
                if (specBook.name.StartsWith("9")) continue;
                latest = specBook.name;
                break;
            }
            var latestSpecBook = specBooks.First(i => i.name == latest);
            var result = new
            {
                latestSpecBook.id,
                specBook = latestSpecBook.name, 
                latestSpecBook.description, 
                latestSpecBook.copyMasterFile,
                latestSpecBook.effectiveDate
            };
            return new DqeResult(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetWorkTypes()
        {
            var code = _webTransportService.GetCodeTable("WRKTYP");
            return new DqeResult(code.CodeValues.OrderBy(i => i.Description).Select(i => new
            {
                name = i.Description,
                code = i.CodeValueName
            }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetStructureForItem(int specYear, string itemName)
        {
            var structure = _payItemStructureRepository.Get(specYear, itemName);
            if (structure == null)
            {
                return new DqeResult(null, new ClientMessage{ Severity = ClientMessageSeverity.Error, text = "Pay Item not found"}, JsonRequestBehavior.AllowGet);
            }
            var result = new
            {
                id = structure.Id,
                name = string.IsNullOrWhiteSpace(structure.StructureId) ? string.Empty : structure.StructureId,
                title = string.IsNullOrWhiteSpace(structure.Title) ? string.Empty : structure.Title,
                unit = !structure.Units.Any()
                    ? string.Empty
                    : structure.Units.Count == 1
                        ? structure.Units[0]
                        : "MIXED"
            };
            var l = new List<object> {result};
            return new DqeResult(l, JsonRequestBehavior.AllowGet);
        }

        private ActionResult GetCodes(string codeName, bool omitParens = false)
        {
            var code = _webTransportService.GetCodeTable(codeName);
            var result = code.CodeValues.Where(i => !i.ObsoleteDate.HasValue || i.ObsoleteDate.Value.Date > DateTime.Now.Date).Select(i => new
            {
                name = omitParens ? i.CodeValueName : string.Format("({0})", i.CodeValueName),
                description = i.Description
            }).OrderBy(i => i.name);
            return new DqeResult(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult GetUnlinkedItems(string val)
        {
            var result = _payItemMasterRepository.GetHeaders(val).Select(i => new
            {
                id = i.Id,
                name = i.RefItemName,
                specBook = i.SpecBook,
                description = i.Description,
                unit = i.BidAsLumpSum
                    ? string.Format("LS/{0}", i.Unit)
                    : i.Unit,
                validDate = i.OpenedDate.HasValue
                    ? i.OpenedDate.Value.ToShortDateString()
                    : string.Empty,
                obsoleteDate = i.ObsoleteDate.HasValue
                    ? i.ObsoleteDate.Value.ToShortDateString()
                    : string.Empty,
                isObsolete = i.ObsoleteDate.HasValue && (i.ObsoleteDate.Value.Date > DateTime.Now.Date)
            }).OrderBy(i => i.name);
            return new DqeResult(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult GetStructures()
        {
            var structures = _payItemStructureRepository.GetAllHeaders(string.Empty, true);
            var result = structures.Select(i => new
            {
                id = i.Id,
                name = string.IsNullOrWhiteSpace(i.StructureId) ? string.Empty : i.StructureId,
                title = string.IsNullOrWhiteSpace(i.Title) ? string.Empty : i.Title,
                unit = !i.Units.Any()
                    ? string.Empty
                    : i.Units.Count == 1
                        ? i.Units[0]
                        : "MIXED"
            });
            return new DqeResult(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetStructuresRange(string set, bool currentStructuresOnly)
        {
            var structures = _payItemStructureRepository.GetAllHeaders(set, currentStructuresOnly);
            var result = structures.Select(i => new
            {
                id = i.Id,
                name = string.IsNullOrWhiteSpace(i.StructureId) ? string.Empty : i.StructureId,
                title = string.IsNullOrWhiteSpace(i.Title) ? string.Empty : i.Title,
                unit = !i.Units.Any()
                    ? string.Empty
                    : i.Units.Count == 1
                        ? i.Units[0]
                        : "MIXED"
            });
            return new DqeResult(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetStructureDetail(int structureId)
        {
            var structure = _payItemStructureRepository.Get(structureId);
            var result = new
            {
                id = structure.Id,
                name = string.IsNullOrWhiteSpace(structure.StructureId) ? string.Empty : structure.StructureId,
                title = string.IsNullOrWhiteSpace(structure.Title) ? string.Empty : structure.Title,
                planQuantity = !structure.IsPlanQuantity.HasValue ? "Yes/No" : structure.IsPlanQuantity.Value ? "Yes" : "No",
                notes = structure.Notes,
                details = structure.Details,
                planSummary = structure.PlanSummary,
                ppmChapterText = structure.PpmChapterText,
                trnsportText = structure.TrnsportText,
                otherText = structure.OtherText,
                standardsText = structure.StandardsText,
                specificationsText = structure.SpecificationsText,
                structureDetails = structure.StructureDescription,
                requiredItems = structure.RequiredItems,
                recommendedItems = structure.RecommendedItems,
                replacementItems = structure.ReplacementItems,
                ppmChapters = structure.PpmChapters.Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    webLink = i.WebLink
                }),
                prepAndDocChapters = structure.PrepAndDocChapters.Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    webLink = i.WebLink
                }),
                otherReferences = structure.OtherReferences.Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    webLink = i.WebLink
                }),
                standards = structure.Standards.Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    webLink = i.WebLink
                }),
                specifications = structure.Specifications.Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    webLink = i.WebLink
                })
            };
            return new DqeResult(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [CustomAuthorize(Roles = new[] {DqeRole.Administrator, DqeRole.PayItemAdministrator})]
        public ActionResult GetProtectedItemDetail(int itemId)
        {
            var item = _payItemMasterRepository.Get(itemId);
            var lreItem = _lreService.GetLrePayItem(item.RefItemName);
            var pls = _lreService.GetLrePickLists().OrderBy(i => i.GroupDescription).Select(i => new
            {
                description = i.GroupDescription,
                id = i.Id,
                selected = lreItem != null && lreItem.PayItemPayItemGroups.FirstOrDefault(ii => ii.MyPayItemGroup.Id == i.Id && ii.Status == "A") != null
            });
            var result = new
            {
                specBook = item.SpecBook,
                isObsolete = item.ObsoleteDate.HasValue && (item.ObsoleteDate.Value.Date > DateTime.Now.Date),
                unit = item.BidAsLumpSum
                    ? string.Format("LS/{0}", item.Unit)
                    : item.Unit,
                administrative = item.Administrative,
                alternateItemName = string.IsNullOrWhiteSpace(item.AlternateItemName) ? string.Empty : item.AlternateItemName,
                asphaltFactor = item.AsphaltFactor,
                autoPaidPercentSchedule = item.AutoPaidPercentSchedule,
                bidAsLumpSum = item.BidAsLumpSum,
                bidRequirementCode = string.IsNullOrWhiteSpace(item.BidRequirementCode) ? string.Empty : item.BidRequirementCode,
                calculatedUnit = string.IsNullOrWhiteSpace(item.CalculatedUnit) ? string.Empty : item.CalculatedUnit,
                coApprovalRequired = item.CoApprovalRequired,
                combineWithLikeItems = item.CombineWithLikeItems,
                commonUnit = string.IsNullOrWhiteSpace(item.CommonUnit) ? string.Empty : item.CommonUnit,
                concreteFactor = item.ConcreteFactor,
                contractClass = string.IsNullOrWhiteSpace(item.ContractClass) ? string.Empty : string.Format("({0})", item.ContractClass),
                conversionFactorToCommonUnit = item.ConversionFactorToCommonUnit.HasValue ? item.ConversionFactorToCommonUnit.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                description = string.IsNullOrWhiteSpace(item.Description) ? string.Empty : item.Description,
                effectiveDate = item.EffectiveDate.HasValue ? item.EffectiveDate.Value.ToShortDateString() : string.Empty,
                exemptFromMaa = item.ExemptFromMaa,
                exemptFromRetainage = item.ExemptFromRetainage,
                factorNotes = string.IsNullOrWhiteSpace(item.FactorNotes) ? string.Empty : item.FactorNotes,
                fuelAdjustment = item.FuelAdjustment,
                fuelAdjustmentType = string.IsNullOrWhiteSpace(item.FuelAdjustmentType) ? string.Empty : string.Format("({0})", item.FuelAdjustmentType),
                id = item.Id,
                ildt2 = item.Ildt2.HasValue ? item.Ildt2.Value.ToShortDateString() : string.Empty,
                ilflg1 = string.IsNullOrWhiteSpace(item.Ilflg1) ? string.Empty : item.Ilflg1,
                specType = string.IsNullOrWhiteSpace(item.Ilflg1) ? string.Empty : item.Ilflg1,
                illst1 = string.IsNullOrWhiteSpace(item.Illst1) ? string.Empty : item.Illst1,
                ilnum1 = item.Ilnum1.HasValue ? item.Ilnum1.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                ilsst1 = string.IsNullOrWhiteSpace(item.Ilsst1) ? string.Empty : item.Ilsst1,
                isFederalFunded = item.IsFederalFunded,
                isFixedPrice = !string.IsNullOrWhiteSpace(item.BidRequirementCode) && item.BidRequirementCode.ToUpper().Trim() == "FIXED",
                itemClass = string.IsNullOrWhiteSpace(item.ItemClass) ? string.Empty : string.Format("({0})", item.ItemClass),
                itemType = string.IsNullOrWhiteSpace(item.ItemType) ? string.Empty : string.Format("({0})", item.ItemType),
                itmqtyprecsn = string.IsNullOrWhiteSpace(item.Itmqtyprecsn) ? string.Empty : string.Format("({0})", item.Itmqtyprecsn),
                lumpSum = item.LumpSum,
                majorItem = item.MajorItem,
                nonBid = item.NonBid,
                obsoleteDate = item.ObsoleteDate.HasValue ? item.ObsoleteDate.Value.ToShortDateString() : string.Empty,
                validDate = item.OpenedDate.HasValue ? item.OpenedDate.Value.ToShortDateString() : string.Empty,
                payPlan = item.PayPlan,
                percentScheduleItem = item.PercentScheduleItem,
                recordSource = string.IsNullOrWhiteSpace(item.RecordSource) ? string.Empty : item.RecordSource,
                name = item.RefItemName,
                refPrice = item.RefPrice.HasValue ? item.RefPrice.Value : 0,
                regressionInclusion = item.RegressionInclusion,
                shortDescription = string.IsNullOrWhiteSpace(item.ShortDescription) ? string.Empty : item.ShortDescription,
                specialtyItem = item.SpecialtyItem,
                stateReferencePrice = item.StateReferencePrice.HasValue ? item.StateReferencePrice.Value : 0,
                suppDescriptionRequired = item.SuppDescriptionRequired,
                itemUnit = string.IsNullOrWhiteSpace(item.Unit) ? string.Empty : item.Unit,
                unitSystem = string.IsNullOrWhiteSpace(item.UnitSystem) ? string.Empty : item.UnitSystem,
                primaryUnit = item.LumpSum || item.BidAsLumpSum ? "LS" : item.Unit,
                hybridUnit = item.Unit,
                isNonPart = !string.IsNullOrWhiteSpace(item.Illst1) && item.Illst1.ToUpper().Trim() == "NPART",
                isFrontLoadedItem = item.IsFrontLoadedItem,
                costBasedTemplate = item.MyCostBasedTemplate == null ? 0 : item.MyCostBasedTemplate.Id,
                lrePickLists = pls
            };
            return new DqeResult(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult GetProtectedStructureDetail(int structureId)
        {
            var structure = _payItemStructureRepository.Get(structureId);
            var monitor = structure.SrsId > 0
                ? _staffService.GetStaffById(structure.SrsId)
                : null;
            var result = new
            {
                id = structure.Id,
                name = string.IsNullOrWhiteSpace(structure.StructureId) ? string.Empty : structure.StructureId,
                title = string.IsNullOrWhiteSpace(structure.Title) ? string.Empty : structure.Title,
                planQuantity = !structure.IsPlanQuantity.HasValue
                    ? "Yes/No"
                    : structure.IsPlanQuantity.Value
                        ? "Yes"
                        : "No",
                notes = structure.Notes,
                isObsolete = structure.IsObsolete,
                details = structure.Details,
                planSummary = structure.PlanSummary,
                ppmChapterText = structure.PpmChapterText,
                trnsportText = structure.TrnsportText,
                otherText = structure.OtherText,
                standardsText = structure.StandardsText,
                specificationsText = structure.SpecificationsText,
                structureDetails = structure.StructureDescription,
                requiredItems = structure.RequiredItems,
                recommendedItems = structure.RecommendedItems,
                replacementItems = structure.ReplacementItems,
                //protected data
                boeRecentChangeDate = structure.BoeRecentChangeDate.HasValue
                    ? structure.BoeRecentChangeDate.Value.ToShortDateString()
                    : string.Empty,
                boeRecentChangeDescription = structure.BoeRecentChangeDescription,
                essHistory = structure.EssHistory,
                pendingInformation = structure.PendingInformation,
                srsId = structure.SrsId,
                monitor = monitor == null 
                    ? new
                    {
                        fullName = string.Empty,
                        email = string.Empty,
                        phoneNumber = string.Empty
                    }
                    : new
                    {
                        fullName = monitor.FullName,
                        email = monitor.Email,
                        phoneNumber = monitor.PhoneAndExtension
                    },
                ppmChapters = structure.PpmChapters.Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    webLink = i.WebLink
                }),
                prepAndDocChapters = structure.PrepAndDocChapters.Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    webLink = i.WebLink
                }),
                otherReferences = structure.OtherReferences.Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    webLink = i.WebLink
                }),
                standards = structure.Standards.Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    webLink = i.WebLink
                }),
                specifications = structure.Specifications.Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    webLink = i.WebLink
                })
            };
            return new DqeResult(result, JsonRequestBehavior.AllowGet);
        }

        private object GetItemHeaders(int structureId, bool currentItemsOnly)
        {
            var structure = _payItemStructureRepository.Get(structureId);
            var items = currentItemsOnly
                ? structure.PayItemMasters.Where(i => !i.ObsoleteDate.HasValue || i.ObsoleteDate.Value.Date >= DateTime.Now.Date)
                : structure.PayItemMasters;
            var result = items.Select(i => new
            {
                id = i.Id,
                specBook = i.MyMasterFile.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                name = i.RefItemName,
                description = i.Description,
                unit = i.BidAsLumpSum
                    ? string.Format("LS/{0}", i.Unit)
                    : i.Unit,
                validDate = i.EffectiveDate.HasValue
                    ? i.EffectiveDate.Value.ToShortDateString()
                    : string.Empty,
                obsoleteDate = i.ObsoleteDate.HasValue
                    ? i.ObsoleteDate.Value.ToShortDateString()
                    : string.Empty,
                isObsolete = i.ObsoleteDate.HasValue && (i.ObsoleteDate.Value.Date < DateTime.Now.Date),
                specType = i.Ilflg1
            });
            return result;
        }

        [HttpGet]
        public ActionResult GetItemHeadersForStructure(int structureId, bool currentItemsOnly)
        {
            return new DqeResult(GetItemHeaders(structureId, currentItemsOnly), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPayItemsBySpecBook(string specBook)
        {
            var structures = _payItemMasterRepository.GetPayItemsWithStructureInfo(specBook);
            var isAuthorized = false;
            var currentUser = User.Identity as DqeIdentity;
            if (currentUser != null && currentUser.IsAuthenticated)
            {
                var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
                if (currentDqeUser.IsActive)
                {
                    isAuthorized = true;
                }
            }
            var security = new
            {
                isAuthorized
            };
            var payItems = structures.Select(i => new
            {
                id = i.Id,
                name = i.RefItemName,
                description = i.Description,
                unit = i.Unit ?? string.Empty,
                detail = string.Empty,
                itemClass = i.ItemClass ?? string.Empty,
                specTech = i.Ilflg1 ?? string.Empty,
                combFlag = i.CombineWithLikeItems == false ? "N" : "Y",
                isObsolete = i.ObsoleteDate.HasValue && i.ObsoleteDate < DateTime.Today,
                obsoleteFlag = i.ObsoleteDate.HasValue && i.ObsoleteDate < DateTime.Today ? "Y" : "N",
                openDate = i.EffectiveDate.HasValue ? i.EffectiveDate.Value.ToShortDateString() : string.Empty,
                obsoleteDate = i.ObsoleteDate.HasValue ? i.ObsoleteDate.Value.ToShortDateString() : string.Empty,
                referencePrice = i.RefPrice == 0 || i.RefPrice == null ? null : ((decimal)i.RefPrice).ToString("C"),
                strucutreId = i.MyPayItemStructure != null ? i.MyPayItemStructure.Id : 0,
                hasStructure = i.MyPayItemStructure != null
            });
            var result = new
            {
                payItems,
                security
            };
            return new DqeResult(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult AddItemToStructure(dynamic structure)
        {
            var pis = (PayItemStructure)_payItemStructureRepository.Get(structure.id);
            var item = _payItemMasterRepository.Get((int)structure.itemToAdd);
            pis.AddPayItem(item);
            return new DqeResult(null);
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult RemoveItemToStructure(dynamic item)
        {
            var pi = _payItemMasterRepository.Get((int)item.id);
            pi.MyPayItemStructure.RemovePayItem(pi);
            return new DqeResult(null);
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult RemoveStructure(dynamic structure)
        {
            var pis = (PayItemStructure)_payItemStructureRepository.Get(structure.id);
            _commandRepository.Remove(pis);
            return new DqeResult(null);
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] {DqeRole.Administrator, DqeRole.PayItemAdministrator})]
        public ActionResult SaveItem(dynamic item)
        {
            item.name = ((string)item.safeName.ToString()).TrimStart('~');
            var codes = _webTransportService.GetCodeTable("UNITS");
            if (item.name == null || string.IsNullOrWhiteSpace(item.name.ToString()) )
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Pay Item ID is required" });
            }
            if (item.description == null || string.IsNullOrWhiteSpace(item.description.ToString()))
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Pay Item Description is required" });
            }
            if (item.shortDescription == null || string.IsNullOrWhiteSpace(item.shortDescription.ToString()))
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Pay Item Short Description is required" });
            }
            if (item.primaryUnit == null || string.IsNullOrWhiteSpace(item.primaryUnit.ToString()))
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Pay Item Unit is required" });
            }
            if (item.primaryUnit == "LS" && (item.hybridUnit == null || string.IsNullOrWhiteSpace(item.hybridUnit.ToString())))
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Pay Item Unit is required" });
            }
            if (item.itemClass == null || string.IsNullOrWhiteSpace(item.itemClass.ToString()))
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Pay Item Item Class is required" });
            }
            if (item.itemType == null || string.IsNullOrWhiteSpace(item.itemType.ToString()))
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Pay Item Type is required" });
            }
            if (item.contractClass == null || string.IsNullOrWhiteSpace(item.contractClass.ToString()))
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Pay Item Contract Class is required" });
            }
            var lreItemSelected = false;
            foreach (var pl in item.lrePickLists)
            {
                if (!bool.Parse(pl.selected.ToString())) continue;
                lreItemSelected = true;
                break;
            }
            if (item.id == 0)
            {
                var cps = _payItemMasterRepository.GetByName((string)(item.name.ToString())).ToList();
                if (cps.Any(i => i.MyMasterFile.FileNumber == int.Parse(item.specBook.ToString())))
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "There is an existing Pay Item with this ID and Spec Book" });
                }
                if (!lreItemSelected)
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "At least one LRE pick list must be specified." });
                }
            }
            else
            {
                var tp = (PayItemMaster)_payItemMasterRepository.Get(item.id);
                var mf = tp.MyMasterFile;
                var cps = _payItemMasterRepository.GetByName((string)(item.name.ToString())).Where(i => i.Id != tp.Id).ToList();
                if (cps.Any(i => i.MyMasterFile.Id == mf.Id))
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "There is an existing Pay Item with this ID and Spec Book" });
                }
            }
            DateTime parseDate;
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var pi = item.id == 0 ? new PayItemMaster() : (PayItemMaster) _payItemMasterRepository.Get(item.id);
            var pit = pi.GetTransformer();
            pit.Administrative = item.administrative;
            pit.AlternateItemName = item.alternateItemName.ToString();
            pit.AsphaltFactor = string.IsNullOrWhiteSpace(item.asphaltFactor.ToString()) ? 0 : decimal.Parse(item.asphaltFactor.ToString());
            pit.AutoPaidPercentSchedule = item.autoPaidPercentSchedule;
            pit.BidAsLumpSum = item.primaryUnit == "LS";
            pit.BidRequirementCode = item.isFixedPrice ? "Fixed" : null;
            pit.CoApprovalRequired = item.coApprovalRequired;
            pit.CombineWithLikeItems = item.combineWithLikeItems;
            pit.CommonUnit = item.commonUnit == null ? null : item.commonUnit.ToString();
            pit.ConcreteFactor = string.IsNullOrWhiteSpace(item.concreteFactor.ToString()) ? 0 : decimal.Parse(item.concreteFactor.ToString());
            pit.ContractClass = item.contractClass == null ? null : item.contractClass.ToString().TrimStart("(".ToCharArray()).TrimEnd(")".ToCharArray());
            pit.ConversionFactorToCommonUnit = string.IsNullOrWhiteSpace(item.conversionFactorToCommonUnit.ToString()) ? 0 : decimal.Parse(item.conversionFactorToCommonUnit.ToString());
            if (item.id == 0)
            {
                pit.CreatedBy = "DQE";
                pit.CreatedDate = DateTime.Now;
                pit.SpecBook = item.specBook.ToString().PadLeft(2, '0');
            }
            pit.Description = item.description.ToString();
            if (item.effectiveDate == null)
            {
                pit.EffectiveDate = null;
            }
            else if (DateTime.TryParse(item.effectiveDate.ToString(), out parseDate))
            {
                pit.EffectiveDate = parseDate;
            }
            else
            {
                pit.EffectiveDate = null;
            }
            if (!pit.EffectiveDate.HasValue)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Pay Item Effective Date is required" });
            }
            if (item.obsoleteDate == null)
            {
                pit.ObsoleteDate = null;
            }
            else if (DateTime.TryParse(item.obsoleteDate.ToString(), out parseDate))
            {
                pit.ObsoleteDate = parseDate;
            }
            else
            {
                pit.ObsoleteDate = null;
            }
            if (item.validDate == null)
            {
                pit.OpenedDate = null;
            }
            else if (DateTime.TryParse(item.validDate.ToString(), out parseDate))
            {
                pit.OpenedDate = parseDate;
            }
            else
            {
                pit.OpenedDate = null;
            }
            if (!pit.OpenedDate.HasValue)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Pay Item Opened Date is required" });
            }
            pit.ExemptFromMaa = item.exemptFromMaa;
            pit.ExemptFromRetainage = item.exemptFromRetainage;
            pit.FactorNotes = item.factorNotes.ToString();
            pit.FuelAdjustment = item.fuelAdjustment;
            pit.FuelAdjustmentType = item.fuelAdjustment ? item.fuelAdjustmentType.ToString().TrimStart("(".ToCharArray()).TrimEnd(")".ToCharArray()) : null;
            if (item.ildt2 == null)
            {
                pit.Ildt2 = null;
            }
            else if (DateTime.TryParse(item.ildt2.ToString(), out parseDate))
            {
                pit.Ildt2 = parseDate;
            }
            else
            {
                pit.Ildt2 = null;
            }
            pit.Illst1 = item.isNonPart ? "NPART" : null;
            pit.Ilsst1 = item.ilsst1.ToString();
            pit.Ilflg1 = item.specType.ToString();
            pit.IsFederalFunded = item.isFederalFunded;
            pit.IsFixedPrice = item.isFixedPrice;
            pit.ItemClass = string.IsNullOrWhiteSpace(item.itemClass.ToString()) ? null : item.itemClass.ToString().TrimStart("(".ToCharArray()).TrimEnd(")".ToCharArray());
            pit.ItemType = string.IsNullOrWhiteSpace(item.itemType.ToString()) ? null : item.itemType.ToString().TrimStart("(".ToCharArray()).TrimEnd(")".ToCharArray());
            pit.Itmqtyprecsn = string.IsNullOrWhiteSpace(item.itmqtyprecsn.ToString()) ? null : item.itmqtyprecsn.ToString().TrimStart("(".ToCharArray()).TrimEnd(")".ToCharArray());
            pit.LastUpdatedBy = "DQE";
            pit.LastUpdatedDate = DateTime.Now;
            pit.LumpSum = item.hybridUnit == "LS";
            pit.MajorItem = item.majorItem;
            pit.NonBid = item.nonBid;
            pit.PayPlan = item.payPlan;
            pit.PercentScheduleItem = item.percentScheduleItem;
            pit.RecordSource = item.recordSource;
            pit.RefItemName = item.name.ToString();
            pit.RefPrice = string.IsNullOrWhiteSpace(item.refPrice.ToString()) ? 0 : decimal.Parse(item.refPrice.ToString());
            pit.RegressionInclusion = item.regressionInclusion;
            pit.ShortDescription = item.shortDescription.ToString();
            pit.SpecialtyItem = item.specialtyItem;
            pit.SuppDescriptionRequired = item.suppDescriptionRequired;
            pit.Unit = item.primaryUnit == "LS" ? item.hybridUnit : item.primaryUnit;
            pit.IsFrontLoadedItem = item.isFrontLoadedItem;
            var code = codes.CodeValues.First(i => i.CodeValueName == item.primaryUnit);
            pit.CalculatedUnit = item.primaryUnit == "LS"
                ? "LS - Lump Sum"
                : string.Format("{0} - {1}", code.CodeValueName, code.Description);
            pit.UnitSystem = item.unitSystem.ToString();
            pi.Transform(pit, currentDqeUser);
            if (item.costBasedTemplate == 0)
            {
                pi.SetCostBasedTemplate(null);
            }
            else
            {
                var cbt = _costBasedTemplateRepository.Get(item.costBasedTemplate);
                pi.SetCostBasedTemplate(cbt);
            }
            var updateLrePayItems = Convert.ToBoolean(ConfigurationManager.AppSettings["updateLrePayItems"]);
            var updateWtPayItems = Convert.ToBoolean(ConfigurationManager.AppSettings["updateWtPayItems"]);
            if (pi.Id == 0)
            {
                var mf = (MasterFile)_masterFileRepository.GetByFileNumber(int.Parse(item.specBook.ToString()));
                mf.AddPayItemMaster(pi);
                var structure = (PayItemStructure)_payItemStructureRepository.Get(item.structureId);
                structure.AddPayItem(pi);
                _commandRepository.Flush();
                if (updateWtPayItems)
                {
                    _webTransportService.UpdateRefItem(pi, currentDqeUser);    
                }
                if (updateLrePayItems && lreItemSelected)
                {
                    _lreService.UpdateRefItem(pi, item.lrePickLists, currentDqeUser);    
                }
                var result = GetItemHeaders(item.structureId, item.showCurrent);
                return new DqeResult(result, new ClientMessage { Severity = ClientMessageSeverity.Success, text = "Pay Item Added" });
            }
            if (updateWtPayItems)
            {
                _webTransportService.UpdateRefItem(pi, currentDqeUser);
            }
            if (updateLrePayItems && lreItemSelected)
            {
                _lreService.UpdateRefItem(pi, item.lrePickLists, currentDqeUser);
            }
            return new DqeResult(null, new ClientMessage{ Severity = ClientMessageSeverity.Success, text = "Pay Item Updated"});
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult SaveStructure(dynamic structure)
        {
            if (structure.name == null || string.IsNullOrWhiteSpace(structure.name.ToString()) ||
                structure.title == null || string.IsNullOrWhiteSpace(structure.title.ToString()))
            {
                return new DqeResult(null, new ClientMessage{Severity = ClientMessageSeverity.Error, text = "Pay Item Structure ID and Title are required"});
            }
            if (structure.id == 0)
            {
                var s = (PayItemStructure) _payItemStructureRepository.GetByStructureId(structure.name, null);
                if (s != null)
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "There is an existing Pay Item Structure with this Structure ID" });
                }
            }
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var pis = structure.id == 0
                ? new PayItemStructure(_payItemStructureRepository)
                : (PayItemStructure)_payItemStructureRepository.Get(structure.id);
            var pist = pis.GetTransformer();
            pist.Details = structure.details.ToString();
            pist.IsPlanQuantity = structure.planQuantity.ToString() == "Yes/No" ? null : structure.planQuantity.ToString() == "Yes";
            pist.Notes = structure.notes.ToString();
            pist.IsObsolete = structure.isObsolete;
            pist.OtherText = structure.otherText.ToString();
            pist.PlanSummary = structure.planSummary.ToString();
            pist.PpmChapterText = structure.ppmChapterText.ToString();
            pist.SpecificationsText = structure.specificationsText.ToString();
            pist.StandardsText = structure.standardsText.ToString();
            pist.StructureDescription = structure.structureDetails.ToString();
            pist.StructureId = structure.name.ToString();
            pist.Title = structure.title.ToString();
            pist.TrnsportText = structure.trnsportText.ToString();
            if (structure.srsId != 0)
            {
                pist.SrsId = structure.srsId;
            }
            //protected
            DateTime parsedDate;
            pist.BoeRecentChangeDate = DateTime.TryParse(structure.boeRecentChangeDate.ToString(), out parsedDate)
                ? parsedDate
                : (DateTime?)null;
            pist.BoeRecentChangeDescription = structure.boeRecentChangeDescription;
            pist.EssHistory = structure.essHistory;
            pist.PendingInformation = structure.pendingInformation;
            pist.RequiredItems = structure.requiredItems != null ? structure.requiredItems.ToString() : null;
            pist.RecommendedItems = structure.recommendedItems != null ? structure.recommendedItems.ToString() : null;
            pist.ReplacementItems = structure.replacementItems != null ? structure.replacementItems.ToString() : null;
            pis.Transform(pist, currentDqeUser);
            _commandRepository.Add(pis);
            //links
            pis.ClearPpmChapters(currentDqeUser);
            foreach (var ppmChapter in structure.ppmChapters)
            {
                var pc = (PpmChapterWebLink)_dqeWebLinkRepository.Get((int)ppmChapter.id);
                if (pc != null)
                {
                    pis.AddPpmChapter(pc, currentDqeUser);
                }
            }
            pis.ClearPrepAndDocChapters(currentDqeUser);
            foreach (var prepAndDocChapter in structure.prepAndDocChapters)
            {
                var pc = (PrepAndDocChapterWebLink)_dqeWebLinkRepository.Get((int)prepAndDocChapter.id);
                if (pc != null)
                {
                    pis.AddPrepAndDocChapter(pc, currentDqeUser);
                }
            }
            pis.ClearOtherReferences(currentDqeUser);
            foreach (var otherReference in structure.otherReferences)
            {
                var pc = (OtherReferenceWebLink)_dqeWebLinkRepository.Get((int)otherReference.id);
                if (pc != null)
                {
                    pis.AddOtherReference(pc, currentDqeUser);
                }
            }
            pis.ClearSpecifications(currentDqeUser);
            foreach (var specification in structure.specifications)
            {
                var pc = (SpecificationWebLink)_dqeWebLinkRepository.Get((int)specification.id);
                if (pc != null)
                {
                    pis.AddSpecification(pc, currentDqeUser);
                }
            }
            pis.ClearStandards(currentDqeUser);
            foreach (var standard in structure.standards)
            {
                var pc = (StandardWebLink)_dqeWebLinkRepository.Get((int)standard.id);
                if (pc != null)
                {
                    pis.AddStandard(pc, currentDqeUser);
                }
            }
            var units =
                pis.PayItemMasters.Select(i => i.BidAsLumpSum ? string.Format("LS/{0}", i.Unit) : i.Unit)
                    .Distinct()
                    .ToList();
            var result = new
            {
                id = pis.Id,
                name = string.IsNullOrWhiteSpace(pis.StructureId) ? string.Empty : pis.StructureId,
                title = string.IsNullOrWhiteSpace(pis.Title) ? string.Empty : pis.Title,
                unit = !units.Any()
                    ? string.Empty
                    : units.Count == 1
                        ? units[0]
                        : "MIXED"
            };
            return new DqeResult(result, new ClientMessage{ Severity = ClientMessageSeverity.Success, text = string.Format("Pay Item Structure {0}", structure.id == 0 ? "Added" : "Updated")});
        }
    }
}