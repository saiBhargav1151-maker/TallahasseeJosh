using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using ClosedXML.Excel;
using Dqe.ApplicationServices;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;
using Dqe.Domain.Model.Reports;
using Dqe.Domain.Repositories.Custom;
using Dqe.Web.ActionResults;
using Dqe.Web.Attributes;
using BidHistory = Dqe.Domain.Model.BidHistory;

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    [CustomAuthorize(Roles = new[] {DqeRole.Administrator, DqeRole.AdminReadOnly, DqeRole.DistrictAdministrator, DqeRole.Estimator, DqeRole.Coder })]
    public class EstimateController : Controller
    {
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProposalRepository _proposalRepository;
        private readonly IWebTransportService _webTransportService;
        private readonly IPayItemMasterRepository _payItemMasterRepository;
        private readonly IPricingEngine _pricingEngine;
        private readonly IMarketAreaRepository _marketAreaRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly ICostBasedTemplateRepository _costBasedTemplateRepository;
        private readonly IReportRepository _reportRepository;
        
        public EstimateController
            (
            IDqeUserRepository dqeUserRepository,
            IProjectRepository projectRepository,
            IWebTransportService webTransportService,
            IPayItemMasterRepository payItemMasterRepository,
            IPricingEngine pricingEngine,
            IMarketAreaRepository marketAreaRepository,
            IProposalRepository proposalRepository,
            ITransactionManager transactionManager,
            ICostBasedTemplateRepository costBasedTemplateRepository,
            IReportRepository reportRepository
            )
        {
            _dqeUserRepository = dqeUserRepository;
            _projectRepository = projectRepository;
            _webTransportService = webTransportService;
            _payItemMasterRepository = payItemMasterRepository;
            _pricingEngine = pricingEngine;
            _marketAreaRepository = marketAreaRepository;
            _proposalRepository = proposalRepository;
            _transactionManager = transactionManager;
            _costBasedTemplateRepository = costBasedTemplateRepository;
            _reportRepository = reportRepository;
        }

        [HttpPost]
        public ActionResult TransferLsDbEstimate(dynamic lsdbLoad)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var detailProject = (Project)_projectRepository.GetDetailProjectForLsBd(lsdbLoad.number, currentDqeUser);
            ProjectEstimate estimate = null;
            foreach (var version in detailProject.ProjectVersions.Where(i => i.VersionOwner == currentDqeUser))
            {
                foreach (var est in version.ProjectEstimates.Where(est => est.IsWorkingEstimate))
                {
                    estimate = est;
                    break;
                }
            }
            if (estimate == null)
            {
                return new DqeResult(null, new ClientMessage{Severity = ClientMessageSeverity.Error, text = "Detail estimate cannot be determined."});
            }
            var estimateTotal = estimate.GetEstimateTotalWithItems();
            var lsdbEstimate = (ProjectEstimate)_projectRepository.GetEstimate(lsdbLoad.estimateId);
            var lsdbEstimateTotal = lsdbEstimate.GetEstimateTotalWithItems();
            if (lsdbEstimateTotal.CategorySets.Count > 1)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "For-Bid estimate cannot be summerized because alternate sets were found." });
            }
            //identify included alternates
            var total = estimateTotal.Total;
            var detailCategories = (IEnumerable<CategorySet>)estimateTotal.CategorySets.Where(i => i.Included).ToList();
            var detailItems = new List<ProjectItem>();
            foreach (var cs in detailCategories)
            {
                foreach (var iset in cs.ItemSets)
                {
                    detailItems.AddRange(iset.ProjectItems);
                }
            }
            var summaryItems = lsdbEstimateTotal.CategorySets[0].ItemSets[0].ProjectItems;
            var matchedItems = new List<string>();
            foreach (var summaryItem in summaryItems)
            {
                foreach (var detailItem in detailItems)
                {
                    if (summaryItem.PayItemNumber == detailItem.PayItemNumber)
                    {
                        matchedItems.Add(summaryItem.PayItemNumber);
                    }
                }
            }
            var summaryBucket = string.Empty;
            foreach (var summaryItem in summaryItems)
            {
                if (matchedItems.Contains(summaryItem.PayItemNumber)) continue;
                if (summaryBucket != string.Empty)
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "For-Bid estimate contains more than one summary item." });
                }
                summaryBucket = summaryItem.PayItemNumber;
            }
            if (summaryBucket == string.Empty)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "For-Bid estimate does not contain a summary item." });
            }
            var matchedItemList = detailItems.Where(i => matchedItems.Contains(i.PayItemNumber)).Select(i => new
            {
                payItemName = i.PayItemNumber,
                total = Math.Round(detailItems.Where(ii => ii.PayItemNumber == i.PayItemNumber).Sum(ii => ii.Price), 2)
            }).ToList();
            var summaryBucketItem = new
            {
                payItemName = summaryBucket,
                total = Math.Round(detailItems.Where(i => !matchedItems.Contains(i.PayItemNumber)).Sum(i => Math.Round(i.Price * i.Quantity, 2, MidpointRounding.AwayFromZero)), 2, MidpointRounding.AwayFromZero)
            };
            return new DqeResult(new
            {
                matchedItemList,
                summaryBucketItem
            }, new ClientMessage{Severity = ClientMessageSeverity.Success, text = "For-Bid project summarized. Please save the estimate if you want to keep this estimate."});
        }

        [HttpPost]
        public ActionResult LoadProjectEstimate(dynamic loadId)
        {
            return CreateProjectEstimateStructure(loadId.loadId);
        }

        [HttpPost]
        public ActionResult LoadProjectEstimateSummary(dynamic estimateId)
        {
            var estimate = _projectRepository.GetEstimate(estimateId.estimateId);
            return new DqeResult(new { total = estimate.GetEstimateTotal() });
        }

        [HttpPost]
        public ActionResult LoadProposalEstimate(dynamic loadId)
        {
            if (loadId.loadId == 0)
            {
                return new DqeResult(null);
            }
            return CreateProposalEstimateStructure(loadId.loadId);
        }

        [HttpPost]
        public ActionResult LoadProposalEstimateSummary(dynamic estimateId)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var proposal = _proposalRepository.GetById(estimateId.estimateId);
            return new DqeResult(new { total = proposal.GetEstimateTotal(currentDqeUser)});    
        }

        [HttpPost]
        public ActionResult DoProposalSync(dynamic estimateId)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var proposal = (Proposal)_proposalRepository.GetById(estimateId.estimateId);
            proposal.SetCurrentEstimator(currentDqeUser);
            var wtp = _webTransportService.GetProposal(proposal.ProposalNumber);
            var success = proposal.SynchronizeStructure(wtp, currentDqeUser, false);
            if (success)
            {
                foreach (var section in proposal.SectionGroups)
                {
                    foreach (var item in section.ProposalItems)
                    {
                        if (!item.ProjectItems.Any())
                        {
                            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Proposal structure is invalid in Project Pre-Construction.  Proposal items without project items were found." });
                        }
                    }
                }
                return new DqeResult(null);
            }
            return new DqeResult(null, new ClientMessage{ Severity = ClientMessageSeverity.Error, text = "Proposal structure is invalid.  Please check the proposal's projects synchronization status."});
        }

        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator })]
        [HttpPost]
        public ActionResult SaveCostBasedTemplate()
        {
            var id = Convert.ToInt32(Request.Form["baseTemplateId"]);
            var template = _costBasedTemplateRepository.Get(id);
            if (Request.Files == null || Request.Files.Count <= 0 || Request.Files[0] == null)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "The file upload failed" });
            }
            if (!Request.Files[0].FileName.ToUpper().EndsWith(".XLSX"))
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "The uploaded file must be a .xlsx excel file" });
            }

            using (var workbook = new XLWorkbook(Request.Files[0].InputStream))
            {
                if (workbook.Worksheets.Count < 1)
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "The uploaded file contains no worksheets" });
                }
                var worksheet = workbook.Worksheets.First();
                var cell = worksheet.Cell(template.ResultCell);
                decimal value;
                try
                {
                    value = Convert.ToDecimal(cell.ValueCached);
                }
                catch
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "The result cell in the template does not contain a valid decimal value" });
                }
                return new DqeResult(new
                {
                    itemId = Convert.ToInt32(Request.Form["itemId"]),
                    templatePrice = value
                });
            }
        }

        private IList<object> BuildProjectLsDbItemGroups(ProjectEstimate estimate)
        {
            var l = new List<object>();
            var summaryGroups = estimate.EstimateGroups.Where(i => i.IsLsDbSummary).ToList();
            foreach (var itemGroup in summaryGroups)
            {
                foreach (var item in itemGroup.ProjectItems)
                {
                    l.Add(new
                    {
                        itemId = item.Id,
                        itemCategory = string.Format("{0} {1}", item.MyEstimateGroup.Name, item.MyEstimateGroup.Description),
                        categoryAlternateMember = itemGroup.AlternateMember,
                        categoryAlternateSet = itemGroup.AlternateSet,
                        categoryDescription = itemGroup.Description,
                        federalConstructionClass = itemGroup.FederalConstructionClass,
                        fund = itemGroup.ProjectItems.Select(i => i.Fund).Distinct().Count() > 1
                            ? "MIXED"
                            : itemGroup.ProjectItems.ToList()[0].Fund,
                        itemFund = item.Fund,
                        itemAlternateMember = item.AlternateMember,
                        itemAlternateSet = item.AlternateSet,
                        itemDescription = item.PayItemDescription,
                        itemNumber = item.PayItemNumber,
                        supplementalDescription = item.SupplementalDescription,
                        unit = item.Unit,
                        combineCategories = itemGroup.CombineWithLikeItems,
                        combineItems = item.CombineWithLikeItems,
                        quantity = item.Quantity,
                        price = item.Price,
                        previousPrice = item.PreviousPrice
                    });
                }
            }
            return l;
        }

        private IList<object> BuildProjectItemGroups(ProjectEstimate estimate)
        {
            var payItems = _payItemMasterRepository.GetAllWithPrices().ToList();
            var igl = estimate
                .GetItemGroups()
                .OrderBy(i => i.FederalConstructionClass)
                .ThenBy(i => i.CategoryDescription)
                .ThenBy(i => i.ItemNumber)
                .ThenBy(i => i.CategoryAlternateSet)
                .ThenBy(i => i.CategoryAlternateMember)
                .ThenBy(i => i.ItemAlternateSet)
                .ThenBy(i => i.ItemAlternateMember)
                .ThenBy(i => i.SupplementalDescription);
            var l = new List<object>();
            var key = 0;
            foreach (var itemGroup in igl)
            {
                var pis = itemGroup.ProjectItems.Where(i => i.Price > 0).ToList();
                var average = pis.Count == 0 ? 0 : pis.Count == 1 ? pis[0].Price : Math.Round(pis.Sum(i => (i.Price * i.Quantity)) / pis.Sum(i => i.Quantity), 2, MidpointRounding.AwayFromZero);
                //[lg]var average = pis.Count == 0 ? 0 : pis.Count == 1 ? pis[0].Price : Math.Round(pis.Sum(i => Math.Round(i.Price * i.Quantity, 2, MidpointRounding.AwayFromZero)) / pis.Sum(i => i.Quantity), 2, MidpointRounding.AwayFromZero);
                //var average = pis.Count == 0 ? 0 : Math.Round(pis.Sum(i => i.Price) / pis.Count(), 2);
                var priceType = itemGroup.ProjectItems.Any(i => i.PriceSet == PriceSetType.SystemOverride)
                    ? "X"
                    : itemGroup.ProjectItems.First().PriceSet == PriceSetType.NotSet
                        ? "N"
                        : itemGroup.ProjectItems.First().PriceSet == PriceSetType.EstimatorOverride
                            ? "O"
                            : itemGroup.ProjectItems.First().PriceSet == PriceSetType.Statewide
                                ? "S"
                                : itemGroup.ProjectItems.First().PriceSet == PriceSetType.MarketArea
                                    ? "M"
                                    : itemGroup.ProjectItems.First().PriceSet == PriceSetType.County
                                        ? "C"
                                        : itemGroup.ProjectItems.First().PriceSet == PriceSetType.Reference
                                            ? "R"
                                            : itemGroup.ProjectItems.First().PriceSet == PriceSetType.Fixed
                                                ? "F"
                                                : itemGroup.ProjectItems.First().PriceSet == PriceSetType.Template
                                                    ? "T"
                                                    : "P";
                key += 1;
                var payItem = payItems.FirstOrDefault(i => i.RefItemName == itemGroup.ItemNumber);
                var costBasedTemplateId = payItem == null || payItem.MyCostBasedTemplate == null ? 0 : payItem.MyCostBasedTemplate.Id;
                itemGroup.Unit = payItem == null
                    ? itemGroup.Unit
                    : payItem.CalculatedUnit.ToUpper().StartsWith("LS")
                        ? payItem.Unit.ToUpper() == "LS"
                            ? payItem.CalculatedUnit
                            : string.Format("{0} ({1})", payItem.CalculatedUnit, payItem.Unit)
                        : payItem.CalculatedUnit;
                var marketAreaAverage = payItem == null 
                    ? null 
                    : payItem.MarketAreaAveragePrices.FirstOrDefault(i => i.MyMarketArea.Counties.Contains(estimate.MyProjectVersion.MyProject.MyCounty));
                var countyAverage = payItem == null
                    ? null
                    : payItem.CountyAveragePrices.FirstOrDefault(i => i.MyCounty == estimate.MyProjectVersion.MyProject.MyCounty);
                var statewidePrice = payItem == null
                    ? 0
                    : payItem.StateReferencePrice.HasValue
                        ? Math.Round(payItem.StateReferencePrice.Value, 2)
                        : 0;
                var marketAreaPrice = marketAreaAverage == null 
                    ? 0 
                    : Math.Round(marketAreaAverage.Price, 2);
                var countyPrice = countyAverage == null 
                    ? 0 
                    : Math.Round(countyAverage.Price, 2);
                //var isFixedPrice = payItem != null && (!string.IsNullOrWhiteSpace(payItem.BidRequirementCode) && (payItem.BidRequirementCode.ToUpper().Trim() == "FIXED"));
                //var isFixedPrice = payItem != null && payItem.NonBid;
                var isFixedPrice = payItem != null
                    && (!string.IsNullOrWhiteSpace(payItem.BidRequirementCode)
                    && (payItem.BidRequirementCode.ToUpper().Trim() == "FIXED"))
                    && payItem.RefPrice.HasValue
                    && payItem.RefPrice.Value > 0;
                var referencePrice = payItem == null 
                    ? 0 
                    : payItem.RefPrice.HasValue ? Math.Round(payItem.RefPrice.Value, 2) : 0;
                var fixedPrice = isFixedPrice ? payItem.RefPrice.HasValue ? Math.Round(payItem.RefPrice.Value, 2) : 0 : 0;
                var isObsolete = payItem == null || payItem.ObsoleteDate.HasValue && (payItem.ObsoleteDate.Value.Date <= DateTime.Now.Date);
                l.Add(new
                {
                    isFirst = true,
                    rowSpan = 1,
                    key,
                    itemId = itemGroup.ProjectItems.Count == 1 ? itemGroup.ProjectItems[0].Id : 0,
                    itemIds = itemGroup.ProjectItems.Select(i => i.Id).ToArray(),
                    group = true,
                    canExpand = itemGroup.ProjectItems.Count > 1,
                    isExpanded = false,
                    itemCategory = string.Empty,
                    categoryAlternateMember = itemGroup.CategoryAlternateMember,
                    categoryAlternateSet = itemGroup.CategoryAlternateSet,
                    categoryDescription = itemGroup.CategoryDescription,
                    federalConstructionClass = itemGroup.FederalConstructionClass,
                    fund = itemGroup.ProjectItems.Select(i => i.Fund).Distinct().Count() > 1
                        ? "MIXED"
                        : itemGroup.ProjectItems[0].Fund,
                    itemFund = string.Empty,
                    itemAlternateMember = itemGroup.ItemAlternateMember,
                    itemAlternateSet = itemGroup.ItemAlternateSet,
                    itemDescription = itemGroup.ItemDescription,
                    itemNumber = itemGroup.ItemNumber,
                    supplementalDescription = itemGroup.SupplementalDescription,
                    unit = itemGroup.Unit,
                    combineCategories = itemGroup.CombineCategories,
                    combineItems = itemGroup.CombineItems,
                    quantity = itemGroup.Quantity,
                    isFixedPrice,
                    price = isFixedPrice ? fixedPrice : average,
                    previousPrice = itemGroup.ProjectItems.Count > 1 
                        ? string.Empty
                        : itemGroup.ProjectItems[0].Price == average
                                ? itemGroup.ProjectItems[0].PreviousPrice.HasValue
                                    ? itemGroup.ProjectItems[0].PreviousPrice.Value.ToString(CultureInfo.InvariantCulture)
                                    : string.Empty
                                : itemGroup.ProjectItems[0].Price.ToString(CultureInfo.InvariantCulture),
                    holdPrice = isFixedPrice ? fixedPrice : average,
                    statewidePrice, 
                    marketAreaPrice, 
                    countyPrice,
                    parameterPrice = itemGroup.ProjectItems[0].PriceSet == PriceSetType.Parameter
                        ? itemGroup.ProjectItems[0].Price == average 
                            ? average 
                            : (decimal)0 
                        : (decimal)0, 
                    referencePrice,
                    fixedPrice,
                    templatePrice = itemGroup.ProjectItems[0].PriceSet == PriceSetType.Template
                        ? itemGroup.ProjectItems[0].Price == average
                            ? average
                            : (decimal)0
                        : (decimal)0, 
                    priceType = isFixedPrice ? "F" : priceType,
                    holdPriceType = isFixedPrice ? "F" : priceType,
                    isSystemOverride = false, 
                    costBasedTemplateId,
                    isObsolete
                });
                if (itemGroup.ProjectItems.Count > 1)
                {
                    var isFirst = true;
                    foreach (var item in itemGroup.ProjectItems)
                    {
                        priceType = item.PriceSet == PriceSetType.NotSet
                            ? "N"
                            : item.PriceSet == PriceSetType.EstimatorOverride
                                ? "O"
                                : item.PriceSet == PriceSetType.Statewide
                                    ? "S"
                                    : item.PriceSet == PriceSetType.MarketArea
                                        ? "M"
                                        : item.PriceSet == PriceSetType.County
                                            ? "C"
                                            : item.PriceSet == PriceSetType.Parameter
                                                ? "P"
                                                : item.PriceSet == PriceSetType.Reference
                                                    ? "R"
                                                    : item.PriceSet == PriceSetType.Fixed
                                                        ? "F"
                                                        : item.PriceSet == PriceSetType.Template
                                                            ? "T"
                                                            : "X";
                        l.Add(new
                        {
                            isFirst,
                            rowSpan = (isFirst) ? itemGroup.ProjectItems.Count : 1,
                            key,
                            itemId = item.Id,
                            itemIds = new[] { item.Id },
                            group = false,
                            canExpand = false,
                            isExpanded = false,
                            itemCategory = string.Format("{0} {1}", item.MyEstimateGroup.Name, item.MyEstimateGroup.Description),
                            categoryAlternateMember = itemGroup.CategoryAlternateMember,
                            categoryAlternateSet = itemGroup.CategoryAlternateSet,
                            categoryDescription = itemGroup.CategoryDescription,
                            federalConstructionClass = itemGroup.FederalConstructionClass,
                            fund = itemGroup.ProjectItems.Select(i => i.Fund).Distinct().Count() > 1
                                ? "MIXED"
                                : itemGroup.ProjectItems[0].Fund,
                            itemFund = item.Fund,
                            itemAlternateMember = itemGroup.ItemAlternateMember,
                            itemAlternateSet = itemGroup.ItemAlternateSet,
                            itemDescription = itemGroup.ItemDescription,
                            itemNumber = itemGroup.ItemNumber,
                            supplementalDescription = itemGroup.SupplementalDescription,
                            unit = itemGroup.Unit,
                            combineCategories = itemGroup.CombineCategories,
                            combineItems = itemGroup.CombineItems,
                            quantity = item.Quantity,
                            isFixedPrice,
                            price = isFixedPrice ? fixedPrice : average,
                            previousPrice = item.Price == average 
                                ? item.PreviousPrice.HasValue
                                    ? item.PreviousPrice.Value
                                    : 0
                                : item.Price,
                            holdPrice = isFixedPrice ? fixedPrice : average,
                            statewidePrice = (decimal)0,
                            marketAreaPrice = (decimal)0,
                            countyPrice = (decimal)0,
                            parameterPrice = (decimal)0,
                            referencePrice = (decimal)0,
                            fixedPrice = (decimal)0,
                            templatePrice = (decimal)0,
                            priceType = isFixedPrice ? "F" : priceType,
                            holdPriceType = isFixedPrice ? "F" : priceType,
                            isSystemOverride = item.Price != average,
                            costBasedTemplateId,
                            isObsolete
                        });
                        isFirst = false;
                    }
                }
            }
            return l;
        }

        private DqeResult CreateProjectEstimateStructure(int estimateId)
        {
            var estimate = _projectRepository.GetEstimate(estimateId);
            var project = estimate.MyProjectVersion.MyProject;
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);       
            if (currentDqeUser.Role != DqeRole.Administrator && currentDqeUser.Role != DqeRole.AdminReadOnly && project.CustodyOwner != currentDqeUser)
            {
                return new DqeResult(null, new ClientMessage{ Severity = ClientMessageSeverity.Error, text = "You must have custody to price the project."});
            }
            if (currentDqeUser.Role != DqeRole.Administrator && currentDqeUser.Role != DqeRole.AdminReadOnly)
            {
                if (!currentDqeUser.IsInDqeDistrict(project.District) && !currentDqeUser.IsAuthorizedOnProject(project) && currentDqeUser.Role != DqeRole.Coder)
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "You are not authorized to price the project." });
                }    
            }
            var wtProposal = project.Proposals.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);

            Domain.Model.Wt.Proposal wtp = null;

            if (wtProposal != null)
            {
                //set actual wt proposal entity here, to create confidential data flag. MB.
                wtp = _webTransportService.GetProposal(wtProposal.ProposalNumber);
            }
           
            var l = BuildProjectItemGroups(estimate);
            var result = new
            {
                viewOnly = (currentDqeUser.Role == DqeRole.Administrator && project.CustodyOwner != currentDqeUser) || currentDqeUser.Role == DqeRole.AdminReadOnly ,
                canEstimate = project.CustodyOwner == currentDqeUser ? true : false,
                isSystemSync = true,
                estimateId = estimate.Id,
                isOfficial = project.GetCurrentSnapshotLabel() == SnapshotLabel.Official,
                proposal = new
                {
                    id = 0,
                    confidentialData = wtp?.OfficialEstimate == "Y" && wtp?.ProposalStatus != "03" //data is confidential iff official estimate AND not executed yet.
                },
                project = new
                {
                    id = project.Id,
                    specYear = project.MyMasterFile.FileNumber.ToString(CultureInfo.InvariantCulture),
                    number = project.ProjectNumber,
                    description = project.Description,
                    county = project.MyCounty.Name,
                    district = project.District,
                    isLsDb = project.WtLsDbId != 0,
                    lettingDate =
                        wtProposal == null
                            ? string.Empty
                            : wtProposal.LettingDate.HasValue
                                ? wtProposal.LettingDate.Value.ToShortDateString()
                                : string.Empty,
                    designer = project.DesignerName
                },
                lsDbItems = BuildProjectLsDbItemGroups(estimate),
                itemGroups = l,
                total = new EstimateTotal()
            };
            return new DqeResult(result, JsonRequestBehavior.AllowGet);
        }

        private IList<object> BuildProposalItemGroups(Proposal proposal)
        {
            var payItems = _payItemMasterRepository.GetAllWithPrices().ToList();
            var estimateGroups = proposal.GetItemGroups()
                .OrderBy(i => i.FederalConstructionClass)
                .ThenBy(i => i.CategoryDescription)
                .ThenBy(i => i.ItemNumber)
                .ThenBy(i => i.CategoryAlternateSet)
                .ThenBy(i => i.CategoryAlternateMember)
                .ThenBy(i => i.ItemAlternateSet)
                .ThenBy(i => i.ItemAlternateMember)
                .ThenBy(i => i.SupplementalDescription)
                .ToList();
            var l = new List<object>();
            var key = 0;
            
            foreach (var itemGroup in estimateGroups)

            {
                if (!itemGroup.ProjectItems.Any()) continue;
                
                   var pis = itemGroup.ProjectItems.Where(i => i.Price > 0).ToList();
                var average = pis.Count == 0 ? 0 : pis.Count == 1 ? pis[0].Price : Math.Round(pis.Sum(i => (i.Price * i.Quantity)) / pis.Sum(i => i.Quantity), 2, MidpointRounding.AwayFromZero);
                //[lg] var average = pis.Count == 0 ? 0 : pis.Count == 1 ? pis[0].Price : Math.Round(pis.Sum(i => Math.Round(i.Price * i.Quantity, 2, MidpointRounding.AwayFromZero)) / pis.Sum(i => i.Quantity), 2, MidpointRounding.AwayFromZero);
                //var average = pis.Count == 0 ? 0 : Math.Round(pis.Sum(i => i.Price) / pis.Count(), 2, MidpointRounding.AwayFromZero);
                var projects = itemGroup.ProjectItems.Select(i => i.MyEstimateGroup.MyProjectEstimate.MyProjectVersion.MyProject).Distinct().ToList();
                var priceType = itemGroup.ProjectItems.Select(i => i.Price).Distinct().Count() > 1
                    ? "X"
                    : itemGroup.ProjectItems.Select(i => i.PriceSet).Distinct().Count() > 1
                        ? "X"
                        : itemGroup.ProjectItems.Any(i => i.PriceSet == PriceSetType.SystemOverride)
                            ? "X"
                            : itemGroup.ProjectItems.First().PriceSet == PriceSetType.NotSet
                                ? "N"
                                : itemGroup.ProjectItems.First().PriceSet == PriceSetType.EstimatorOverride
                                    ? "O"
                                    : itemGroup.ProjectItems.First().PriceSet == PriceSetType.Statewide
                                        ? "S"
                                        : itemGroup.ProjectItems.First().PriceSet == PriceSetType.MarketArea
                                            ? "M"
                                            : itemGroup.ProjectItems.First().PriceSet == PriceSetType.County
                                                ? "C"
                                                : itemGroup.ProjectItems.First().PriceSet == PriceSetType.Reference
                                                    ? "R"
                                                    : itemGroup.ProjectItems.First().PriceSet == PriceSetType.Fixed
                                                        ? "F"
                                                        : itemGroup.ProjectItems.First().PriceSet == PriceSetType.Template
                                                            ? "T"
                                                            : "P";
                key += 1;
                var payItem = payItems.FirstOrDefault(i => i.RefItemName == itemGroup.ItemNumber);
                var costBasedTemplateId = payItem == null || payItem.MyCostBasedTemplate == null ? 0 : payItem.MyCostBasedTemplate.Id;
                itemGroup.Unit = payItem == null
                    ? itemGroup.Unit
                    : payItem.CalculatedUnit.ToUpper().StartsWith("LS")
                        ? payItem.Unit.ToUpper() == "LS"
                            ? payItem.CalculatedUnit
                            : string.Format("{0} ({1})", payItem.CalculatedUnit, payItem.Unit)
                        : payItem.CalculatedUnit;

                var marketAreaAverage = payItem == null
                    ? null
                    : payItem.MarketAreaAveragePrices.FirstOrDefault(i => i.MyMarketArea.Counties.Contains(proposal.County));
                var countyAverage = payItem == null
                    ? null
                    : payItem.CountyAveragePrices.FirstOrDefault(i => i.MyCounty == proposal.County);
                var statewidePrice = payItem == null
                    ? 0
                    : payItem.StateReferencePrice.HasValue
                        ? Math.Round(payItem.StateReferencePrice.Value, 2)
                        : 0;
                var marketAreaPrice = marketAreaAverage == null
                    ? 0
                    : Math.Round(marketAreaAverage.Price, 2);
                var countyPrice = countyAverage == null
                    ? 0
                    : Math.Round(countyAverage.Price, 2);
                //only use the non-bid 
                var isFixedPrice = payItem != null 
                    && (!string.IsNullOrWhiteSpace(payItem.BidRequirementCode) 
                    && (payItem.BidRequirementCode.ToUpper().Trim() == "FIXED"))
                    && payItem.RefPrice.HasValue 
                    && payItem.RefPrice.Value > 0;
                var referencePrice = payItem == null
                    ? 0
                    : payItem.RefPrice.HasValue ? Math.Round(payItem.RefPrice.Value, 2) : 0;
                var fixedPrice = isFixedPrice ? payItem.RefPrice.HasValue ? Math.Round(payItem.RefPrice.Value, 2) : 0 : 0;
                var isObsolete = payItem == null || payItem.ObsoleteDate.HasValue && (payItem.ObsoleteDate.Value.Date <= (proposal.LettingDate.HasValue ? proposal.LettingDate.Value.Date : DateTime.Now.Date));
                l.Add(new
                {
                    isFirst = true,
                    rowSpan = 1,
                    key,
                    itemId = itemGroup.ProjectItems.Count == 1 ? itemGroup.ProjectItems[0].Id : 0,
                    itemIds = itemGroup.ProjectItems.Select(i => i.Id).ToArray(),
                    group = true,
                    canExpand = itemGroup.ProjectItems.Count > 1,
                    isExpanded = false,
                    itemCategory = string.Empty,
                    categoryAlternateMember = itemGroup.CategoryAlternateMember,
                    categoryAlternateSet = itemGroup.CategoryAlternateSet,
                    categoryDescription = itemGroup.CategoryDescription,
                    federalConstructionClass = itemGroup.FederalConstructionClass,
                    fund = itemGroup.Fund,
                    itemFund = string.Empty,
                    itemAlternateMember = itemGroup.ItemAlternateMember,
                    itemAlternateSet = itemGroup.ItemAlternateSet,
                    itemDescription = itemGroup.ItemDescription,
                    itemNumber = itemGroup.ItemNumber,
                    supplementalDescription = itemGroup.SupplementalDescription,
                    unit = itemGroup.Unit,
                    combineCategories = itemGroup.CombineCategories,
                    combineItems = itemGroup.CombineItems,
                    quantity = itemGroup.Quantity,
                    isFixedPrice,
                    price = isFixedPrice ? fixedPrice : average,
                    previousPrice = itemGroup.ProjectItems.Count > 1
                        ? string.Empty
                        : itemGroup.ProjectItems[0].Price == average
                            ? itemGroup.ProjectItems[0].PreviousPrice.HasValue 
                                ? itemGroup.ProjectItems[0].PreviousPrice.Value.ToString(CultureInfo.InvariantCulture) 
                                : string.Empty
                            : itemGroup.ProjectItems[0].Price.ToString(CultureInfo.InvariantCulture),
                    holdPrice = isFixedPrice ? fixedPrice : average,
                    statewidePrice,
                    marketAreaPrice,
                    countyPrice,
                    //parameterPrice = (decimal)0,
                    parameterPrice = itemGroup.ProjectItems[0].PriceSet == PriceSetType.Parameter
                        ? itemGroup.ProjectItems[0].Price == average
                            ? average
                            : (decimal)0
                        : (decimal)0,
                    referencePrice,
                    fixedPrice,
                    templatePrice = itemGroup.ProjectItems[0].PriceSet == PriceSetType.Template
                        ? itemGroup.ProjectItems[0].Price == average
                            ? average
                            : (decimal)0
                        : (decimal)0,
                    projectNumber = projects.Aggregate(string.Empty, (current, project) => current + string.Format("{0} ", project.ProjectNumber)).TrimEnd(' '),
                    itemProjectNumber = projects.Count == 1 ? projects[0].ProjectNumber : "Multiple",
                    priceType = isFixedPrice ? "F" : priceType,
                    holdPriceType = isFixedPrice ? "F" : priceType,
                    isSystemOverride = false,
                    costBasedTemplateId,
                    isObsolete
                });
                if (itemGroup.ProjectItems.Count > 1)
                {
                    var isFirst = true;
                    foreach (var item in itemGroup.ProjectItems)
                    {
                        //priceType = item.PriceSet == PriceSetType.NotSet
                        //    ? "N"
                        //    : item.PriceSet == PriceSetType.EstimatorOverride
                        //        ? "O"
                        //        : item.PriceSet == PriceSetType.Statewide
                        //            ? "S"
                        //            : item.PriceSet == PriceSetType.MarketArea
                        //                ? "M"
                        //                : item.PriceSet == PriceSetType.County
                        //                    ? "C"
                        //                    : item.PriceSet == PriceSetType.Parameter
                        //                        ? "P"
                        //                        : item.PriceSet == PriceSetType.Reference
                        //                            ? "R"
                        //                            : item.PriceSet == PriceSetType.Fixed
                        //                                ? "F"
                        //                                : "X";
                        l.Add(new
                        {
                            isFirst,
                            rowSpan = (isFirst) ? itemGroup.ProjectItems.Count : 1,
                            key,
                            itemId = item.Id,
                            itemIds = new[] {item.Id},
                            group = false,
                            canExpand = false,
                            isExpanded = false,
                            itemCategory =
                                string.Format("{0} {1}", item.MyEstimateGroup.Name, item.MyEstimateGroup.Description),
                            categoryAlternateMember = itemGroup.CategoryAlternateMember,
                            categoryAlternateSet = itemGroup.CategoryAlternateSet,
                            categoryDescription = itemGroup.CategoryDescription,
                            federalConstructionClass = itemGroup.FederalConstructionClass,
                            fund = itemGroup.ProjectItems.Select(i => i.Fund).Distinct().Count() > 1
                                ? "MIXED"
                                : itemGroup.ProjectItems[0].Fund,
                            itemFund = item.Fund,
                            itemAlternateMember = itemGroup.ItemAlternateMember,
                            itemAlternateSet = itemGroup.ItemAlternateSet,
                            itemDescription = itemGroup.ItemDescription,
                            itemNumber = itemGroup.ItemNumber,
                            supplementalDescription = itemGroup.SupplementalDescription,
                            unit = itemGroup.Unit,
                            combineCategories = itemGroup.CombineCategories,
                            combineItems = itemGroup.CombineItems,
                            quantity = item.Quantity,
                            isFixedPrice,
                            price = isFixedPrice ? fixedPrice : average,
                            previousPrice = item.Price == average 
                                ? item.PreviousPrice.HasValue
                                    ? item.PreviousPrice.Value
                                    : 0
                                : item.Price,
                            holdPrice = isFixedPrice ? fixedPrice : average,
                            statewidePrice = (decimal)0,
                            marketAreaPrice = (decimal)0,
                            countyPrice = (decimal)0,
                            parameterPrice = (decimal)0,
                            referencePrice = (decimal)0,
                            fixedPrice = (decimal)0,
                            templatePrice = (decimal)0,
                            projectNumber =
                                projects.Aggregate(string.Empty,
                                    (current, project) => current + string.Format("{0} ", project.ProjectNumber))
                                    .TrimEnd(' '),
                            itemProjectNumber =
                                item.MyEstimateGroup.MyProjectEstimate.MyProjectVersion.MyProject.ProjectNumber,
                            priceType = isFixedPrice ? "F" : priceType,
                            holdPriceType = isFixedPrice ? "F" : priceType,
                            isSystemOverride = item.Price != average,
                            costBasedTemplateId,
                            isObsolete
                        });
                        isFirst = false;
                    }
                }
            }
            return l;
        }

        [HttpPost]
        public ActionResult WriteProposalPrices(dynamic proposal)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);

            if (!proposal.takeLabeledSnapshot) return new DqeResult(null);
            var p = (Proposal)_proposalRepository.GetById(proposal.id);
            var nextSnapshot = p.GetNextSnapshotLabel();
            if (nextSnapshot != SnapshotLabel.Authorization && nextSnapshot != SnapshotLabel.Official) return new DqeResult(null);

            //validate non-zero prices
            //TODO: should this check start from proposal level or project level?
            var hasZeroPrices = false;
            foreach (var sectionGroup in p.SectionGroups)
            {
                if (hasZeroPrices) break;
                foreach (var proposalItem in sectionGroup.ProposalItems)
                {
                    if (hasZeroPrices) break;
                    foreach (var projectItem in proposalItem.ProjectItems)
                    {
                        if (projectItem.Price <= 0)
                        {
                            hasZeroPrices = true;
                            break;
                        }
                    }
                }
            }
            if (hasZeroPrices)
            {
                return new DqeResult(null, new ClientMessage{ Severity = ClientMessageSeverity.Error, text = "Zero prices detected - Please verify that all items have a price greater than zero."});
            }
            //end non-zero price validation

            var authorizationTotal = (Decimal)0;
            var officialTotal = (Decimal)0;
            Domain.Model.Wt.Letting letting = null;
            if (nextSnapshot == SnapshotLabel.Official || nextSnapshot == SnapshotLabel.Authorization)
            {
                //build report structure
               try
                {
                    _proposalRepository.BuildReportProposal(proposal.id, currentDqeUser, _payItemMasterRepository, _reportRepository, _webTransportService, false);

                    letting = _webTransportService.GetLettingByProposal(p.ProposalNumber);

                    //We might not have a letting yet for the proposal
                    if (letting != null)
                    {
                        letting = _webTransportService.GetLetting(letting.LettingName);

                        //delete letting data
                        _reportRepository.DeleteLettingData(letting, false);

                        //rebuild
                        _reportRepository.RebuildReportStructure(letting, null, null, false, new List<PayItemMaster>());
                    }
                }
                catch (InvalidOperationException exception)
                {
                    if (exception.Message.StartsWith("SYSTEMERROR:"))
                    {
                        //the proposal->project structure in wT is invalid.  Give the user a message they can provide to wT folks instead of an 'Unexpected Error'
                        return new DqeResult(null,
                            new ClientMessage
                            {
                                Severity = ClientMessageSeverity.Error,
                                text = exception.Message.Replace("SYSTEMERROR:", string.Empty)
                            });
                    }
                    throw;
                }

                _transactionManager.Flush();

                var authorizationEstimate = _reportRepository.GetReportProposal(p.ProposalNumber, ReportProposalLevel.Authorization);
                authorizationTotal = authorizationEstimate == null ? 0 : authorizationEstimate.Total;

                if (nextSnapshot == SnapshotLabel.Official)
                {
                    var officialEstimate = _reportRepository.GetReportProposal(p.ProposalNumber, ReportProposalLevel.Official);
                    officialTotal = officialEstimate == null ? 0 : officialEstimate.Total;
                }
            }

            var readyForOfficial = _webTransportService.IsProposalReadyForOfficialEstimate(p.ProposalNumber);

            if (nextSnapshot == SnapshotLabel.Official && !readyForOfficial)
            {
                var officialResult = new
                {
                    isOfficial = true,
                    authorizationTotal = authorizationTotal,
                    officialTotal = officialTotal
                };
                return new DqeResult(officialResult, JsonRequestBehavior.AllowGet);
            }

            //not needed since DSS is decommissioned, dont need to pass to DSS
            //if (nextSnapshot == SnapshotLabel.Official)
            //{
            //    if (letting != null)
            //    {
            //        var wtProposal = letting.Proposals.First(i => i.ProposalNumber == p.ProposalNumber);
            //        if (!string.IsNullOrEmpty(wtProposal.PassToDss) && (wtProposal.PassToDss == "B" || wtProposal.PassToDss == "D"))
            //        {

            //            _webTransportService.UpdateProposalReadyForDssPass(wtProposal);
            //        }
            //    }
            //}

            var message = _webTransportService.UpdatePrices(p, nextSnapshot == SnapshotLabel.Official, currentDqeUser);
            var result = new
            {
                authorizationTotal = authorizationTotal,
                officialTotal = officialTotal
            };
            if (string.IsNullOrWhiteSpace(message)) return new DqeResult(result, JsonRequestBehavior.AllowGet);
            return new DqeResult(null, new ClientMessage{ Severity = ClientMessageSeverity.Error, text = message});
        }

        private DqeResult CreateProposalEstimateStructure(int estimateId)
        {
            var proposal = _proposalRepository.GetById(estimateId);
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var itemsCount = proposal.SectionGroups.Sum(i => i.ProposalItems.Count());
            //TODO: need to set wt proposal here, to create confidential data flag. MB.
            Domain.Model.Wt.Proposal wtp = null;

            if (proposal != null)
            {
                //set actual wt proposal entity here, to create confidential data flag. MB.
                wtp = _webTransportService.GetProposal(proposal.ProposalNumber);
            }

            if ((currentDqeUser.Role != DqeRole.Administrator && currentDqeUser.Role != DqeRole.AdminReadOnly && proposal.CurrentEstimator != currentDqeUser) || itemsCount == 0)
            {
                proposal.SetCurrentEstimator(currentDqeUser);
                proposal.SynchronizeStructure(wtp, currentDqeUser, false);
            }


            var projects = proposal.Projects.OrderBy(i => i.ProjectNumber).Select(i => new
            {
                HasCustody = i.CustodyOwner == currentDqeUser,
                HasWorkingEstimate = i.ProjectHasWorkingEstimateForUser(currentDqeUser)
            }).ToList();

            if (currentDqeUser.Role == DqeRole.Administrator || currentDqeUser.Role == DqeRole.AdminReadOnly)
            {
                projects = proposal.Projects.OrderBy(i => i.ProjectNumber).Select(ii => new
                {
                    HasCustody = ii.CustodyOwner == currentDqeUser,
                    HasWorkingEstimate = ii.ProjectVersions.Any(pv => pv.ProjectEstimates.Any(e => e.IsWorkingEstimate))
                }).ToList();
                if (projects.Any(i => !i.HasWorkingEstimate))
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "A working estimate for each project is required to price the proposal." });
                }
            }
            else if (projects.Any(i => !i.HasCustody) || projects.Any(i => !i.HasWorkingEstimate) )
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "You must have custody and a working estimate for each project to price the proposal." });
            }

            if (currentDqeUser.Role != DqeRole.Administrator && currentDqeUser.Role != DqeRole.AdminReadOnly)
            {
                if (!proposal.Projects.All(i => currentDqeUser.IsInDqeDistrict(i.District)) && !proposal.Projects.All(currentDqeUser.IsAuthorizedOnProject))
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "You are not authorized to price the proposal." });
                }    
            }

            var l = BuildProposalItemGroups(proposal);
            var result = new
            {
                viewOnly = (currentDqeUser.Role == DqeRole.Administrator && projects.Any(i => !i.HasCustody)) || currentDqeUser.Role == DqeRole.AdminReadOnly ,
                canEstimate = projects.Any(i => !i.HasCustody) ? false : true,
                isSystemSync = true,
                estimateId = proposal.Id,
                isOfficial = proposal.GetCurrentSnapshotLabel() == SnapshotLabel.Official,
                proposal = new
                {
                    id = proposal.Id,
                    number = proposal.ProposalNumber,
                    description = proposal.Description,
                    county = proposal.County.Name,
                    confidentialData = wtp?.OfficialEstimate == "Y" && wtp?.ProposalStatus != "03" //data is confidential if official estimate but not executed yet.
                },
                project = new
                {
                    id = 0
                },
                itemGroups = l,
                total = new EstimateTotal()
            };
            return new DqeResult(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ThrowSiteManagerExtendedAmountError()
        {
            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "The extended amount is too large on an item.  The maximum allowed is $99,999,999.99" });
        }

        [HttpPost]
        public ActionResult SaveProposalEstimate(dynamic estimate)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var proposal = _proposalRepository.GetById(currentDqeUser.MyRecentProposal.Id);
            if (proposal == null)
            {
                return new DqeResult(null, JsonRequestBehavior.AllowGet);
            }
            var l = new List<ProjectItem>();
            foreach (var g in proposal.SectionGroups)
            {
                foreach (var pi in g.ProposalItems)
                {
                    l.AddRange(pi.ProjectItems);
                }
            }
            foreach (var itemGroup in estimate.itemGroups)
            {
                var pi = l.SingleOrDefault(i => i.Id == (int)itemGroup.itemId);
                if (pi == null) continue;
                var pit = pi.GetTransformer();
                pit.Price = (decimal)itemGroup.price;
                if (pit.Price == 0)
                {
                    pit.PriceSet = PriceSetType.NotSet;
                }
                else
                {
                    pit.PriceSet = itemGroup.priceType == "O"
                        ? PriceSetType.EstimatorOverride
                        : itemGroup.priceType == "S"
                            ? PriceSetType.Statewide
                            : itemGroup.priceType == "M"
                                ? PriceSetType.MarketArea
                                : itemGroup.priceType == "C"
                                    ? PriceSetType.County
                                    : itemGroup.priceType == "R"
                                        ? PriceSetType.Reference
                                        : itemGroup.priceType == "F"
                                            ? PriceSetType.Fixed
                                            : itemGroup.priceType == "X"
                                                ? PriceSetType.SystemOverride
                                                : itemGroup.priceType == "T"
                                                    ? PriceSetType.Template
                                                    : PriceSetType.Parameter;
                }
                pi.Transform(pit, currentDqeUser);
            }
            //push prices means transfer prices to PrP
            if (estimate.pushPrices)
            {
                var pricesPushed = false;
                var nextSnapshot = proposal.GetNextSnapshotLabel();
                if (nextSnapshot != SnapshotLabel.Official && proposal.GetCurrentSnapshotLabel() != SnapshotLabel.Official)
                {
                    var isSynced = false;
                    foreach (var p in proposal.Projects)
                    {
                        isSynced = IsProjectSynced(p.Id);
                        if (!isSynced) break;
                    }
                    if (isSynced)
                    {
                        var result = _webTransportService.UpdatePrices(proposal, false, currentDqeUser);
                        if (!string.IsNullOrWhiteSpace(result))
                        {
                            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = result }, JsonRequestBehavior.AllowGet);
                        }
                        pricesPushed = true;
                    }
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = string.Format("Estimate saved - {0}", pricesPushed ? "Project Preconstruction prices updated" : "Project Preconstruction prices not updated because a project is not synchronized") }, JsonRequestBehavior.AllowGet);
                }
                // Stop pushing prices for non-bid items
                //if (nextSnapshot == SnapshotLabel.Official)
                //{
                //    var isSynced = false;
                //    foreach (var p in proposal.Projects)
                //    {
                //        isSynced = IsProjectSynced(p.Id);
                //        if (!isSynced) break;
                //    }
                //    if (isSynced)
                //    {
                //        var result = _webTransportService.UpdateFixedPrices(proposal, currentDqeUser);
                //        if (!string.IsNullOrWhiteSpace(result))
                //        {
                //            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = result }, JsonRequestBehavior.AllowGet);
                //        }
                //        pricesPushed = true;
                //    }
                //    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = string.Format("Estimate saved - {0}", pricesPushed ? "Project Preconstruction fixed price items updated" : "Project Preconstruction fixed price items not updated because a project is not synchronized") }, JsonRequestBehavior.AllowGet);
                //}
            }
            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = string.Format("Estimate Saved") }, JsonRequestBehavior.AllowGet);
        }

        private bool IsProjectSynced(long projectId)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            if (currentDqeUser.MyRecentProjectEstimate == null) return false;
            var p = _projectRepository.Get(projectId);
            if (p == null) return false;
            var vs = p.ProjectVersions.Where(i => i.VersionOwner == currentDqeUser).Distinct().ToList();
            if (vs.Count == 0) return false;
            var v = vs.FirstOrDefault(i => i.ProjectEstimates.Any(ii => ii.IsWorkingEstimate));
            if (v == null) return false;
            var est = v.ProjectEstimates.FirstOrDefault(i => i.IsWorkingEstimate);
            if (est == null) return false;
            return est.IsSyncedWithWt();
        }

        [HttpPost]
        public ActionResult SaveEstimate(dynamic estimate)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            if (currentDqeUser.MyRecentProjectEstimate == null)
            {
                return new DqeResult(null, JsonRequestBehavior.AllowGet);
            }
            var project = currentDqeUser.MyRecentProjectEstimate.MyProjectVersion.MyProject;
            if (project.CustodyOwner != currentDqeUser)
            {
                return new DqeResult(null, JsonRequestBehavior.AllowGet);
            }
            var l = new List<ProjectItem>();
            foreach (var estimateGroup in currentDqeUser.MyRecentProjectEstimate.EstimateGroups)
            {
                l.AddRange(estimateGroup.ProjectItems);
            }
            foreach (var itemGroup in estimate.itemGroups)
            {
                var pi = l.SingleOrDefault(i => i.Id == (int)itemGroup.itemId);
                if (pi == null) continue;
                var pit = pi.GetTransformer();
                pit.Price = (decimal)itemGroup.price;
                if (pit.Price == 0)
                {
                    pit.PriceSet = PriceSetType.NotSet;
                }
                else
                {
                    pit.PriceSet = itemGroup.priceType == "O"
                        ? PriceSetType.EstimatorOverride
                        : itemGroup.priceType == "S"
                            ? PriceSetType.Statewide
                            : itemGroup.priceType == "M"
                                ? PriceSetType.MarketArea
                                : itemGroup.priceType == "C"
                                    ? PriceSetType.County
                                    : itemGroup.priceType == "R"
                                        ? PriceSetType.Reference
                                        : itemGroup.priceType == "F"
                                            ? PriceSetType.Fixed
                                            : itemGroup.priceType == "X"
                                                ? PriceSetType.SystemOverride
                                                : itemGroup.priceType == "T"
                                                    ? PriceSetType.Template
                                                    : PriceSetType.Parameter;
                }
                pi.Transform(pit, currentDqeUser);
            }         
            if (estimate.pushPrices)
            {
                var proposal = project.Proposals.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);
                if (proposal != null)
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = string.Format("Estimate Saved - Project Preconstruction prices not updated because the project is associated to a proposal.  Estimate at the proposal level to update Project Preconstruction prices.") }, JsonRequestBehavior.AllowGet);
                }
                var pricesPushed = false;
                //var pushAllPrices = proposal == null;
                //var pushFixedPrices = false;
                //if (!pushAllPrices)
                //{
                //    var nextSnapshot = proposal.GetNextSnapshotLabel();
                //    if (nextSnapshot != SnapshotLabel.Official && nextSnapshot != SnapshotLabel.Estimator)
                //    {
                //        pushAllPrices = true;
                //    }
                //    else if (nextSnapshot == SnapshotLabel.Estimator)
                //    {
                //        var projectSnapshot = project.GetCurrentSnapshotLabel();
                //        if (projectSnapshot == SnapshotLabel.Phase2 || projectSnapshot == SnapshotLabel.Phase3 || projectSnapshot == SnapshotLabel.Phase4)
                //        {
                //            pushAllPrices = true;
                //        }
                //        else if (project.GetNextSnapshotLabel() == SnapshotLabel.Phase2)
                //        {
                //            pushAllPrices = true;
                //        }
                //    }
                //    if (nextSnapshot == SnapshotLabel.Official)
                //    {
                //        pushFixedPrices = true;
                //    }
                //}
                //if (pushAllPrices)
                //{
                if (currentDqeUser.MyRecentProjectEstimate.IsSyncedWithWt())
                {
                    var result = _webTransportService.UpdateProjectPrices(currentDqeUser.MyRecentProjectEstimate, currentDqeUser, true);
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = result }, JsonRequestBehavior.AllowGet);
                    }
                    pricesPushed = true;
                }
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = string.Format("Estimate Saved - {0}", pricesPushed ? "Project Preconstruction prices updated" : "Project Preconstruction prices not updated because the project is not synchronized") }, JsonRequestBehavior.AllowGet);
                //}
                //if (pushFixedPrices)
                //{
                //    if (currentDqeUser.MyRecentProjectEstimate.IsSyncedWithWt())
                //    {
                //        var result = _webTransportService.UpdateFixedPrices(currentDqeUser.MyRecentProjectEstimate, currentDqeUser);
                //        if (!string.IsNullOrWhiteSpace(result))
                //        {
                //            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = result }, JsonRequestBehavior.AllowGet);
                //        }
                //        pricesPushed = true;
                //    }
                //    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = string.Format("Estimate Saved - {0}", pricesPushed ? "Project Preconstruction fixed price items updated" : "Project Preconstruction fixed price items not updated because the project is not synchronized") }, JsonRequestBehavior.AllowGet);
                //}
            }
            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = string.Format("Estimate Saved") }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GenerateParameterPrices(dynamic parms)
        {
            var itemGroup = parms.itemGroup;
            var contractType = parms.contractType;
            var workTypes = parms.workTypes;
            var marketAreas = parms.marketAreas;
            var countyName = parms.county;
            var county = _marketAreaRepository.GetAllCounties().FirstOrDefault(i => i.Name == countyName);
            var bidMonths = parms.bidMonths;

            //ALL = All
            //AWO = Awarded Only
            //BID = top #bids

            var bidFilter = parms.bidFilter;
            int numberOfBids;
            int.TryParse(parms.numberOfBids.ToString(), out numberOfBids);
            
            
            var includeEstimates = parms.includeEstimates;
            var item = (PayItemMaster)_payItemMasterRepository.GetWithHistory(Convert.ToString(itemGroup.itemNumber));
            if (item == null || !item.ProposalHistories.Any())
            {
                return new DqeResult(0);
            }
            var bhl = new List<BidHistory>();
            foreach (var ph in item.ProposalHistories.OrderByDescending(i => i.LettingDate))
            {
                var include = true;
                if (contractType != "all" && ph.ContractType.ToUpper().StartsWith("M")) continue;
                foreach (var workType in workTypes)
                {
                    if (ph.ContractWorkType != workType.code.ToString()) continue;
                    include = workType.include;
                    break;
                }
                if (!include) continue;
                var countyFound = false;
                foreach (var marketArea in marketAreas)
                {
                    foreach (var cty in marketArea.counties)
                    {
                        if (ph.County != cty.name.ToString()) continue;
                        include = cty.include;
                        countyFound = true;
                        break;
                    }
                    if (countyFound) break;
                }
                if (!include || !countyFound) continue;
                var d1 = ph.LettingDate.Date;
                var d2 = DateTime.Now.Date;
                include = (d2.Day >= d1.Day ? 0 : -1) + ((d2.Year - d1.Year) * 12) + (d2.Month - d1.Month) < bidMonths;
                if (!include) continue;
                if (includeEstimates)
                {
                    if (bidFilter == "AWO")
                    {
                        bhl.AddRange(ph.BidHistories.Where(i => i.IsAwarded || i.IsEstimate));
                    }
                    else if (bidFilter == "BID")
                    {
                        var ab = ph.BidHistories.FirstOrDefault(i => i.IsAwarded || i.IsEstimate);
                        if (ab != null)
                        {
                            bhl.Add(ab);
                        }
                        bhl.AddRange(ph.BidHistories.Where(i => !i.IsAwarded && !i.IsEstimate).OrderBy(i => i.BidTotal).Skip(0).Take(Math.Min(ab == null ? numberOfBids : numberOfBids - 1, ph.BidHistories.Count())));
                    }
                    else
                    {
                        bhl.AddRange(ph.BidHistories);    
                    }
                }
                else
                {
                    if (ph.BidHistories.Any(i => i.IsEstimate)) continue;
                    if (bidFilter == "AWO")
                    {
                        bhl.AddRange(ph.BidHistories.Where(i => i.IsAwarded));
                    }
                    else if (bidFilter == "BID")
                    {
                        bhl.AddRange(ph.BidHistories.OrderBy(i => i.IsAwarded).ThenBy(i => i.BidTotal).Skip(0).Take(Math.Min(numberOfBids, ph.BidHistories.Count())));
                    }
                    else
                    {
                        bhl.AddRange(ph.BidHistories);
                    }
                }
            }
            var bs = new BidSet();
            var bl = bhl.Select(i => new Bid
            {
                Id = i.Id,
                LettingDate = i.MyProposalHistory.LettingDate,
                IsAwarded = i.IsAwarded,
                IsLowCost = i.IsLowCost,
                IsEstimate = i.IsEstimate,
                IsBlank = false,
                Price = i.Price,
                BidTotal = i.BidTotal,
                Quantity = i.MyProposalHistory.Quantity,
                County = i.MyProposalHistory.County
            }).ToList();
            bs.Bids = bl;
            bs = _pricingEngine.CalculateAveragePrice(bs, true, Convert.ToDecimal(itemGroup.quantity), county);
            return new DqeResult((bool)parms.useStraightAverage ? bs.UnweightedAveragePrice : bs.WeightedAveragePrice);
        }

        [HttpPost]
        public ActionResult UpdateBidHistory(dynamic itemToPrice)
        {
            var itemGroup = itemToPrice.itemGroup;
            var countyName = itemToPrice.county;
            County county = null;
            if (!string.IsNullOrWhiteSpace(countyName))
            {
                county = _marketAreaRepository.GetAllCounties().FirstOrDefault(i => i.Name == countyName);
            }
            var item = (PayItemMaster)_payItemMasterRepository.GetWithHistory(Convert.ToString(itemGroup.itemNumber));
            if (item == null || !item.ProposalHistories.Any())
            {
                return new DqeResult(null);
            }
            //var bhl = new List<Domain.Model.BidHistory>();
            //foreach (var ph in item.ProposalHistories)
            //{
            //    bhl.AddRange(ph.BidHistories);
            //}
            var bs = new BidSet();
            var bl = new List<Bid>();
            foreach (var proposal in itemGroup.history.proposals)
            {
                foreach (var bid in proposal.bids)
                {
                    if (!Convert.ToBoolean(bid.include) || Convert.ToBoolean(bid.blank)) continue;
                    bl.Add(new Bid
                    {
                        County = proposal.county,
                        Id = bid.id,
                        IsAwarded = bid.awarded,
                        IsEstimate = bid.estimate,
                        BidTotal = bid.bidTotal,
                        Included = true,
                        IsBlank = false,
                        IsLowCost = bid.lowCost,
                        LettingDate = DateTime.Parse(Convert.ToString(proposal.letting)),
                        Price = Convert.ToDecimal(bid.price),
                        Quantity = Convert.ToDecimal(proposal.quantity),
                    });
                }
            }

            //var count = bl.Count();

            bs.Bids = bl;
            if (!bs.Bids.Any())
            {
                return new DqeResult(null);
            }
            bs = _pricingEngine.CalculateAveragePrice(bs, (bool)itemGroup.history.omitOutliers, Convert.ToDecimal(itemGroup.quantity), county);
            return FormatBidHistory(bs, item, (bool)itemGroup.history.useStraightAverage);
        }

        [HttpPost]
        [OverrideAuthorization]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.AdminReadOnly, DqeRole.DistrictAdministrator, DqeRole.Estimator, DqeRole.Coder, DqeRole.DistrictReviewer, DqeRole.StateReviewer })]
        public ActionResult GetBidHistory(dynamic itemToPrice)
        {
            var itemGroup = itemToPrice.itemGroup;
            var countyName = itemToPrice.county;
            County county = null;
            if (!string.IsNullOrWhiteSpace(countyName))
            {
                county = _marketAreaRepository.GetAllCounties().FirstOrDefault(i => i.Name == countyName);    
            }
            var item = (PayItemMaster)_payItemMasterRepository.GetWithHistory(Convert.ToString(itemGroup.itemNumber));
            if (item == null || !item.ProposalHistories.Any())
            {
                return new DqeResult(null);
            }
            var bhl = new List<BidHistory>();
            foreach (var ph in item.ProposalHistories)
            {
                bhl.AddRange(ph.BidHistories);
            }
            var bs = new BidSet();
            var bl = bhl.Where(i => !i.IsEstimate).Select(i => new Bid
            {
                Id = i.Id,
                LettingDate = i.MyProposalHistory.LettingDate,
                IsAwarded = i.IsAwarded,
                IsLowCost = i.IsLowCost,
                IsEstimate = i.IsEstimate,
                IsBlank = false,
                Price = i.Price,
                BidTotal = i.BidTotal,
                Quantity = i.MyProposalHistory.Quantity,
                County = i.MyProposalHistory.County
            });
            bs.Bids = bl;
            bs = _pricingEngine.CalculateAveragePrice(bs, true, Convert.ToDecimal(itemGroup.quantity), county);
            return FormatBidHistory(bs, item, false);
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

        private ActionResult FormatBidHistory(BidSet bs, PayItemMaster item, bool useStraightAverage)
        {
            var included = bs.Bids == null 
                ? new List<Bid>()
                : bs.Bids.Where(i => i.Included).ToList();
            var maxBidders = item.ProposalHistories.Max(i => i.BidHistories.Count());
            var proposals = new List<dynamic>();
            var shift =
                item.ProposalHistories.Where(i => i.BidHistories.Count() == maxBidders)
                    .Any(i => !i.BidHistories.Any(ii => ii.IsAwarded))
                    ? 1
                    : 0;
            foreach (var p in item.ProposalHistories.OrderByDescending(i => i.LettingDate))
            {
                var blankCount = maxBidders - p.BidHistories.Count();
                if (p.BidHistories.All(i => !i.IsAwarded)) blankCount -= 1;
                blankCount = blankCount + shift;
                var blanks = new List<object>();
                for (var i = 0; i < blankCount; i++)
                {
                    blanks.Add(new
                    {
                        id = 0,
                        blank = true,
                        price = (decimal)(dynamic)0,
                        include = false,
                        awarded = false,
                        estimate = false,
                        bidTotal = (decimal)(dynamic) 0,
                        lowCost = false
                    });
                }
                var awardedBid = p.BidHistories.FirstOrDefault(i => i.IsAwarded);
                var aBid = (awardedBid == null)
                    ? new
                    {
                        id = (long)0,
                        blank = true,
                        price = (decimal) (dynamic) 0,
                        include = false,
                        awarded = true,
                        estimate = false,
                        bidTotal = (decimal)(dynamic) 0,
                        lowCost = false
                    }
                    : new
                    {
                        id = awardedBid.Id,
                        blank = false,
                        price = awardedBid.Price,
                        include = included.FirstOrDefault(ii => ii.Id == awardedBid.Id) != null,
                        awarded = true,
                        estimate = awardedBid.IsEstimate,
                        bidTotal = awardedBid.BidTotal,
                        lowCost = awardedBid.IsLowCost
                    };
                var bids = p.BidHistories.Where(i => !i.IsAwarded).OrderBy(i => i.BidTotal).Select(i => new
                {
                    id = i.Id,
                    blank = false,
                    price = i.Price,
                    include = included.FirstOrDefault(ii => ii.Id == i.Id) != null,
                    awarded = false,
                    estimate = i.IsEstimate,
                    bidTotal = i.BidTotal,
                    lowCost = i.IsLowCost
                }).Concat(blanks).ToList();
                bids.Insert(0, aBid);
                Bid t = null;
                foreach (var bh in p.BidHistories)
                {
                    t = included.FirstOrDefault(i => i.Id == bh.Id);
                    if (t != null) break;
                }
                var locationWeight = t == null ? 9999999 : t.LocationWeight;
                var quantityWeight = t == null ? 9999999 : t.QuantityWeight;
                var timeWeight = t == null ? 9999999 : t.TimeWeight;
                proposals.Add(new
                {
                    id = p.Id,
                    proposal = p.ProposalNumber,
                    include = p.BidHistories.Any(i => included.Count(ii => ii.Id == i.Id) > 0),
                    letting = p.LettingDate.ToShortDateString(),
                    lettingAsDate = p.LettingDate.Date.ToString("yyyy-MM-dd"),
                    county = p.County,
                    quantity = p.Quantity,
                    contractType = p.ContractType,
                    workType = p.ContractWorkType,
                    duration = p.Duration,
                    locationWeight,
                    quantityWeight,
                    timeWeight,
                    estimate = p.EstimateAmount.HasValue ? p.EstimateAmount.Value : 0,
                    bids
                });
            }
            var c = 0;
            var history = new
            {
                maxBiddersProposal = maxBidders == 0
                    ? null
                    : new
                    {
                        bids = new int[maxBidders + shift].Select(ii => new
                        {
                            number = c += 1
                        })
                    },
                proposals,
                average = useStraightAverage ? bs.UnweightedAveragePrice : bs.WeightedAveragePrice
            };
            return new DqeResult(history);
        }
    }
}