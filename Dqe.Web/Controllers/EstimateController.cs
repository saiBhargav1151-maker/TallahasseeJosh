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
        
        public EstimateController
            (
            IDqeUserRepository dqeUserRepository,
            IProjectRepository projectRepository
            )
        {
            _dqeUserRepository = dqeUserRepository;
            _projectRepository = projectRepository;
        }

        [HttpGet]
        public ActionResult LoadEstimate()
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            if (!currentDqeUser.CanEstimateRecentProject() && !currentDqeUser.CanEstimateRecentProposal())
            {
                return new DqeResult(null, JsonRequestBehavior.AllowGet);
            }
            if (currentDqeUser.CanEstimateRecentProject() && !currentDqeUser.CanEstimateRecentProposal())
            {
                //show project estimate
                return CreateProjectEstimateStructure(currentDqeUser);
            }
            if (currentDqeUser.CanEstimateRecentProposal() && !currentDqeUser.CanEstimateRecentProject())
            {
                //show proposal estimate
                return CreateProposalEstimateStructure(currentDqeUser);
            }
            //show choice
            return CreateProposalProjectEstimateStructure(currentDqeUser);
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

        private DqeResult CreateProposalProjectEstimateStructure(DqeUser currentDqeUser)
        {
            var project = currentDqeUser.MyRecentProjectEstimate.MyProjectVersion.MyProject;
            var proposal = currentDqeUser.MyRecentProposal;
            return new DqeResult(new
            {
                estimateProject = true,
                estimateProposal = true,
                proposal = new
                {
                    id = proposal.Id,
                    number = proposal.ProposalNumber,
                    description = proposal.Description,
                },
                project = new
                {
                    id = project.Id,
                    number = project.ProjectNumber,
                    description = project.Description,
                    county = project.MyCounty.Name
                }
            }, JsonRequestBehavior.AllowGet);
        }

        private DqeResult CreateProjectEstimateStructure(DqeUser currentDqeUser)
        {
            //var isSynced = currentDqeUser.MyRecentProjectEstimate.IsSyncedWithWt();

            var project = currentDqeUser.MyRecentProjectEstimate.MyProjectVersion.MyProject;
            return new DqeResult(new
            {
                estimateProject = true,
                estimateProposal = false,
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
                    lettingDate = project.LettingDate,
                    designer = project.DesignerName
                },
                sets = currentDqeUser
                    .MyRecentProjectEstimate
                    .EstimateGroups
                    .Select(i => new
                    {
                        id =   i.AlternateSet,
                        member = i.AlternateMember
                    })
                    .Distinct()
                    .OrderBy(i => !string.IsNullOrWhiteSpace(string.Format("{0}{1}", i.id, i.member)) ? "A" : "B")
                    .Select(i => new
                    {
                        i.id, 
                        i.member,
                        groups = currentDqeUser
                            .MyRecentProjectEstimate
                            .EstimateGroups
                            .Where(ii => ii.AlternateSet == i.id && ii.AlternateMember == i.member)
                            .OrderBy(ii => ii.Name)
                            .ThenBy(ii => ii.Description)
                            .Select(iii => new
                            {
                                id = iii.Id,
                                description = string.Format("{0} {1}", iii.Name, iii.Description),
                                alternate = iii.AlternateSet,
                                alternateMember = iii.AlternateMember,
                                combine = iii.CombineWithLikeItems ? "Yes" : "No",
                                payItems = iii.ProjectItems.OrderBy(iiii => iiii.PayItemNumber).Select(iiii => new
                                {
                                    id = iiii.Id,
                                    number = iiii.PayItemNumber,
                                    description = iiii.PayItemDescription,
                                    alternate = iiii.AlternateSet,
                                    alternateMember = iiii.AlternateMember,
                                    quantity = iiii.Quantity,
                                    price = iiii.Price,
                                    lumpSum = iiii.IsLumpSum,
                                    unit = iiii.CalculatedUnit,
                                    combine = iiii.CombineWithLikeItems ? "Yes" : "No",
                                    contractClass = iiii.ContractClass,
                                    itemClass = iiii.ItemClass,
                                }).OrderBy(iiii => iiii.alternate).ThenBy(iiii => iiii.number)
                            })
                    }),
                //groups = currentDqeUser
                //    .MyRecentProjectEstimate
                //    .EstimateGroups
                //    .OrderBy(i => !string.IsNullOrWhiteSpace(i.AlternateSet) ? "A" : "B")
                //    .ThenBy(i => i.AlternateSet)
                //    .ThenBy(i => i.AlternateMember)
                //    .ThenBy(i => i.Name)
                //    .ThenBy(i => i.Description)
                //    .Select(i => new
                //    {
                //        id = i.Id,
                //        description = string.Format("{0} {1}", i.Name, i.Description),
                //        alternate = string.Format("{0}{1}", i.AlternateSet, i.AlternateMember),
                //        combine = i.CombineWithLikeItems ? "Yes" : "No",
                //        payItems = i.ProjectItems.OrderBy(ii => ii.PayItemNumber).Select(ii => new
                //        {
                //            id = ii.Id,
                //            number = ii.PayItemNumber,
                //            description = ii.PayItemDescription,
                //            alternate = string.Format("{0}{1}", ii.AlternateSet, ii.AlternateMember),
                //            quantity = ii.Quantity,
                //            price = ii.Price,
                //            lumpSum = ii.IsLumpSum,
                //            unit = ii.CalculatedUnit,
                //            combine = ii.CombineWithLikeItems ? "Yes" : "No",
                //            contractClass = ii.ContractClass,
                //            itemClass = ii.ItemClass,
                //        }).OrderBy(ii => ii.alternate).ThenBy(ii => ii.number)
                //    })
            }, JsonRequestBehavior.AllowGet);
        }

        private DqeResult CreateProposalEstimateStructure(DqeUser currentDqeUser)
        {
            var proposal = currentDqeUser.MyRecentProposal;
            var estimateGroups = proposal.GetEstimateGroups(currentDqeUser).ToList();
            return new DqeResult(new
            {
                estimateProject = false,
                estimateProposal = true,
                proposal = new
                {
                    id = proposal.Id,
                    number= proposal.ProposalNumber,
                    description = proposal.Description
                },
                project = new
                {
                    id = 0
                },
                groups = estimateGroups.Select(i => new
                {
                    id = i.Id,
                    description = i.Description,
                    payItems = i.ProjectItems.Select(ii => new
                    {
                        id = ii.Id,
                        number = ii.PayItemNumber,
                        description = ii.PayItemDescription,
                        quantity = ii.Quantity,
                        price = ii.Price,
                        lumpSum = ii.IsLumpSum,
                        unit = ii.CalculatedUnit,
                        combine = ii.CombineWithLikeItems,
                        contractClass = ii.ContractClass,
                        itemClass = ii.ItemClass,
                    })
                })
            }, JsonRequestBehavior.AllowGet);
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
            //var t = project.GetTransformer();
            //project.Transform(t, currentDqeUser);
            foreach (var set in estimate.sets)
            {
                foreach (var group in set.groups)
                {
                    var g = group;
                    var eg = currentDqeUser.MyRecentProjectEstimate.EstimateGroups.SingleOrDefault(i => i.Id == (int)g.id);
                    if (eg == null) continue;
                    foreach (var payItem in g.payItems)
                    {
                        var p = payItem;
                        var pi = eg.ProjectItems.SingleOrDefault(i => i.Id == (int)p.id);
                        if (pi == null) continue;
                        var pit = pi.GetTransformer();
                        pit.Price = (decimal)p.price;
                        pi.Transform(pit, currentDqeUser);
                    }
                }    
            }
            return new DqeResult(null, new ClientMessage{ Severity = ClientMessageSeverity.Success, text = "Estimate saved"}, JsonRequestBehavior.AllowGet);
        }
    }
}