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
        
        public EstimateController
            (
            IDqeUserRepository dqeUserRepository,
            IProjectRepository projectRepository,
            IWebTransportService webTransportService
            )
        {
            _dqeUserRepository = dqeUserRepository;
            _projectRepository = projectRepository;
            _webTransportService = webTransportService;
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
                key += 1;
                l.Add(new
                {
                    isFirst = true,
                    rowSpan = 1,
                    key,
                    itemId = itemGroup.ProjectItems.Count == 1 ? itemGroup.ProjectItems[0].Id : 0,
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
                    priceType = itemGroup.ProjectItems.First().PriceSet == PriceSetType.NotSet ? "N" : itemGroup.ProjectItems.First().PriceSet == PriceSetType.EstimatorOverride ? "O" : "S",
                    isSystemOverride = false
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
                            priceType = item.PriceSet == PriceSetType.NotSet ? "N" : item.PriceSet == PriceSetType.EstimatorOverride ? "O" : "S",
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
                    lettingDate =
                        wtProposal == null
                            ? string.Empty
                            : wtProposal.LettingDate.HasValue
                                ? wtProposal.LettingDate.Value.ToShortDateString()
                                : string.Empty,
                    designer = project.DesignerName
                },
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
                    lettingDate =
                        wtProposal == null
                            ? string.Empty
                            : wtProposal.LettingDate.HasValue
                                ? wtProposal.LettingDate.Value.ToShortDateString()
                                : string.Empty,
                    designer = project.DesignerName
                },
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
                    projectNumber = projects.Aggregate(string.Empty, (current, project) => current + string.Format("{0} ", project.ProjectNumber)).TrimEnd(' '),
                    itemProjectNumber = projects.Count == 1 ? projects[0].ProjectNumber : "Multiple",
                    priceType = itemGroup.ProjectItems.First().PriceSet == PriceSetType.NotSet ? "N" : itemGroup.ProjectItems.First().PriceSet == PriceSetType.EstimatorOverride ? "O" : "S",
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
                            projectNumber = projects.Aggregate(string.Empty, (current, project) => current + string.Format("{0} ", project.ProjectNumber)).TrimEnd(' '),
                            itemProjectNumber = item.MyEstimateGroup.MyProjectEstimate.MyProjectVersion.MyProject.ProjectNumber,
                            priceType = item.PriceSet == PriceSetType.NotSet ? "N" : item.PriceSet == PriceSetType.EstimatorOverride ? "O" : "S",
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
                total = proposal.GetEstimateTotal()
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
                    pit.PriceSet = PriceSetType.EstimatorOverride;
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
                    pit.PriceSet = PriceSetType.EstimatorOverride;
                }
                pi.Transform(pit, currentDqeUser);
            }
            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = "Estimate saved" }, JsonRequestBehavior.AllowGet);
        }
    }
}