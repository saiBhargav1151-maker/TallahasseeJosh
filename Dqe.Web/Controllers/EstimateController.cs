using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Office.CustomUI;
using Dqe.ApplicationServices;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;
using Dqe.Domain.Model.Reports;
using Dqe.Domain.Repositories.Custom;
using Dqe.Web.ActionResults;
using Dqe.Web.Attributes;

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    [CustomAuthorize(Roles = new[] {DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator})]
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
        
        
        public EstimateController
            (
            IDqeUserRepository dqeUserRepository,
            IProjectRepository projectRepository,
            IWebTransportService webTransportService,
            IPayItemMasterRepository payItemMasterRepository,
            IPricingEngine pricingEngine,
            IMarketAreaRepository marketAreaRepository,
            IProposalRepository proposalRepository,
            ITransactionManager transactionManager
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
            return CreateProposalEstimateStructure(loadId.loadId);
        }

        //[HttpPost]
        //public ActionResult TestReport()
        //{
        //    var rpt = _proposalRepository.GetReportProposal("T1299", ReportProposalLevel.Authorization, _payItemMasterRepository);
        //    return new DqeResult(rpt.Total);
        //}

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
            var proposal = _proposalRepository.GetById(estimateId.estimateId);
            proposal.SetCurrentEstimator(currentDqeUser);
            var wtp = _webTransportService.GetProposal(proposal.ProposalNumber);
            proposal.SynchronizeStructure(wtp, currentDqeUser);
            return new DqeResult(null);
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
                var average = pis.Count == 0 ? 0 : pis.Sum(i => i.Price) / pis.Count();
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
                                                : "P";
                key += 1;
                var payItem = payItems.FirstOrDefault(i => i.RefItemName == itemGroup.ItemNumber);
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
                        ? payItem.StateReferencePrice.Value
                        : 0;
                var marketAreaPrice = marketAreaAverage == null 
                    ? 0 
                    : marketAreaAverage.Price;
                var countyPrice = countyAverage == null 
                    ? 0 
                    : countyAverage.Price;
                //var isFixedPrice = payItem != null && (!string.IsNullOrWhiteSpace(payItem.BidRequirementCode) && (payItem.BidRequirementCode.ToUpper().Trim() == "FIXED"));
                //var isFixedPrice = payItem != null && payItem.NonBid;
                var isFixedPrice = payItem != null
                    && (!string.IsNullOrWhiteSpace(payItem.BidRequirementCode)
                    && (payItem.BidRequirementCode.ToUpper().Trim() == "FIXED"))
                    && payItem.RefPrice.HasValue
                    && payItem.RefPrice.Value > 0;
                var referencePrice = payItem == null 
                    ? 0 
                    : payItem.RefPrice.HasValue ? payItem.RefPrice.Value : 0;
                var fixedPrice = isFixedPrice ? payItem.RefPrice.HasValue ? payItem.RefPrice.Value : 0 : 0;
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
                    parameterPrice = (decimal)0, 
                    referencePrice,
                    fixedPrice,
                    priceType = isFixedPrice ? "F" : priceType,
                    holdPriceType = isFixedPrice ? "F" : priceType,
                    isSystemOverride = false
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
                            priceType = isFixedPrice ? "F" : priceType,
                            holdPriceType = isFixedPrice ? "F" : priceType,
                            isSystemOverride = item.Price != average
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
            if (project.CustodyOwner != currentDqeUser)
            {
                return new DqeResult(null, new ClientMessage{ Severity = ClientMessageSeverity.Error, text = "You must have custody to price the project."});
            }
            if (currentDqeUser.Role != DqeRole.Administrator)
            {
                if (!currentDqeUser.IsInDqeDistrict(project.District) && !currentDqeUser.IsAuthorizedOnProject(project))
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "You are not authorized to price the project." });
                }    
            }
            var wtProposal = project.Proposals.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);
            var l = BuildProjectItemGroups(estimate);
            var result = new
            {
                canEstimate = true,
                isSystemSync = true,
                estimateId = estimate.Id,
                proposal = new
                {
                    id = 0
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
                var average = pis.Count == 0 ? 0 : Math.Round(pis.Sum(i => i.Price) / pis.Count(), 2);
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
                                                        : "P";
                key += 1;
                var payItem = payItems.FirstOrDefault(i => i.RefItemName == itemGroup.ItemNumber);
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
                        ? payItem.StateReferencePrice.Value
                        : 0;
                var marketAreaPrice = marketAreaAverage == null
                    ? 0
                    : marketAreaAverage.Price;
                var countyPrice = countyAverage == null
                    ? 0
                    : countyAverage.Price;
                //only use the non-bid 
                var isFixedPrice = payItem != null 
                    && (!string.IsNullOrWhiteSpace(payItem.BidRequirementCode) 
                    && (payItem.BidRequirementCode.ToUpper().Trim() == "FIXED"))
                    && payItem.RefPrice.HasValue 
                    && payItem.RefPrice.Value > 0;
                var referencePrice = payItem == null
                    ? 0
                    : payItem.RefPrice.HasValue ? payItem.RefPrice.Value : 0;
                var fixedPrice = isFixedPrice ? payItem.RefPrice.HasValue ? payItem.RefPrice.Value : 0 : 0;
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
                    parameterPrice = (decimal)0,
                    referencePrice,
                    fixedPrice,
                    projectNumber = projects.Aggregate(string.Empty, (current, project) => current + string.Format("{0} ", project.ProjectNumber)).TrimEnd(' '),
                    itemProjectNumber = projects.Count == 1 ? projects[0].ProjectNumber : "Multiple",
                    priceType = isFixedPrice ? "F" : priceType,
                    holdPriceType = isFixedPrice ? "F" : priceType,
                    isSystemOverride = false,
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
                            projectNumber =
                                projects.Aggregate(string.Empty,
                                    (current, project) => current + string.Format("{0} ", project.ProjectNumber))
                                    .TrimEnd(' '),
                            itemProjectNumber =
                                item.MyEstimateGroup.MyProjectEstimate.MyProjectVersion.MyProject.ProjectNumber,
                            priceType = isFixedPrice ? "F" : priceType,
                            holdPriceType = isFixedPrice ? "F" : priceType,
                            isSystemOverride = item.Price != average,
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
            if (!proposal.takeLabeledSnapshot) return new DqeResult(null);
            var p = (Proposal)_proposalRepository.GetById(proposal.id);
            var nextSnapshot = p.GetNextSnapshotLabel();
            if (nextSnapshot != SnapshotLabel.Authorization && nextSnapshot != SnapshotLabel.Official) return new DqeResult(null);
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);   
            _webTransportService.UpdatePrices(p, nextSnapshot == SnapshotLabel.Official, currentDqeUser);
            return new DqeResult(null);
        }

        private DqeResult CreateProposalEstimateStructure(int estimateId)
        {
            var proposal = _proposalRepository.GetById(estimateId);
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var itemsCount = proposal.SectionGroups.Sum(i => i.ProposalItems.Count());

            if (proposal.CurrentEstimator != currentDqeUser || itemsCount == 0)
            {
                proposal.SetCurrentEstimator(currentDqeUser);
                var wtp = _webTransportService.GetProposal(proposal.ProposalNumber);
                proposal.SynchronizeStructure(wtp, currentDqeUser);
            }

            var projects = proposal.Projects.OrderBy(i => i.ProjectNumber).Select(i => new
            {
                HasCustody = i.CustodyOwner == currentDqeUser,
                HasWorkingEstimate = i.ProjectHasWorkingEstimateForUser(currentDqeUser)
            }).ToList();
            if (projects.Any(i => !i.HasCustody) || projects.Any(i => !i.HasWorkingEstimate))
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "You must have custody and a working estimate for each project to price the proposal." });
            }

            if (currentDqeUser.Role != DqeRole.Administrator)
            {
                if (!proposal.Projects.All(i => currentDqeUser.IsInDqeDistrict(i.District)) && !proposal.Projects.All(currentDqeUser.IsAuthorizedOnProject))
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "You are not authorized to price the proposal." });
                }    
            }

            var l = BuildProposalItemGroups(proposal);
            var result = new
            {
                canEstimate = true,
                isSystemSync = true,
                estimateId = proposal.Id,
                proposal = new
                {
                    id = proposal.Id,
                    number = proposal.ProposalNumber,
                    description = proposal.Description,
                    county = proposal.County.Name
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
                                                : PriceSetType.Parameter;
                }
                pi.Transform(pit, currentDqeUser);
            }
            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = "Estimate saved" }, JsonRequestBehavior.AllowGet);
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
                                                : PriceSetType.Parameter;
                }
                pi.Transform(pit, currentDqeUser);
            }
            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = "Estimate saved" }, JsonRequestBehavior.AllowGet);
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
            var item = (PayItemMaster)_payItemMasterRepository.GetWithHistory(Convert.ToString(itemGroup.itemNumber));
            if (item == null || !item.ProposalHistories.Any())
            {
                return new DqeResult(null);
            }
            var bhl = new List<Domain.Model.BidHistory>();
            foreach (var ph in item.ProposalHistories)
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
                if (!include) continue; 
                bhl.AddRange(ph.BidHistories);
            }
            var bs = new BidSet();
            var bl = bhl.Select(i => new Bid
            {
                Id = i.Id,
                LettingDate = i.MyProposalHistory.LettingDate,
                IsAwarded = i.IsAwarded,
                IsLowCost = i.IsLowCost,
                IsBlank = false,
                Price = i.Price,
                BidTotal = i.BidTotal,
                Quantity = i.MyProposalHistory.Quantity,
                County = i.MyProposalHistory.County
            });
            bs.Bids = bl;
            bs = _pricingEngine.CalculateAveragePrice(bs, true, Convert.ToDecimal(itemGroup.quantity), county);
            return new DqeResult(bs.WeightedAveragePrice);
        }

        [HttpPost]
        public ActionResult UpdateBidHistory(dynamic itemToPrice)
        {
            var itemGroup = itemToPrice.itemGroup;
            var countyName = itemToPrice.county;
            var county = _marketAreaRepository.GetAllCounties().FirstOrDefault(i => i.Name == countyName);
            var item = (PayItemMaster)_payItemMasterRepository.GetWithHistory(Convert.ToString(itemGroup.itemNumber));
            if (item == null || !item.ProposalHistories.Any())
            {
                return new DqeResult(null);
            }
            var bhl = new List<Domain.Model.BidHistory>();
            foreach (var ph in item.ProposalHistories)
            {
                bhl.AddRange(ph.BidHistories);
            }
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
            bs.Bids = bl;
            if (!bs.Bids.Any())
            {
                return new DqeResult(null);
            }
            bs = _pricingEngine.CalculateAveragePrice(bs, (bool)itemGroup.history.omitOutliers, Convert.ToDecimal(itemGroup.quantity), county);
            return FormatBidHistory(bs, item);
        }

        [HttpPost]
        public ActionResult GetBidHistory(dynamic itemToPrice)
        {
            var itemGroup = itemToPrice.itemGroup;
            var countyName = itemToPrice.county;
            var county = _marketAreaRepository.GetAllCounties().FirstOrDefault(i => i.Name == countyName);
            var item = (PayItemMaster)_payItemMasterRepository.GetWithHistory(Convert.ToString(itemGroup.itemNumber));
            if (item == null || !item.ProposalHistories.Any())
            {
                return new DqeResult(null);
            }
            var bhl = new List<Domain.Model.BidHistory>();
            foreach (var ph in item.ProposalHistories)
            {
                bhl.AddRange(ph.BidHistories);
            }
            var bs = new BidSet();
            var bl = bhl.Select(i => new Bid
            {
                Id = i.Id,
                LettingDate = i.MyProposalHistory.LettingDate,
                IsAwarded = i.IsAwarded,
                IsLowCost = i.IsLowCost,
                IsBlank = false,
                Price = i.Price,
                BidTotal = i.BidTotal,
                Quantity = i.MyProposalHistory.Quantity,
                County = i.MyProposalHistory.County
            });
            bs.Bids = bl;
            bs = _pricingEngine.CalculateAveragePrice(bs, true, Convert.ToDecimal(itemGroup.quantity), county);
            return FormatBidHistory(bs, item);
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

        private ActionResult FormatBidHistory(BidSet bs, PayItemMaster item)
        {
            var included = bs.Bids.Where(i => i.Included).ToList();
            var maxBidders = item.ProposalHistories.Max(i => i.BidHistories.Count());
            var proposals = new List<dynamic>();
            var shift =
                item.ProposalHistories.Where(i => i.BidHistories.Count() == maxBidders)
                    .Any(i => !i.BidHistories.Any(ii => ii.IsAwarded))
                    ? 1
                    : 0;
            foreach (var p in item.ProposalHistories)
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
                    county = p.County,
                    quantity = p.Quantity,
                    contractType = p.ContractType,
                    workType = p.ContractWorkType,
                    duration = p.Duration,
                    locationWeight,
                    quantityWeight,
                    timeWeight,
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
                average = bs.WeightedAveragePrice
            };
            return new DqeResult(history);
        }
    }
}