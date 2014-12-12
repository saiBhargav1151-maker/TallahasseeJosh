using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dqe.ApplicationServices;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;
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
        private readonly IWebTransportService _webTransportService;
        private readonly IPayItemMasterRepository _payItemMasterRepository;
        
        public EstimateController
            (
            IDqeUserRepository dqeUserRepository,
            IProjectRepository projectRepository,
            IWebTransportService webTransportService,
            IPayItemMasterRepository payItemMasterRepository
            )
        {
            _dqeUserRepository = dqeUserRepository;
            _projectRepository = projectRepository;
            _webTransportService = webTransportService;
            _payItemMasterRepository = payItemMasterRepository;
        }

        [HttpGet]
        public ActionResult LoadProjectEstimate()
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            if (!currentDqeUser.CanEstimateRecentProject())
            {
                return new DqeResult(new { canEstimate = false }, JsonRequestBehavior.AllowGet);
            }
            
            //show project estimate
            return CreateProjectEstimateStructure(currentDqeUser);
        }

        [HttpGet]
        public ActionResult LoadProjectEstimateSummary()
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            //show project estimate summary
            var estimate = currentDqeUser.MyRecentProjectEstimate;
            return new DqeResult(new
            {
                total = estimate.GetEstimateTotal()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult LoadProposalEstimateSummary()
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            //show project estimate summary
            var proposal = currentDqeUser.MyRecentProposal;
            //GetCandidateTotals(proposal);
            if (currentDqeUser.CanTotalProposal(proposal))
            {
                return new DqeResult(new { total = proposal.GetEstimateTotal(currentDqeUser) }, JsonRequestBehavior.AllowGet);    
            }
            return new DqeResult(new { total = new EstimateTotal() }, JsonRequestBehavior.AllowGet);
        }

        //private void GetCandidateTotals(Proposal proposal)
        //{
        //    var l = new List<IEnumerable<DqeUser>>();
        //    foreach (var project in proposal.Projects)
        //    {
        //        l.Add(project.ProjectVersions.Select(i => i.VersionOwner).Distinct().ToList());
        //    }
        //    IEnumerable<DqeUser> candidates = null;
        //    foreach (var list in l)
        //    {
        //        candidates = candidates == null ? list : candidates.Intersect(list);
        //    }
        //    if (candidates == null) return;
        //    foreach (var candidate in candidates)
        //    {
        //        if (candidate.CanTotalProposal(proposal))
        //        {
                    
        //        }
        //    }
        //}

        [HttpGet]
        public ActionResult LoadProposalEstimate()
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            if (!currentDqeUser.CanEstimateRecentProposal())
            {
                return new DqeResult(new { canEstimate = false }, JsonRequestBehavior.AllowGet);
            }
            //show proposal estimate
            return CreateProposalEstimateStructure(currentDqeUser);
        }

        [HttpGet]
        public ActionResult LoadProject()
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            if (currentDqeUser.CanEstimateRecentProject())
            {
                //show project estimate
                return CreateProjectEstimateStructure(currentDqeUser);
            }
            return new DqeResult(null, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult LoadProposal()
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            if (currentDqeUser.CanEstimateRecentProposal())
            {
                //show proposal estimate
                return CreateProposalEstimateStructure(currentDqeUser);
            }
            return new DqeResult(null, JsonRequestBehavior.AllowGet);
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
                        price = item.Price
                    });
                }
            }
            return l;
        }

        private IList<object> BuildProjectItemGroups(ProjectEstimate estimate)
        {
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
            //.ThenBy(i => i.Fund);
            var l = new List<object>();
            var key = 0;
            foreach (var itemGroup in igl)
            {
                //sync mismatched prices
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
                                        : "P";
                key += 1;
                var statewidePrice = _payItemMasterRepository.GetStatePriceForItem(itemGroup.ItemNumber);
                var marketAreaPrice = _payItemMasterRepository.GetMarketPriceForItem(itemGroup.ItemNumber, estimate.MyProjectVersion.MyProject.MyCounty.Name);
                var countyPrice = _payItemMasterRepository.GetCountyPriceForItem(itemGroup.ItemNumber, estimate.MyProjectVersion.MyProject.MyCounty.Name);
                l.Add(new
                {
                    isFirst = true,
                    rowSpan = 1,
                    key,
                    itemId = itemGroup.ProjectItems.Count == 1 ? itemGroup.ProjectItems[0].Id : 0,
                    itemIds = itemGroup.ProjectItems.Select(i => i.Id),
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
                    price = average,
                    holdPrice = average,
                    statewidePrice = statewidePrice.HasValue ? statewidePrice.Value : 0, 
                    marketAreaPrice, 
                    countyPrice,
                    parameterPrice = 0, 
                    priceType,
                    holdPriceType = priceType,
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
                            price = average,
                            holdPrice = average,
                            statewidePrice = 0,
                            marketAreaPrice = 0,
                            countyPrice = 0,
                            parameterPrice = 0,
                            priceType,
                            holdPriceType = priceType,
                            isSystemOverride = item.Price != average
                        });
                        isFirst = false;
                    }
                }
            }
            return l;
        }

        private DqeResult CreateProjectEstimateStructure(DqeUser currentDqeUser)
        {
            var estimate = currentDqeUser.MyRecentProjectEstimate;
            var project = estimate.MyProjectVersion.MyProject;
            var wtProposal = project.Proposals.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);
            var l = BuildProjectItemGroups(estimate);
            var result = new
            {
                canEstimate = true,
                isSystemSync = true,
                proposal = new
                {
                    id = 0
                },
                project = new
                {
                    id = project.Id,
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
            SaveEstimate(result);
            l = BuildProjectItemGroups(estimate);
            result = new
            {
                canEstimate = true,
                isSystemSync = true,
                proposal = new
                {
                    id = 0
                },
                project = new
                {
                    id = project.Id,
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
                total = estimate.GetEstimateTotal()
            };
            return new DqeResult(result, JsonRequestBehavior.AllowGet);
        }

        private IList<object> BuildProposalItemGroups(Proposal proposal)
        {
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
                //sync mismatched prices
                var pis = itemGroup.ProjectItems.Where(i => i.Price > 0).ToList();
                var average = pis.Count == 0 ? 0 : pis.Sum(i => i.Price) / pis.Count();
                var projects = itemGroup.ProjectItems.Select(i => i.MyEstimateGroup.MyProjectEstimate.MyProjectVersion.MyProject).Distinct().ToList();
                key += 1;
                l.Add(new
                {
                    isFirst = true,
                    rowSpan = 1,
                    key,
                    itemId = itemGroup.ProjectItems.Count == 1 ? itemGroup.ProjectItems[0].Id : 0,
                    itemIds = itemGroup.ProjectItems.Select(i => i.Id),
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
                    price = average,
                    holdPrice = average,
                    projectNumber = projects.Aggregate(string.Empty, (current, project) => current + string.Format("{0} ", project.ProjectNumber)).TrimEnd(' '),
                    itemProjectNumber = projects.Count == 1 ? projects[0].ProjectNumber : "Multiple",
                    priceType = itemGroup.ProjectItems.Any(i => i.PriceSet == PriceSetType.SystemOverride)
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
                                                : "P",
                    //priceType = itemGroup.ProjectItems.First().PriceSet == PriceSetType.NotSet ? "N" : itemGroup.ProjectItems.First().PriceSet == PriceSetType.EstimatorOverride ? "O" : "X",
                    isSystemOverride = false,
                });
                if (itemGroup.ProjectItems.Count > 1)
                {
                    var isFirst = true;
                    foreach (var item in itemGroup.ProjectItems)
                    {
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
                            fund = itemGroup.Fund,
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
                            price = average,
                            holdPrice = average,
                            projectNumber = projects.Aggregate(string.Empty, (current, project) => current + string.Format("{0} ", project.ProjectNumber)).TrimEnd(' '),
                            itemProjectNumber = item.MyEstimateGroup.MyProjectEstimate.MyProjectVersion.MyProject.ProjectNumber,
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
                                                    : "X",

                            isSystemOverride = item.Price != average,
                        });
                        isFirst = false;
                    }
                }
            }
            return l;
        }

        private DqeResult CreateProposalEstimateStructure(DqeUser currentDqeUser)
        {
            var proposal = currentDqeUser.MyRecentProposal;
            var wtp = _webTransportService.GetProposal(proposal.ProposalNumber);
            proposal.SynchronizeStructure(wtp, currentDqeUser);
            var l = BuildProposalItemGroups(proposal);
            var result = new
            {
                canEstimate = true,
                isSystemSync = true,
                proposal = new
                {
                    id = proposal.Id,
                    number = proposal.ProposalNumber,
                    description = proposal.Description
                },
                project = new
                {
                    id = 0
                },
                itemGroups = l,
                total = new EstimateTotal()
            };
            SaveProposalEstimate(result);
            l = BuildProposalItemGroups(proposal);
            result = new
            {
                canEstimate = true,
                isSystemSync = true,
                proposal = new
                {
                    id = proposal.Id,
                    number = proposal.ProposalNumber,
                    description = proposal.Description
                },
                project = new
                {
                    id = 0
                },
                itemGroups = l,
                total = proposal.GetEstimateTotal(currentDqeUser)
            };
            return new DqeResult(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveProposalEstimate(dynamic estimate)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var proposal = currentDqeUser.MyRecentProposal;
            if (proposal == null)
            {
                return new DqeResult(null, JsonRequestBehavior.AllowGet);
            }
            if (!currentDqeUser.CanEstimateRecentProposal())
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
                else if (estimate.isSystemSync && itemGroup.isSystemOverride)
                {
                    pit.PriceSet = PriceSetType.SystemOverride;
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
                else if (estimate.isSystemSync && itemGroup.isSystemOverride)
                {
                    pit.PriceSet = PriceSetType.SystemOverride;
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
                                    : PriceSetType.Parameter;
                }
                pi.Transform(pit, currentDqeUser);
            }
            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = "Estimate saved" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AsyncSaveBidHistory(dynamic estimate)
        {
            return SaveBidHistory(estimate);
        }

        [HttpPost]
        public ActionResult SaveBidHistory(dynamic itemGroup)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            foreach (var id in itemGroup.itemIds)
            {
                var item = _projectRepository.GetProjectItem((int) id);
                var pit = item.GetTransformer();
                pit.Price = decimal.Parse(itemGroup.price.ToString());
                pit.PriceSet = pit.Price == 0 ? PriceSetType.NotSet : PriceSetType.Parameter;
                item.Transform(pit, currentDqeUser);
                item.ClearHistory();
                foreach (var proposal in itemGroup.history.proposals)
                {
                    var proposalHistory = item.AddProposalHistory(proposal);
                    foreach (var bid in proposal.bids)
                    {
                        if (bool.Parse(bid.blank.ToString())) continue;
                        proposalHistory.AddBidHistory(bid);
                    }
                }
            }
            return null;
        }

        [HttpPost]
        public ActionResult GetBidHistory(dynamic itemGroup)
        {
            var item = _projectRepository.GetProjectItem((int)itemGroup.itemIds[0]);
            if (!item.ProposalHistories.Any())
            {
                return new DqeResult(null);
            }
            var maxBidders = item.ProposalHistories.Max(i => i.BidHistories.Count()); 
            var proposals = new List<dynamic>();
            foreach (var p in item.ProposalHistories)
            {
                var blankCount = maxBidders - p.BidHistories.Count();
                var blanks = new List<object>();
                for (var i = 0; i < blankCount; i++)
                {
                    blanks.Add(new
                    {
                        blank = true,
                        price = (decimal) (dynamic)0,
                        include = false
                    });
                }
                var bids = p.BidHistories.Select(i => new
                {
                    blank = false,
                    price = i.Price,
                    include = i.IncludedInAverage
                }).Concat(blanks);
                proposals.Add(new
                {
                    proposal = p.ProposalNumber,
                    include = p.BidHistories.Any(i => i.IncludedInAverage),
                    letting = p.LettingDate.ToShortDateString(),
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
                        bids = new int[maxBidders].Select(ii => new
                        {
                            number = c += 1
                        })
                    },
                proposals
            };
            return new DqeResult(history);
        }
    }
}